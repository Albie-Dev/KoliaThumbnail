import type { PagedRequestParams } from '../../types/paging.types'

export function buildPagedQuery(params: PagedRequestParams): URLSearchParams {
  const query = new URLSearchParams()

  if (params.pageNumber !== undefined) {
    query.append('PageNumber', String(params.pageNumber))
  }
  if (params.pageSize !== undefined) {
    query.append('PageSize', String(params.pageSize))
  }
  if (params.includeTotalCount !== undefined) {
    query.append('IncludeTotalCount', String(params.includeTotalCount))
  }
  if (params.includeItems !== undefined) {
    query.append('IncludeItems', String(params.includeItems))
  }
  if (params.searchText) {
    query.append('SearchText', params.searchText)
  }

  // Sorts: Sorts[i].Field, Sorts[i].Direction
  if (params.sorts) {
    params.sorts.forEach((sort, index) => {
      query.append(`Sorts[${index}].Field`, sort.field)
      query.append(`Sorts[${index}].Direction`, String(sort.direction))
    })
  }

  // Filters: Filters[i].Field, Filters[i].Operator, Filters[i].Values[j], Filters[i].LogicalOperator
  if (params.filters) {
    params.filters.forEach((filter, index) => {
      query.append(`Filters[${index}].Field`, filter.field)
      query.append(`Filters[${index}].Operator`, String(filter.operator))
      if (filter.values) {
        filter.values.forEach((val) => {
          query.append(`Filters[${index}].Values`, String(val))
        })
      }
      if (filter.logicalOperator !== undefined) {
        query.append(`Filters[${index}].LogicalOperator`, String(filter.logicalOperator))
      }
    })
  }

  // RangeFilters: RangeFilters[i].Field, RangeFilters[i].From, RangeFilters[i].To, RangeFilters[i].LogicalOperator
  if (params.rangeFilters) {
    params.rangeFilters.forEach((rf, index) => {
      query.append(`RangeFilters[${index}].Field`, rf.field)
      if (rf.from !== undefined && rf.from !== null) {
        query.append(`RangeFilters[${index}].From`, String(rf.from))
      }
      if (rf.to !== undefined && rf.to !== null) {
        query.append(`RangeFilters[${index}].To`, String(rf.to))
      }
      if (rf.logicalOperator !== undefined) {
        query.append(`RangeFilters[${index}].LogicalOperator`, String(rf.logicalOperator))
      }
    })
  }

  return query
}
