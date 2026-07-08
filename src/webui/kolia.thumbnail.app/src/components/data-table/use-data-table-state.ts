import { parseAsInteger, parseAsString, useQueryState } from 'nuqs'

export type SortOrder = 'asc' | 'desc' | null

export function useDataTableState(initialPage = 1, initialPageSize = 10) {
  const [page, setPage] = useQueryState('page', parseAsInteger.withDefault(initialPage))
  const [pageSize, setPageSize] = useQueryState('size', parseAsInteger.withDefault(initialPageSize))
  const [search, setSearch] = useQueryState('search', parseAsString.withDefault(''))
  const [sortBy, setSortBy] = useQueryState('sortBy', parseAsString.withDefault(''))
  const [sortOrder, setSortOrder] = useQueryState('sortOrder', parseAsString.withDefault(''))

  const handleSort = (column: string) => {
    if (sortBy === column) {
      // Toggle sort order
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')
    } else {
      // New column, default to asc
      setSortBy(column)
      setSortOrder('asc')
    }
    setPage(1) // Reset to first page
  }

  return {
    page,
    setPage,
    pageSize,
    setPageSize,
    search,
    setSearch,
    sortBy,
    setSortBy,
    sortOrder: (sortOrder as SortOrder) || null,
    setSortOrder,
    handleSort,
  }
}
