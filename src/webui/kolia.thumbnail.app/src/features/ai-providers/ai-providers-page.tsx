import { useMemo, useState, useEffect, useCallback } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Pencil, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { DataTable } from '../../components/data-table/data-table'
import { useDataTableState } from '../../components/data-table/use-data-table-state'
import { Button } from '../../components/ui/button'
import { ConfirmDialog } from '../../components/ui/confirm-dialog'
import { DateTimePicker } from '../../components/ui/date-time-picker'
import { useSidebarContext } from '../../lib/sidebar-context'
import { formatDateTime } from '../../lib/date-formatter'
import { StatusFilterGroup, type StatusFilter } from '../../components/filters/status-filter'
import { fetchThumbnails, deleteAIProvider, type ThumbnailItem } from './api'
import { SortDirection, type SortRequestDto, type RangeFilterRequestDto } from '../../types/paging.types'
import { useQueryState, parseAsString } from 'nuqs'
import { ApiError } from '../../lib/api/api-error'

export function AiProvidersPage() {
  const { open } = useSidebarContext()
  const queryClient = useQueryClient()
  const { page, setPage, pageSize, setPageSize, search, setSearch, sortBy, sortOrder, handleSort } =
    useDataTableState(1, 10)

  // ── Status filter ──────────────────────────────────────────────────
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('active')
  const [localStatusFilter, setLocalStatusFilter] = useState<StatusFilter>('active')

  // ── Delete confirm state ─────────────────────────────────────────────
  const [deleteTarget, setDeleteTarget] = useState<ThumbnailItem | null>(null)

  const { mutate: remove, isPending: isDeleting } = useMutation({
    mutationFn: (id: string) => deleteAIProvider(id),
    onSuccess: () => {
      toast.success('Đã xoá nhà cung cấp thành công.')
      queryClient.invalidateQueries({ queryKey: ['ai-providers'] })
    },
    onError: (error) => {
      toast.error(error instanceof ApiError ? error.message : 'Có lỗi xảy ra khi xoá.')
    },
  })

  const handleDeleteConfirm = useCallback(() => {
    if (!deleteTarget) return
    remove(deleteTarget.id, {
      onSuccess: () => setDeleteTarget(null),
    })
  }, [deleteTarget, remove])

  // ── Sync applied filter values to URL query state ──────────────────────
  const [appliedCreationFrom, setAppliedCreationFrom] = useQueryState('creationFrom', parseAsString.withDefault(''))
  const [appliedCreationTo, setAppliedCreationTo] = useQueryState('creationTo', parseAsString.withDefault(''))

  // ── Local filter inputs state (controlled input fields in Sidebar) ──────
  const [filterCreationFrom, setFilterCreationFrom] = useState(appliedCreationFrom)
  const [filterCreationTo, setFilterCreationTo] = useState(appliedCreationTo)

  // ── Sync local status filter when applied status changes (e.g. sidebar opens) ──
  useEffect(() => {
    setLocalStatusFilter(statusFilter)
  }, [statusFilter])

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
    if (sortBy === 'creationTime') field = 'CreationTime'
    if (sortBy === 'lastModificationTime') field = 'LastModificationTime'

    return [
      {
        field,
        direction: sortOrder === 'desc' ? SortDirection.Desc : SortDirection.Asc,
      },
    ]
  }, [sortBy, sortOrder])

  // Build Filters for backend (status)
  // Không cần backendFilters cho IsDeleted nữa — dùng query params riêng
  const backendDeletedFilters = useMemo(() => {
    if (statusFilter === 'all') return { includeDeleted: true }
    if (statusFilter === 'deleted') return { includeDeleted: true, deletedOnly: true }
    return {} // active — global filter xử lý
  }, [statusFilter])

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
      'ai-providers',
      page,
      pageSize,
      search,
      statusFilter,
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
        includeDeleted: backendDeletedFilters.includeDeleted,
        deletedOnly: backendDeletedFilters.deletedOnly,
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
        key: 'creationTime',
        header: 'Tạo lúc',
        sortable: true,
        render: (item: ThumbnailItem) => formatDateTime(item.creationTime),
      },
      {
        key: 'lastModificationTime',
        header: 'Cập nhật lúc',
        sortable: true,
        render: (item: ThumbnailItem) => formatDateTime(item.lastModificationTime),
      },
      {
        key: 'actions',
        header: '',
        render: (item: ThumbnailItem) => (
          <div className="flex items-center gap-0.5">
            <button
              type="button"
              onClick={() => open({ type: 'edit-ai-provider', provider: item })}
              className="inline-flex items-center justify-center rounded-md p-1.5 text-slate-400 hover:bg-slate-100 hover:text-slate-700 transition-colors"
              title="Chỉnh sửa"
            >
              <Pencil className="h-4 w-4" />
            </button>
            <button
              type="button"
              onClick={() => setDeleteTarget(item)}
              className="inline-flex items-center justify-center rounded-md p-1.5 text-slate-400 hover:bg-red-50 hover:text-red-600 transition-colors"
              title="Xoá"
            >
              <Trash2 className="h-4 w-4" />
            </button>
          </div>
        ),
      },
    ],
    [open, setDeleteTarget],
  )

  const handleApplyFilter = () => {
    setAppliedCreationFrom(filterCreationFrom)
    setAppliedCreationTo(filterCreationTo)
    setStatusFilter(localStatusFilter)
    setPage(1)
  }

  const handleResetFilter = () => {
    setFilterCreationFrom('')
    setFilterCreationTo('')
    setAppliedCreationFrom('')
    setAppliedCreationTo('')
    setLocalStatusFilter('active')
    setStatusFilter('active')
    setPage(1)
  }

  const filterContent = (
    <div className="space-y-5">
      {/* Trạng thái */}
      <StatusFilterGroup value={localStatusFilter} onChange={setLocalStatusFilter} />

      {/* Khoảng thời gian tạo */}
      <div>
        <div className="space-y-3">
          <DateTimePicker
            label="Từ ngày"
            value={filterCreationFrom}
            onChange={(e) => setFilterCreationFrom(e.target.value)}
          />
          <DateTimePicker
            label="Đến ngày"
            value={filterCreationTo}
            onChange={(e) => setFilterCreationTo(e.target.value)}
          />
        </div>
      </div>
    </div>
  )

  return (
    <div className="mx-auto max-w-7xl">
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
        filterContent={filterContent}
        onApplyFilter={handleApplyFilter}
        onResetFilter={handleResetFilter}
      />

      {/* Delete confirm dialog */}
      <ConfirmDialog
        open={deleteTarget !== null}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDeleteConfirm}
        title="Xoá nhà cung cấp"
        message={
          <>
            Bạn có chắc chắn muốn xoá <strong>{deleteTarget?.name}</strong>?
            <br />
            Thao tác này không thể hoàn tác.
          </>
        }
        confirmLabel="Xoá"
        cancelLabel="Hủy"
        variant="danger"
        loading={isDeleting}
      />
    </div>
  )
}
