using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Kolia.Thumbnail.API.Attributes;
using Kolia.Thumbnail.API.Models.Commons;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Extensions
{
    internal static partial class QueryableExtensions
    {
        private sealed class QueryPropertyMetadata
        {
            public required PropertyInfo Property { get; init; }

            public required QueryableAttribute Queryable { get; init; }

            public string Name => Property.Name;

            public Type PropertyType =>
                Nullable.GetUnderlyingType(Property.PropertyType) ?? Property.PropertyType;
        }

        private sealed class QueryEntityMetadata
        {
            public required IReadOnlyDictionary<string, QueryPropertyMetadata> Properties { get; init; }

            public required IReadOnlyList<QueryPropertyMetadata> SearchableProperties { get; init; }

            public required IReadOnlyDictionary<string, QueryPropertyMetadata> FilterableProperties { get; init; }

            public required IReadOnlyDictionary<string, QueryPropertyMetadata> SortableProperties { get; init; }

            /// <summary>
            /// Các property được phép filter theo khoảng giá trị (From/To).
            /// Chỉ chứa các property có <see cref="QueryableAttribute.RangeFilterable"/> = <c>true</c>
            /// Và kiểu thuộc <see cref="_rangeComparableTypes"/>.
            /// </summary>
            public required IReadOnlyDictionary<string, QueryPropertyMetadata> RangeFilterableProperties { get; init; }
        }

        private static readonly ConcurrentDictionary<Type, QueryEntityMetadata> _metadataCache = new();

        /// <summary>
        /// Tập hợp các kiểu dữ liệu hỗ trợ toán tử so sánh (<c>&gt;=</c>, <c>&lt;=</c>).
        /// Đây là nguồn thật duy nhất cho việc detect range-comparable type —
        /// không hard-code ở từng call-site.
        /// Muốn bổ sung kiểu mới chỉ cần thêm vào set này.
        /// </summary>
        private static readonly HashSet<Type> _rangeComparableTypes =
        [
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(DateOnly),
            typeof(TimeOnly)
        ];

        /// <summary>
        /// Kiểm tra một kiểu có hỗ trợ range filter không (dựa trên <see cref="_rangeComparableTypes"/>).
        /// Tự động unwrap <c>Nullable&lt;T&gt;</c> trước khi kiểm tra.
        /// </summary>
        private static bool IsRangeComparableType(Type type) =>
            _rangeComparableTypes.Contains(
                Nullable.GetUnderlyingType(type) ?? type);

        private static QueryEntityMetadata GetMetadata<TEntity>()
        {
            return GetMetadata(typeof(TEntity));
        }

        private static QueryEntityMetadata GetMetadata(Type type)
        {
            return _metadataCache.GetOrAdd(type, BuildMetadata);
        }

        private static QueryEntityMetadata BuildMetadata(Type type)
        {
            List<QueryPropertyMetadata> properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => new
                {
                    Property = property,
                    Attribute = property.GetCustomAttribute<QueryableAttribute>(true)
                })
                .Where(x => x.Attribute is not null)
                .Select(x => new QueryPropertyMetadata
                {
                    Property = x.Property,
                    Queryable = x.Attribute!
                })
                .ToList();

            return new QueryEntityMetadata
            {
                Properties = properties.ToDictionary(
                    x => x.Name,
                    StringComparer.OrdinalIgnoreCase),

                SearchableProperties = properties
                    .Where(x => x.Queryable.Searchable)
                    .OrderBy(x => x.Queryable.SearchOrder)
                    .ToList(),

                FilterableProperties = properties
                    .Where(x => x.Queryable.Filterable)
                    .ToDictionary(
                        x => x.Name,
                        StringComparer.OrdinalIgnoreCase),

                SortableProperties = properties
                    .Where(x => x.Queryable.Sortable)
                    .ToDictionary(
                        x => x.Name,
                        StringComparer.OrdinalIgnoreCase),

                RangeFilterableProperties = properties
                    .Where(x => x.Queryable.RangeFilterable && IsRangeComparableType(x.PropertyType))
                    .ToDictionary(
                        x => x.Name,
                        StringComparer.OrdinalIgnoreCase)
            };
        }


        public static IQueryable<TEntity> ApplySearch<TEntity>(
            this IQueryable<TEntity> query,
            string? searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return query;
            }

            QueryEntityMetadata metadata = GetMetadata<TEntity>();

            if (metadata.SearchableProperties.Count == 0)
            {
                return query;
            }

            ParameterExpression parameter = Expression.Parameter(
                typeof(TEntity),
                "x");

            Expression? searchExpression = null;

            foreach (QueryPropertyMetadata property in metadata.SearchableProperties)
            {
                Expression propertyExpression = Expression.Property(
                    parameter,
                    property.Property);

                Expression? currentExpression = BuildSearchExpression(
                    propertyExpression,
                    property.PropertyType,
                    searchText,
                    property.Queryable.IgnoreCase);

                if (currentExpression is null)
                {
                    continue;
                }

                searchExpression = searchExpression is null
                    ? currentExpression
                    : Expression.OrElse(
                        searchExpression,
                        currentExpression);
            }

            if (searchExpression is null)
            {
                return query;
            }

            Expression<Func<TEntity, bool>> lambda =
                Expression.Lambda<Func<TEntity, bool>>(
                    searchExpression,
                    parameter);

            return query.Where(lambda);
        }

        private static Expression? BuildSearchExpression(
            Expression propertyExpression,
            Type propertyType,
            string searchText,
            bool ignoreCase)
        {
            if (propertyType == typeof(string))
            {
                return BuildStringContainsExpression(
                    propertyExpression,
                    searchText,
                    ignoreCase);
            }

            if (propertyType == typeof(Guid))
            {
                if (!Guid.TryParse(searchText, out Guid value))
                {
                    return null;
                }

                return Expression.Equal(
                    propertyExpression,
                    Expression.Constant(value));
            }

            if (propertyType.IsEnum)
            {
                if (!Enum.TryParse(
                    propertyType,
                    searchText,
                    true,
                    out object? value))
                {
                    return null;
                }

                return Expression.Equal(
                    propertyExpression,
                    Expression.Constant(value));
            }

            return null;
        }

        private static Expression BuildStringContainsExpression(
            Expression propertyExpression,
            string searchText,
            bool ignoreCase)
        {
            MethodInfo containsMethod = typeof(string)
                .GetMethod(
                    nameof(string.Contains),
                    new[] { typeof(string) })!;

            if (!ignoreCase)
            {
                return Expression.Call(
                    propertyExpression,
                    containsMethod,
                    Expression.Constant(searchText));
            }

            MethodInfo toLowerMethod = typeof(string)
                .GetMethod(
                    nameof(string.ToLower),
                    Type.EmptyTypes)!;

            return Expression.Call(
                Expression.Call(
                    propertyExpression,
                    toLowerMethod),
                containsMethod,
                Expression.Constant(searchText.ToLower()));
        }

        public static IQueryable<TEntity> ApplyFilters<TEntity>(
            this IQueryable<TEntity> query,
            IReadOnlyCollection<FilterRequestDto>? filters)
        {
            if (filters is null || filters.Count == 0)
            {
                return query;
            }

            QueryEntityMetadata metadata = GetMetadata<TEntity>();

            ParameterExpression parameter = Expression.Parameter(
                typeof(TEntity),
                "x");

            Expression? finalExpression = null;

            foreach (FilterRequestDto filter in filters)
            {
                if (!metadata.FilterableProperties.TryGetValue(
                        filter.Field,
                        out QueryPropertyMetadata? property))
                {
                    continue;
                }

                Expression? expression = BuildFilterExpression(
                    parameter,
                    property,
                    filter);

                if (expression is null)
                {
                    continue;
                }

                if (finalExpression is null)
                {
                    finalExpression = expression;
                    continue;
                }

                finalExpression = filter.LogicalOperator == CLogicalOperator.Or
                    ? Expression.OrElse(finalExpression, expression)
                    : Expression.AndAlso(finalExpression, expression);
            }

            if (finalExpression is null)
            {
                return query;
            }

            Expression<Func<TEntity, bool>> lambda =
                Expression.Lambda<Func<TEntity, bool>>(
                    finalExpression,
                    parameter);

            return query.Where(lambda);
        }

        private static Expression? BuildFilterExpression(
            ParameterExpression parameter,
            QueryPropertyMetadata property,
            FilterRequestDto filter)
        {
            MemberExpression member = Expression.Property(
                parameter,
                property.Property);

            Type type = property.PropertyType;

            // Operators that don't need a value.
            if (filter.Operator == CFilterOperator.IsNull)
            {
                return IsNullableType(member.Type)
                    ? Expression.Equal(member, Expression.Constant(null, member.Type))
                    : null;
            }

            if (filter.Operator == CFilterOperator.IsNotNull)
            {
                return IsNullableType(member.Type)
                    ? Expression.NotEqual(member, Expression.Constant(null, member.Type))
                    : null;
            }

            if (filter.Operator == CFilterOperator.Between)
            {
                return filter.Values is { Count: >= 2 }
                    ? BuildBetweenExpression(member, type, filter.Values)
                    : null;
            }

            if (filter.Operator == CFilterOperator.In ||
                filter.Operator == CFilterOperator.NotIn)
            {
                return BuildCollectionExpression(
                    member,
                    type,
                    filter.Values ?? [],
                    filter.Operator == CFilterOperator.NotIn);
            }

            // All remaining operators require exactly one value.
            if (filter.Values is null || filter.Values.Count == 0)
            {
                return null;
            }

            JsonElement value = filter.Values.First();

            return filter.Operator switch
            {
                CFilterOperator.Equal =>
                    BuildEqualExpression(member, type, value),

                CFilterOperator.NotEqual =>
                    BuildNotEqualExpression(member, type, value),

                CFilterOperator.GreaterThan =>
                    BuildCompareExpression(
                        member,
                        type,
                        value,
                        ExpressionType.GreaterThan),

                CFilterOperator.GreaterThanOrEqual =>
                    BuildCompareExpression(
                        member,
                        type,
                        value,
                        ExpressionType.GreaterThanOrEqual),

                CFilterOperator.LessThan =>
                    BuildCompareExpression(
                        member,
                        type,
                        value,
                        ExpressionType.LessThan),

                CFilterOperator.LessThanOrEqual =>
                    BuildCompareExpression(
                        member,
                        type,
                        value,
                        ExpressionType.LessThanOrEqual),

                CFilterOperator.Contains =>
                    BuildStringMethodExpression(
                        member,
                        nameof(string.Contains),
                        value),

                CFilterOperator.StartsWith =>
                    BuildStringMethodExpression(
                        member,
                        nameof(string.StartsWith),
                        value),

                CFilterOperator.EndsWith =>
                    BuildStringMethodExpression(
                        member,
                        nameof(string.EndsWith),
                        value),

                _ => null
            };
        }

        private static Expression BuildEqualExpression(
            Expression member,
            Type type,
            JsonElement value)
        {
            return Expression.Equal(
                member,
                Expression.Constant(
                    ConvertValue(value, type),
                    member.Type));
        }

        private static Expression BuildNotEqualExpression(
            Expression member,
            Type type,
            JsonElement value)
        {
            return Expression.NotEqual(
                member,
                Expression.Constant(
                    ConvertValue(value, type),
                    member.Type));
        }

        private static Expression BuildCompareExpression(
            Expression member,
            Type type,
            JsonElement value,
            ExpressionType expressionType)
        {
            return Expression.MakeBinary(
                expressionType,
                member,
                Expression.Constant(
                    ConvertValue(value, type),
                    member.Type));
        }

        private static Expression? BuildStringMethodExpression(
            Expression member,
            string method,
            JsonElement value)
        {
            if (member.Type != typeof(string))
            {
                return null;
            }

            if (value.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            return Expression.Call(
                member,
                typeof(string).GetMethod(
                    method,
                    new[] { typeof(string) })!,
                Expression.Constant(
                    value.GetString()));
        }

        private static Expression BuildCollectionExpression(
            Expression member,
            Type type,
            IEnumerable<JsonElement> values,
            bool negate)
        {
            // Use the member's actual type (which may be Nullable<T>) so the
            // generic Enumerable.Contains<T> call type-matches the member
            // expression exactly.
            Type elementType = member.Type;

            IReadOnlyList<JsonElement> valueList = values as IReadOnlyList<JsonElement>
                ?? values.ToList();

            Array array = Array.CreateInstance(
                elementType,
                valueList.Count);

            for (int index = 0; index < valueList.Count; index++)
            {
                array.SetValue(
                    ConvertValue(valueList[index], type),
                    index);
            }

            Expression contains = Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Contains),
                new[] { elementType },
                Expression.Constant(array),
                member);

            return negate
                ? Expression.Not(contains)
                : contains;
        }

        private static Expression? BuildBetweenExpression(
            Expression member,
            Type type,
            IReadOnlyList<JsonElement> values)
        {
            if (values.Count < 2)
            {
                return null;
            }

            Expression greater =
                Expression.GreaterThanOrEqual(
                    member,
                    Expression.Constant(
                        ConvertValue(values[0], type),
                        member.Type));

            Expression less =
                Expression.LessThanOrEqual(
                    member,
                    Expression.Constant(
                        ConvertValue(values[1], type),
                        member.Type));

            return Expression.AndAlso(
                greater,
                less);
        }

        /// <summary>
        /// Áp dụng các điều kiện lọc theo khoảng giá trị (<c>&gt;=</c> / <c>&lt;=</c>) vào query.
        /// Hỗ trợ partial range: chỉ cần truyền <c>From</c> hoặc <c>To</c>.
        /// Property phải được đánh dấu <see cref="QueryableAttribute.RangeFilterable"/> = <c>true</c>
        /// và kiểu phải thuộc các kiểu so sánh được hỗ trợ (<see cref="_rangeComparableTypes"/>).
        /// </summary>
        public static IQueryable<TEntity> ApplyRangeFilters<TEntity>(
            this IQueryable<TEntity> query,
            IReadOnlyCollection<RangeFilterRequestDto>? rangeFilters)
        {
            if (rangeFilters is null || rangeFilters.Count == 0)
            {
                return query;
            }

            QueryEntityMetadata metadata = GetMetadata<TEntity>();

            ParameterExpression parameter = Expression.Parameter(
                typeof(TEntity),
                "x");

            Expression? finalExpression = null;

            foreach (RangeFilterRequestDto filter in rangeFilters)
            {
                if (!metadata.RangeFilterableProperties.TryGetValue(
                        filter.Field,
                        out QueryPropertyMetadata? property))
                {
                    continue;
                }

                Expression? expression = BuildRangeExpression(
                    parameter,
                    property,
                    filter);

                if (expression is null)
                {
                    continue;
                }

                finalExpression = finalExpression is null
                    ? expression
                    : filter.LogicalOperator == CLogicalOperator.Or
                        ? Expression.OrElse(finalExpression, expression)
                        : Expression.AndAlso(finalExpression, expression);
            }

            if (finalExpression is null)
            {
                return query;
            }

            Expression<Func<TEntity, bool>> lambda =
                Expression.Lambda<Func<TEntity, bool>>(
                    finalExpression,
                    parameter);

            return query.Where(lambda);
        }

        /// <summary>
        /// Xây dựng expression so sánh khoảng cho một property.
        /// Hỗ trợ đầy đủ 4 trường hợp:
        /// <list type="bullet">
        /// <item><c>From</c> + <c>To</c>: <c>member &gt;= from AND member &lt;= to</c></item>
        /// <item><c>From</c> only: <c>member &gt;= from</c></item>
        /// <item><c>To</c> only: <c>member &lt;= to</c></item>
        /// <item>Cả hai null: trả về <c>null</c> (bỏ qua filter này)</item>
        /// </list>
        /// </summary>
        private static Expression? BuildRangeExpression(
            ParameterExpression parameter,
            QueryPropertyMetadata property,
            RangeFilterRequestDto filter)
        {
            MemberExpression member = Expression.Property(
                parameter,
                property.Property);

            Type type = property.PropertyType;

            Expression? lower = BuildBoundExpression(
                member,
                type,
                filter.From,
                ExpressionType.GreaterThanOrEqual);

            Expression? upper = BuildBoundExpression(
                member,
                type,
                filter.To,
                ExpressionType.LessThanOrEqual);

            return (lower, upper) switch
            {
                (not null, not null) => Expression.AndAlso(lower, upper),
                (not null, null)     => lower,
                (null, not null)     => upper,
                _                    => null
            };
        }

        /// <summary>
        /// Xây dựng một cạnh của range expression (cận dưới hoặc cận trên).
        /// Trả về <c>null</c> khi giá trị không được truyền hoặc null/rỗng.
        /// Nhận <c>string?</c> vì <see cref="RangeFilterRequestDto.From"/> và <see cref="RangeFilterRequestDto.To"/>
        /// được bind từ query string qua <c>[FromQuery]</c>.
        /// </summary>
        private static Expression? BuildBoundExpression(
            MemberExpression member,
            Type type,
            string? bound,
            ExpressionType comparisonType)
        {
            if (string.IsNullOrWhiteSpace(bound))
            {
                return null;
            }

            object? converted = null;

            if (type == typeof(DateTimeOffset))
            {
                if (DateTimeOffset.TryParse(bound, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
                {
                    converted = dto;
                }
                else if (DateTimeOffset.TryParse(bound, out var dto2))
                {
                    converted = dto2;
                }
            }
            else if (type == typeof(DateTime))
            {
                if (DateTime.TryParse(bound, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
                {
                    converted = dt;
                }
                else if (DateTime.TryParse(bound, out var dt2))
                {
                    converted = dt2;
                }
            }

            if (converted is null)
            {
                try
                {
                    converted = ConvertValue(bound, type);
                }
                catch
                {
                    return null;
                }
            }

            if (converted is null)
            {
                return null;
            }

            return Expression.MakeBinary(
                comparisonType,
                member,
                Expression.Constant(converted, member.Type));
        }

        public static IQueryable<TEntity> ApplySorting<TEntity>(
            this IQueryable<TEntity> query,
            IReadOnlyCollection<SortRequestDto>? sorts)
        {
            var (result, _) = ApplySortingCore(query, sorts);
            return result;
        }

        /// <summary>
        /// Áp dụng sort và trả về flag cho biết có sort nào được apply không.
        /// </summary>
        private static (IQueryable<TEntity> Query, bool WasSorted) ApplySortingCore<TEntity>(
            IQueryable<TEntity> query,
            IReadOnlyCollection<SortRequestDto>? sorts)
        {
            if (sorts is null || sorts.Count == 0)
            {
                return (query, false);
            }

            QueryEntityMetadata metadata = GetMetadata<TEntity>();

            bool firstSort = true;

            foreach (SortRequestDto sort in sorts)
            {
                if (!metadata.SortableProperties.TryGetValue(
                        sort.Field,
                        out QueryPropertyMetadata? property))
                {
                    continue;
                }

                query = ApplyOrder(
                    query,
                    property.Property,
                    sort.Direction,
                    firstSort);

                firstSort = false;
            }

            // firstSort vẫn true có nghĩa không có field hợp lệ nào được apply
            return (query, !firstSort);
        }

        /// <summary>
        /// Áp dụng sort mặc định khi không có sort nào được truyền vào (hoặc tất cả field không hợp lệ).
        /// Ưu tiên sort theo <c>CreationTime DESC</c>; nếu entity không có thì dùng sortable property đầu tiên tìm được.
        /// Giúp tránh EF Core warning <i>Skip/Take without OrderBy</i>.
        /// </summary>
        private static IQueryable<TEntity> ApplyDefaultSorting<TEntity>(
            IQueryable<TEntity> query)
        {
            QueryEntityMetadata metadata = GetMetadata<TEntity>();

            if (metadata.SortableProperties.Count == 0)
            {
                return query;
            }

            // Ưu tiên CreationTime DESC (field chuẩn trên BaseEntity)
            if (metadata.SortableProperties.TryGetValue(
                    "CreationTime",
                    out QueryPropertyMetadata? creationTimeProp))
            {
                return ApplyOrder(query, creationTimeProp.Property, CSortDirection.Desc, true);
            }

            // Fallback: lấy sortable property đầu tiên theo thứ tự alphabetical
            QueryPropertyMetadata fallbackProp = metadata.SortableProperties
                .OrderBy(x => x.Key)
                .First()
                .Value;

            return ApplyOrder(query, fallbackProp.Property, CSortDirection.Asc, true);
        }

        private static IQueryable<TEntity> ApplyOrder<TEntity>(
            IQueryable<TEntity> source,
            PropertyInfo property,
            CSortDirection direction,
            bool firstSort)
        {
            ParameterExpression parameter =
                Expression.Parameter(
                    typeof(TEntity),
                    "x");

            Expression propertyAccess =
                Expression.Property(
                    parameter,
                    property);

            LambdaExpression lambda =
                Expression.Lambda(
                    propertyAccess,
                    parameter);

            string methodName;

            if (firstSort)
            {
                methodName = direction == CSortDirection.Desc
                    ? nameof(Queryable.OrderByDescending)
                    : nameof(Queryable.OrderBy);
            }
            else
            {
                methodName = direction == CSortDirection.Desc
                    ? nameof(Queryable.ThenByDescending)
                    : nameof(Queryable.ThenBy);
            }

            MethodInfo method =
                typeof(Queryable)
                    .GetMethods()
                    .First(x =>
                        x.Name == methodName &&
                        x.IsGenericMethodDefinition &&
                        x.GetGenericArguments().Length == 2 &&
                        x.GetParameters().Length == 2);

            MethodInfo genericMethod =
                method.MakeGenericMethod(
                    typeof(TEntity),
                    property.PropertyType);

            return (IQueryable<TEntity>)genericMethod.Invoke(
                null,
                new object[]
                {
                    source,
                    lambda
                })!;
        }

        public static IQueryable<TEntity> ApplyPaging<TEntity>(
            this IQueryable<TEntity> query,
            PagedRequestDto request)
        {
            int skip = (request.PageNumber - 1) * request.PageSize;

            return query
                .Skip(skip)
                .Take(request.PageSize);
        }

        public static IQueryable<TEntity> ApplyQuery<TEntity>(
            this IQueryable<TEntity> query,
            PagedRequestDto request)
        {
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                query = query.ApplySearch(request.SearchText);
            }

            if (request.Filters is { Count: > 0 })
            {
                query = query.ApplyFilters(request.Filters);
            }

            if (request.RangeFilters is { Count: > 0 })
            {
                query = query.ApplyRangeFilters(request.RangeFilters);
            }

            // Apply sorting; nếu không có sort hợp lệ nào được apply thì dùng default sort
            // để tránh EF Core warning: "Skip/Take without OrderBy".
            var (sortedQuery, wasSorted) = ApplySortingCore(query, request.Sorts);
            query = wasSorted ? sortedQuery : ApplyDefaultSorting(query);

            return query;
        }

        /// <summary>
        /// Áp dụng Skip/Take cho một query đã được filter + sort.
        /// Dùng trong <c>ToPagedResponseAsync</c> — tách khỏi <see cref="ApplyQuery"/>
        /// nhằm đảm bảo <c>CountAsync</c> luôn chạy trước paging, tránh total-records bị sai.
        /// </summary>
        private static IQueryable<TEntity> ApplyQueryPaging<TEntity>(
            this IQueryable<TEntity> query,
            PagedRequestDto request)
        {
            if (request.PageSize > 0)
            {
                query = query.ApplyPaging(request);
            }

            return query;
        }

        public static async Task<PagedResponseDto<TDestination>> ToPagedResponseAsync<TEntity, TDestination>(
            this IQueryable<TEntity> query,
            PagedRequestDto request,
            Func<TEntity, TDestination> selector,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(selector);

            int totalRecords = 0;

            if (request.IncludeTotalCount)
            {
                // Count trước — trên query đã filter/sort nhưng chưa Skip/Take
                totalRecords = await query.CountAsync(cancellationToken);
            }

            IReadOnlyList<TDestination> items = [];

            if (request.IncludeItems)
            {
                // Paging sau khi đã count xong
                var pagedQuery = query.ApplyQueryPaging(request);

                List<TEntity> entities = await pagedQuery
                    .ToListAsync(cancellationToken);

                items = entities
                    .Select(selector)
                    .ToList();
            }

            return new PagedResponseDto<TDestination>
            {
                Items = items,
                PageInfo = BuildPageInfo(
                    request,
                    totalRecords)
            };
        }

        public static async Task<PagedResponseDto<TEntity>> ToPagedResponseAsync<TEntity>(
            this IQueryable<TEntity> query,
            PagedRequestDto request,
            CancellationToken cancellationToken = default)
        {
            int totalRecords = 0;

            if (request.IncludeTotalCount)
            {
                totalRecords = await query.CountAsync(cancellationToken);
            }

            IReadOnlyList<TEntity> items = [];

            if (request.IncludeItems)
            {
                items = await query
                    .ToListAsync(cancellationToken);
            }

            return new PagedResponseDto<TEntity>
            {
                Items = items,
                PageInfo = BuildPageInfo(
                    request,
                    totalRecords)
            };
        }

        private static PageInfoDto BuildPageInfo(
            PagedRequestDto request,
            int totalRecords)
        {
            int totalPages = request.PageSize <= 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)request.PageSize);

            return new PageInfoDto
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < totalPages
            };
        }


        private static object? ConvertValue(
            JsonElement element,
            Type targetType)
        {
            Type type = Nullable.GetUnderlyingType(targetType)
                        ?? targetType;

            if (element.ValueKind == JsonValueKind.Null ||
                element.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            if (type == typeof(string))
            {
                return element.GetString();
            }

            if (type == typeof(Guid))
            {
                return Guid.Parse(
                    element.GetString()!);
            }

            if (type == typeof(DateTime))
            {
                return element.GetDateTime();
            }

            if (type == typeof(DateTimeOffset))
            {
                return element.GetDateTimeOffset();
            }

            if (type == typeof(bool))
            {
                return element.GetBoolean();
            }

            if (type == typeof(byte))
            {
                return element.GetByte();
            }

            if (type == typeof(short))
            {
                return element.GetInt16();
            }

            if (type == typeof(int))
            {
                return element.GetInt32();
            }

            if (type == typeof(long))
            {
                return element.GetInt64();
            }

            if (type == typeof(float))
            {
                return element.GetSingle();
            }

            if (type == typeof(double))
            {
                return element.GetDouble();
            }

            if (type == typeof(decimal))
            {
                return element.GetDecimal();
            }

            if (type.IsEnum)
            {
                if (element.ValueKind == JsonValueKind.String)
                {
                    return Enum.Parse(
                        type,
                        element.GetString()!,
                        true);
                }

                return Enum.ToObject(
                    type,
                    element.GetInt32());
            }

            if (type == typeof(byte[]))
            {
                return element.GetBytesFromBase64();
            }

            return JsonSerializer.Deserialize(
                element.GetRawText(),
                type,
                JsonSerializerOptions.Default);
        }

        private static object? ConvertValue(
            string value,
            Type targetType)
        {
            Type type = Nullable.GetUnderlyingType(targetType)
                        ?? targetType;

            if (type == typeof(string))
            {
                return value;
            }

            if (type == typeof(Guid))
            {
                return Guid.Parse(value);
            }

            if (type == typeof(DateTime))
            {
                return DateTime.Parse(
                    value,
                    CultureInfo.InvariantCulture);
            }

            if (type == typeof(DateTimeOffset))
            {
                return DateTimeOffset.Parse(
                    value,
                    CultureInfo.InvariantCulture);
            }

            if (type == typeof(bool))
            {
                return bool.Parse(value);
            }

            if (type.IsEnum)
            {
                return Enum.Parse(
                    type,
                    value,
                    true);
            }

            return Convert.ChangeType(
                value,
                type,
                CultureInfo.InvariantCulture);
        }

        private static bool TryConvertValue(
            JsonElement element,
            Type targetType,
            out object? value)
        {
            try
            {
                value = ConvertValue(
                    element,
                    targetType);

                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        private static Expression BuildPropertyExpression(
            ParameterExpression parameter,
            PropertyInfo property)
        {
            return Expression.Property(
                parameter,
                property);
        }

        private static ConstantExpression BuildConstantExpression(
            object? value,
            Type type)
        {
            return Expression.Constant(
                value,
                type);
        }

        private static Expression BuildNullExpression(
            Expression expression)
        {
            return Expression.Equal(
                expression,
                Expression.Constant(
                    null,
                    expression.Type));
        }

        private static bool IsNullableType(Type type)
        {
            return !type.IsValueType ||
                   Nullable.GetUnderlyingType(type) != null;
        }

        private static Type GetNonNullableType(Type type)
        {
            return Nullable.GetUnderlyingType(type)
                   ?? type;
        }

        private static bool IsStringType(Type type)
        {
            return GetNonNullableType(type) == typeof(string);
        }

        private static bool IsCollectionType(Type type)
        {
            return type != typeof(string) &&
                   typeof(System.Collections.IEnumerable)
                       .IsAssignableFrom(type);
        }

        private static MethodInfo GetQueryableMethod(
            string methodName,
            int parameterCount)
        {
            return typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(x =>
                    x.Name == methodName &&
                    x.IsGenericMethodDefinition &&
                    x.GetParameters().Length == parameterCount);
        }

        private static MethodInfo GetEnumerableMethod(
            string methodName,
            int parameterCount)
        {
            return typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(x =>
                    x.Name == methodName &&
                    x.IsGenericMethodDefinition &&
                    x.GetParameters().Length == parameterCount);
        }

        private static Expression CombineExpressions(
            Expression? left,
            Expression right,
            CLogicalOperator logicalOperator)
        {
            if (left is null)
            {
                return right;
            }

            return logicalOperator == CLogicalOperator.Or
                ? Expression.OrElse(left, right)
                : Expression.AndAlso(left, right);
        }

        private static string NormalizeSearchText(
            string value,
            bool ignoreCase)
        {
            return ignoreCase
                ? value.ToLowerInvariant()
                : value;
        }

        private static Expression BuildStringNormalizeExpression(
            Expression expression,
            bool ignoreCase)
        {
            if (!ignoreCase)
            {
                return expression;
            }

            MethodInfo method =
                typeof(string).GetMethod(
                    nameof(string.ToLowerInvariant),
                    Type.EmptyTypes)!;

            return Expression.Call(
                expression,
                method);
        }

        private static bool HasQueryableAttribute(
            PropertyInfo property)
        {
            return property.GetCustomAttribute<QueryableAttribute>(
                true) != null;
        }

        private static IEnumerable<PropertyInfo> GetQueryableProperties(
            Type type)
        {
            return type
                .GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public)
                .Where(HasQueryableAttribute);
        }
    }
}