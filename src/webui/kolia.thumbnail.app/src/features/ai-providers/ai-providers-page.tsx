import { useMemo, useState, useEffect, useCallback } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Pencil, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { DataTable } from '../../components/data-table/data-table'
import { useDataTableState } from '../../components/data-table/use-data-table-state'
import { Button } from '../../components/ui/button'
import { ConfirmDialog } from '../../components/ui/confirm-dialog'
import { DateTimePicker } from '../../components/ui/date-time-picker'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { useSidebarContext } from '../../lib/sidebar-context'
import { formatDateTime } from '../../lib/date-formatter'
import { StatusFilterGroup, type StatusFilter } from '../../components/filters/status-filter'
import { getAIProvidersWithPaging, deleteAIProvider, type AIProviderBaseDto } from './api'
import { SortDirection, FilterOperator, type SortRequestDto, type RangeFilterRequestDto } from '../../types/paging.types'
import { useQueryState, parseAsString } from 'nuqs'
import { ApiError } from '../../lib/api/api-error'
import { AI_PROVIDER_TYPE_OPTIONS, CAIProviderType, getAIProviderTypeLabel, getAIProviderTypeBadgeClass } from './ai-provider-type'

export function AiProvidersPage() {
  const { open } = useSidebarContext()
  const queryClient = useQueryClient()
  const { page, setPage, pageSize, setPageSize, search, setSearch, sortBy, sortOrder, handleSort } =
    useDataTableState(1, 10)

  // ── Status filter ──────────────────────────────────────────────────
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('active')
  const [localStatusFilter, setLocalStatusFilter] = useState<StatusFilter>('active')

  // ── Delete confirm state ─────────────────────────────────────────────
  const [deleteTarget, setDeleteTarget] = useState<AIProviderBaseDto | null>(null)

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

  // ── ProviderType filter ──────────────────────────────────────────────
  const [appliedProviderType, setAppliedProviderType] = useQueryState('providerType', parseAsString.withDefault(''))
  const [filterProviderType, setFilterProviderType] = useState(appliedProviderType)

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

  useEffect(() => {
    setFilterProviderType(appliedProviderType)
  }, [appliedProviderType])

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
      appliedProviderType,
      backendSorts,
      backendRangeFilters,
    ],
    queryFn: () =>
      getAIProvidersWithPaging({
        pageNumber: page,
        pageSize: pageSize,
        searchText: search || undefined,
        sorts: backendSorts,
        rangeFilters: backendRangeFilters,
        filters: appliedProviderType
          ? [{ field: 'ProviderType', operator: FilterOperator.Equal, values: [Number(appliedProviderType)] }]
          : undefined,
        includeDeleted: backendDeletedFilters.includeDeleted,
        deletedOnly: backendDeletedFilters.deletedOnly,
      }),
  })

  const columns = useMemo(
    () => [
      { key: 'name', header: 'Tên', sortable: true, render: (item: AIProviderBaseDto) => item.name },
      { key: 'shortName', header: 'Mã', sortable: true, render: (item: AIProviderBaseDto) => item.shortName },
      {
        key: 'baseUrl',
        header: 'Base URL',
        render: (item: AIProviderBaseDto) => (
          <span className="max-w-[200px] truncate block text-xs" title={item.baseUrl}>
            {item.baseUrl}
          </span>
        ),
      },
      {
        key: 'image',
        header: 'Logo',
        render: (item: AIProviderBaseDto) =>
          item.imageUrl ? (
            <img src={item.imageUrl} alt={item.name} className="h-8 w-8 rounded object-cover" />
          ) : (
            <span className="text-sm text-slate-400 dark:text-slate-500">—</span>
          ),
      },
      {
        key: 'providerType',
        header: 'Loại',
        sortable: true,
        render: (item: AIProviderBaseDto) => {
          const label = getAIProviderTypeLabel(item.providerType as CAIProviderType)
          return (
            <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${getAIProviderTypeBadgeClass(item.providerType)}`}>
              {label || item.providerType}
            </span>
          )
        },
      },
      {
        key: 'creationTime',
        header: 'Tạo lúc',
        sortable: true,
        render: (item: AIProviderBaseDto) => formatDateTime(item.creationTime),
      },
      {
        key: 'lastModificationTime',
        header: 'Cập nhật lúc',
        sortable: true,
        render: (item: AIProviderBaseDto) => formatDateTime(item.lastModificationTime),
      },
      {
        key: 'actions',
        header: '',
        render: (item: AIProviderBaseDto) =>
          item.isDeleted ? (
            <span className="text-xs text-slate-400 dark:text-slate-500 italic">Đã xoá</span>
          ) : (
            <div className="flex items-center gap-0.5">
              <Button
                variant="ghost"
                size="icon"
                onClick={() => open({ type: 'edit-ai-provider', provider: item })}
                title="Chỉnh sửa"
              >
                <Pencil className="h-4 w-4" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                onClick={() => setDeleteTarget(item)}
                title="Xoá"
                className="hover:bg-red-50 hover:dark:bg-red-950/40 hover:text-red-600 hover:dark:text-red-400"
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            </div>
          ),
      },
    ],
    [open, setDeleteTarget],
  )

  const handleApplyFilter = () => {
    setAppliedCreationFrom(filterCreationFrom)
    setAppliedCreationTo(filterCreationTo)
    setAppliedProviderType(filterProviderType)
    setStatusFilter(localStatusFilter)
    setPage(1)
  }

  const handleResetFilter = () => {
    setFilterCreationFrom('')
    setFilterCreationTo('')
    setAppliedCreationFrom('')
    setAppliedCreationTo('')
    setFilterProviderType('')
    setAppliedProviderType('')
    setLocalStatusFilter('active')
    setStatusFilter('active')
    setPage(1)
  }

  const filterContent = (
    <div className="space-y-5">
      {/* Trạng thái */}
      <StatusFilterGroup value={localStatusFilter} onChange={setLocalStatusFilter} />

      {/* Loại nhà cung cấp */}
      <div>
        <label className="mb-1.5 block text-xs font-medium text-slate-600 dark:text-slate-300">Loại nhà cung cấp</label>
        <SelectDropdown<{ id: number; label: string }>
          items={AI_PROVIDER_TYPE_OPTIONS}
          getOptionId={(opt) => String(opt.id)}
          getOptionLabel={(opt) => opt.label}
          value={AI_PROVIDER_TYPE_OPTIONS.find((opt) => String(opt.id) === filterProviderType) ?? null}
          onChange={(opt) => setFilterProviderType(opt ? String(opt.id) : '')}
          allowSearch={true}
          searchPlaceholder="Tìm loại..."
          placeholder="Tất cả loại"
        />
      </div>

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
          <Button onClick={() => open({ type: 'create-ai-provider' })} className="gap-2">
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
