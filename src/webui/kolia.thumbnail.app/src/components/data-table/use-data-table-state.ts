import { parseAsInteger, parseAsString, useQueryState } from 'nuqs'

export function useDataTableState(initialPage = 1, initialPageSize = 10) {
  const [page, setPage] = useQueryState('page', parseAsInteger.withDefault(initialPage))
  const [pageSize, setPageSize] = useQueryState('size', parseAsInteger.withDefault(initialPageSize))
  const [search, setSearch] = useQueryState('search', parseAsString.withDefault(''))

  return { page, setPage, pageSize, setPageSize, search, setSearch }
}
