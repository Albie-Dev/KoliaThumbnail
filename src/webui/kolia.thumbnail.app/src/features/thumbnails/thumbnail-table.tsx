import { useMemo, useState, useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Plus } from 'lucide-react'
import { DataTable } from '../../components/data-table/data-table'
import { useDataTableState } from '../../components/data-table/use-data-table-state'
import { Button } from '../../components/ui/button'
import { useSidebarContext } from '../../lib/sidebar-context'
import { formatDateTime } from '../../lib/date-formatter'
import { fetchThumbnails, type ThumbnailItem } from './api'
import { SortDirection, type SortRequestDto, type RangeFilterRequestDto } from '../../types/paging.types'
import { useQueryState, parseAsString } from 'nuqs'

export function ThumbnailTable() {
  const { open } = useSidebarContext()
  const { page, setPage, pageSize, setPageSize, search, setSearch, sortBy, sortOrder, handleSort } =
    useDataTableState(1, 10)

  // ── Sync applied filter values to URL query state ──────────────────────
  const [appliedCreationFrom, setAppliedCreationFrom] = useQueryState('creationFrom', parseAsString.withDefault(''))
  const [appliedCreationTo, setAppliedCreationTo] = useQueryState('creationTo', parseAsString.withDefault(''))

  // ── Local filter inputs state (controlled input fields in Sidebar) ──────
  const [filterCreationFrom, setFilterCreationFrom] = useState(appliedCreationFrom)
  const [filterCreationTo, setFilterCreationTo] = useState(appliedCreationTo)

  // ── Sync local inputs with URL query parameters (useful on page load / remote changes)
  useEffect(() => {
    setFilterCreationFrom(appliedCreationFrom)
  }, [appliedCreationFrom])

  useEffect(() => {
    setFilterCreationTo(appliedCreationTo)
  }, [appliedCreationTo])
  const backendSorts = useMemo<SortRequestDto[]>(() => {
    if (!sortBy || !sortOrder) return []
    let field = 'Name'
    if (sortBy === 'name') field = 'Name'
    if (sortBy === 'shortName') field = 'ShortName'
    if (sortBy === 'created') field = 'CreationTime'

    return [
      {
        field,
        direction: sortOrder === 'desc' ? SortDirection.Desc : SortDirection.Asc,
      },
    ]
  }, [sortBy, sortOrder])

  // Build Range Filters for backend
  const backendRangeFilters = useMemo<RangeFilterRequestDto[]>(() => {
    const list: RangeFilterRequestDto[] = []
    if (appliedCreationFrom || appliedCreationTo) {
      list.push({
        field: 'CreationTime',
        from: appliedCreationFrom ? new Date(appliedCreationFrom).toISOString() : null,
        to: appliedCreationTo ? new Date(appliedCreationTo).toISOString() : null,
      })
    }
    return list
  }, [appliedCreationFrom, appliedCreationTo])

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: [
      'thumbnails',
      page,
      pageSize,
      search,
      backendSorts,
      backendRangeFilters,
    ],
    queryFn: () =>
      fetchThumbnails({
        pageNumber: page,
        pageSize: pageSize,
        searchText: search || undefined,
        sorts: backendSorts,
        rangeFilters: backendRangeFilters,
      }),
  })

  const columns = useMemo(
    () => [
      { key: 'name', header: 'Tên', sortable: true, render: (item: ThumbnailItem) => item.name },
      { key: 'shortName', header: 'Mã', sortable: true, render: (item: ThumbnailItem) => item.shortName },
      {
        key: 'image',
        header: 'Logo',
        render: (item: ThumbnailItem) =>
          item.imageUrl ? (
            <img src={item.imageUrl} alt={item.name} className="h-8 w-8 rounded object-cover" />
          ) : (
            <span className="text-sm text-slate-400">—</span>
          ),
      },
      {
        key: 'created',
        header: 'Tạo lúc',
        sortable: true,
        render: (item: ThumbnailItem) => formatDateTime(item.creationTime),
      },
    ],
    [],
  )

  const handleApplyFilter = () => {
    setAppliedCreationFrom(filterCreationFrom)
    setAppliedCreationTo(filterCreationTo)
    setPage(1)
  }

  const handleResetFilter = () => {
    setFilterCreationFrom('')
    setFilterCreationTo('')
    setAppliedCreationFrom('')
    setAppliedCreationTo('')
    setPage(1)
  }

  const filterContent = (
    <div className="space-y-4">
      <div>
        <label className="block text-xs font-semibold text-slate-500 uppercase mb-2">Khoảng thời gian tạo</label>
        <div className="space-y-2">
          <div>
            <span className="text-xs text-slate-400 block mb-1">Từ ngày</span>
            <input
              type="datetime-local"
              value={filterCreationFrom}
              onChange={(e) => setFilterCreationFrom(e.target.value)}
              className="w-full rounded-md border border-slate-300 bg-white px-3 py-1 text-sm shadow-sm outline-none transition focus:border-slate-400"
            />
          </div>
          <div>
            <span className="text-xs text-slate-400 block mb-1">Đến ngày</span>
            <input
              type="datetime-local"
              value={filterCreationTo}
              onChange={(e) => setFilterCreationTo(e.target.value)}
              className="w-full rounded-md border border-slate-300 bg-white px-3 py-1 text-sm shadow-sm outline-none transition focus:border-slate-400"
            />
          </div>
        </div>
      </div>
    </div>
  )

  return (
    <DataTable
      title="AI Providers"
      columns={columns}
      data={data?.items ?? []}
      isLoading={isLoading}
      error={error instanceof Error ? error.message : null}
      onRetry={() => void refetch()}
      emptyMessage="Không có nhà cung cấp nào phù hợp."
      // ── Pagination ──────────────────────────────────────────
      page={data?.pageNumber ?? 1}
      pageSize={data?.pageSize ?? 10}
      totalPages={data?.totalPages ?? 1}
      totalCount={data?.totalCount ?? 0}
      onPageChange={(nextPage) => setPage(nextPage)}
      onPageSizeChange={(nextSize) => {
        setPageSize(nextSize)
        setPage(1)
      }}
      // ── Sort ────────────────────────────────────────────────
      sortBy={sortBy}
      sortOrder={sortOrder}
      onSort={handleSort}
      // ── Toolbar ─────────────────────────────────────────────
      actions={
        <Button onClick={() => open('create-ai-provider')} className="gap-2">
          <Plus className="h-4 w-4" />
          Thêm mới
        </Button>
      }
      search={search}
      searchPlaceholder="Nhập tên hoặc mã"
      onSearchChange={(value) => {
        setSearch(value)
        setPage(1)
      }}
      onSearchClear={() => {
        setSearch('')
        setPage(1)
      }}
      // filterContent không truyền → nút Filter ẩn
      filterContent={filterContent}
      onApplyFilter={handleApplyFilter}
      onResetFilter={handleResetFilter}
    />
  )
}
