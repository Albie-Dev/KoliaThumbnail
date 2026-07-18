using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Reflection;
using System.Xml.Linq;

namespace Kolia.Thumbnail.API.OpenApi
{
    public class EnumDescriptionsTransformer : IOpenApiSchemaTransformer
    {
        private readonly XElement? _xmlDoc;

        public EnumDescriptionsTransformer()
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                _xmlDoc = XElement.Load(xmlPath);
            }
        }

        public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
        {
            if (_xmlDoc == null) return Task.CompletedTask;

            // Xử lý enum schemas - thêm descriptions cho từng enum value
            if (schema.Enum != null && schema.Enum.Count > 0)
            {
                var enumDescriptions = new List<string>();
                var enumType = FindEnumType();

                for (int i = 0; i < schema.Enum.Count; i++)
                {
                    var enumValue = schema.Enum[i];
                    if (enumValue != null)
                    {
                        // Lấy giá trị enum
                        var stringValue = enumValue.ToString()?.Replace("\"", "").Replace("'", "");
                        var fieldName = stringValue;

                        if (!string.IsNullOrEmpty(fieldName))
                        {
                            // Tìm tất cả XML comments có tên field này
                            var fieldComments = _xmlDoc.Descendants("member")
                                .Where(x => x.Attribute("name")?.Value.StartsWith("F:") == true
                                          && x.Attribute("name")?.Value.EndsWith($".{fieldName}") == true)
                                .ToList();

                            foreach (var fieldComment in fieldComments)
                            {
                                var summary = fieldComment.Element("summary")?.Value.Trim();
                                if (!string.IsNullOrEmpty(summary))
                                {
                                    // Lấy giá trị số của enum nếu có thể
                                    var enumIntValue = GetEnumIntValue(enumType, fieldName, i);
                                    enumDescriptions.Add(enumIntValue.HasValue
                                        ? $"- {enumIntValue.Value} - {fieldName} : {summary}"
                                        : $"- {fieldName} : {summary}");
                                    break; // Chỉ lấy description đầu tiên
                                }
                            }
                        }
                    }
                }

                // Thêm enum descriptions vào schema.Description để hiển thị trong Scalar/ReDoc
                if (enumDescriptions.Count > 0)
                {
                    var enumText = "Available values:\n" + string.Join("\n", enumDescriptions);
                    schema.Description = enumText;

                    // Xóa schema.Enum để tránh hiển thị danh sách values raw trùng lặp
                    schema.Enum = null;
                }
            }

            return Task.CompletedTask;
        }

        private Type? FindEnumType()
        {
            try
            {
                // Tìm enum type từ XML documentation
                var enumTypeName = _xmlDoc?.Descendants("member")
                    .Where(x => x.Attribute("name")?.Value.StartsWith("F:") == true)
                    .Select(x => x.Attribute("name")?.Value)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(enumTypeName))
                {
                    // Parse type name từ format "F:Namespace.EnumType.FieldName"
                    var parts = enumTypeName.Split('.');
                    if (parts.Length >= 3)
                    {
                        var namespaceParts = parts.Take(parts.Length - 2).ToArray();
                        var typeName = parts[^2];
                        var fullTypeName = string.Join(".", namespaceParts) + "." + typeName;

                        return Type.GetType(fullTypeName);
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        private static int? GetEnumIntValue(Type? enumType, string fieldName, int index)
        {
            if (enumType == null) return index;

            try
            {
                var field = enumType.GetField(fieldName);
                if (field != null)
                {
                    var enumValue = field.GetRawConstantValue();
                    if (enumValue is int intValue)
                    {
                        return intValue;
                    }
                }
            }
            catch
            {
                // Fallback to index
            }

            return index;
        }
    }
}