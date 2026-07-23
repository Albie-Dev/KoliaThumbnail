import { useMemo, useState, useEffect, useCallback } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Pencil, Trash2, Power, PowerOff, Radio } from 'lucide-react'
import { toast } from 'sonner'
import { DataTable } from '../../components/data-table/data-table'
import { useDataTableState } from '../../components/data-table/use-data-table-state'
import { Button } from '../../components/ui/button'
import { ConfirmDialog } from '../../components/ui/confirm-dialog'
import { DateTimePicker } from '../../components/ui/date-time-picker'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { Checkbox } from '../../components/ui/checkbox'
import { useSidebarContext } from '../../lib/sidebar-context'
import { formatDateTime } from '../../lib/date-formatter'
import { StatusFilterGroup, type StatusFilter } from '../../components/filters/status-filter'
import {
  getNewsSourcesWithPaging,
  deleteNewsSource,
  toggleNewsSource,
  bulkSetTrustNewsSources,
  type NewsSourceListItemDto,
} from './api'
import { SortDirection, FilterOperator, type SortRequestDto, type RangeFilterRequestDto, type FilterRequestDto } from '../../types/paging.types'
import { useQueryState, parseAsString } from 'nuqs'
import { CMarketScope, MARKET_SCOPE_OPTIONS } from '../../types/enums/pipeline.enums'
import { NEWS_SOURCE_GROUP_OPTIONS, getNewsSourceGroupLabel, getNewsSourceGroupBadgeClass } from './news-source-group-type'
import { getSourceFetchModeLabel } from './news-source-fetch-mode-type'
import { NewsSourceHealthBadge } from './news-source-health-badge'
import { NewsSourceTestFetchDialog } from './news-source-test-fetch-dialog'

export function NewsSourcesPage() {
  const { open } = useSidebarContext()
  const queryClient = useQueryClient()
  const { page, setPage, pageSize, setPageSize, search, setSearch, sortBy, sortOrder, handleSort } =
    useDataTableState(1, 10)

  // ── Status filter ──────────────────────────────────────────────────
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('active')
  const [localStatusFilter, setLocalStatusFilter] = useState<StatusFilter>('active')

  // ── Delete confirm state ─────────────────────────────────────────────
  const [deleteTarget, setDeleteTarget] = useState<NewsSourceListItemDto | null>(null)

  const { mutate: remove, isPending: isDeleting } = useMutation({
    mutationFn: (id: string) => deleteNewsSource(id),
    onSuccess: () => {
      toast.success('Đã xoá nguồn tin thành công.')
      queryClient.invalidateQueries({ queryKey: ['news-sources'] })
    },
  })

  const handleDeleteConfirm = useCallback(() => {
    if (!deleteTarget) return
    remove(deleteTarget.id, {
      onSuccess: () => setDeleteTarget(null),
    })
  }, [deleteTarget, remove])

  // ── Toggle mutation ──────────────────────────────────────────────────
  const { mutate: doToggle } = useMutation({
    mutationFn: (id: string) => toggleNewsSource(id),
    onSuccess: () => {
      toast.success('Đã thay đổi trạng thái nguồn tin.')
      queryClient.invalidateQueries({ queryKey: ['news-sources'] })
    },
    onError: () => {
      toast.error('Không thể thay đổi trạng thái nguồn tin.')
    },
  })

  // ── Test fetch dialog state ──────────────────────────────────────────
  const [testFetchTarget, setTestFetchTarget] = useState<NewsSourceListItemDto | null>(null)

  // ── Bulk shutdown ────────────────────────────────────────────────────
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [bulkConfirmOpen, setBulkConfirmOpen] = useState(false)

  const { mutate: doBulkSetTrust, isPending: isBulkSetting } = useMutation({
    mutationFn: ({ ids, isTrusted }: { ids: string[]; isTrusted: boolean }) =>
      bulkSetTrustNewsSources(ids, isTrusted),
    onSuccess: () => {
      toast.success('Đã cập nhật trạng thái hàng loạt thành công.')
      setSelectedIds(new Set())
      queryClient.invalidateQueries({ queryKey: ['news-sources'] })
    },
    onError: () => {
      toast.error('Không thể cập nhật trạng thái hàng loạt.')
    },
  })

  const handleBulkShutdown = useCallback(() => {
    if (selectedIds.size === 0) return
    doBulkSetTrust({ ids: Array.from(selectedIds), isTrusted: false })
    setBulkConfirmOpen(false)
  }, [selectedIds, doBulkSetTrust])

  // ── Sync applied filter values to URL query state ──────────────────────
  const [appliedCreationFrom, setAppliedCreationFrom] = useQueryState('creationFrom', parseAsString.withDefault(''))
  const [appliedCreationTo, setAppliedCreationTo] = useQueryState('creationTo', parseAsString.withDefault(''))

  // ── Local filter inputs state ────────────────────────────────────────
  const [filterCreationFrom, setFilterCreationFrom] = useState(appliedCreationFrom)
  const [filterCreationTo, setFilterCreationTo] = useState(appliedCreationTo)

  // ── SourceGroup filter ──────────────────────────────────────────────
  const [appliedSourceGroup, setAppliedSourceGroup] = useQueryState('sourceGroup', parseAsString.withDefault(''))
  const [filterSourceGroup, setFilterSourceGroup] = useState(appliedSourceGroup)

  // ── Region filter ───────────────────────────────────────────────────
  const [appliedRegion, setAppliedRegion] = useQueryState('region', parseAsString.withDefault(''))
  const [filterRegion, setFilterRegion] = useState(appliedRegion)

  // ── Only failing filter ─────────────────────────────────────────────
  const [appliedOnlyFailing, setAppliedOnlyFailing] = useQueryState('onlyFailing', parseAsString.withDefault(''))
  const [filterOnlyFailing, setFilterOnlyFailing] = useState(appliedOnlyFailing === 'true')

  // ── Sync local filter when applied changes ───────────────────────────
  useEffect(() => { setLocalStatusFilter(statusFilter) }, [statusFilter])
  useEffect(() => { setFilterCreationFrom(appliedCreationFrom) }, [appliedCreationFrom])
  useEffect(() => { setFilterCreationTo(appliedCreationTo) }, [appliedCreationTo])
  useEffect(() => { setFilterSourceGroup(appliedSourceGroup) }, [appliedSourceGroup])
  useEffect(() => { setFilterRegion(appliedRegion) }, [appliedRegion])
  useEffect(() => { setFilterOnlyFailing(appliedOnlyFailing === 'true') }, [appliedOnlyFailing])

  const backendSorts = useMemo<SortRequestDto[]>(() => {
    if (!sortBy || !sortOrder) return []
    let field = 'Name'
    if (sortBy === 'name') field = 'Name'
    if (sortBy === 'domain') field = 'Domain'
    if (sortBy === 'priority') field = 'Priority'
    if (sortBy === 'creationTime') field = 'CreationTime'
    if (sortBy === 'lastFetchedAt') field = 'LastFetchedAt'

    return [
      {
        field,
        direction: sortOrder === 'desc' ? SortDirection.Desc : SortDirection.Asc,
      },
    ]
  }, [sortBy, sortOrder])

  // Build Deleted params
  const backendDeletedFilters = useMemo(() => {
    if (statusFilter === 'all') return { includeDeleted: true }
    if (statusFilter === 'deleted') return { includeDeleted: true, deletedOnly: true }
    return {}
  }, [statusFilter])

  // Build Range Filters
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

  // Build additional filters
  const backendFilters = useMemo(() => {
    const filters: FilterRequestDto[] = []

    if (appliedOnlyFailing === 'true') {
      filters.push({ field: 'IsTrusted', operator: FilterOperator.Equal, values: [false] })
    }

    return filters
  }, [appliedOnlyFailing])

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: [
      'news-sources',
      page,
      pageSize,
      search,
      statusFilter,
      appliedSourceGroup,
      appliedRegion,
      appliedOnlyFailing,
      backendSorts,
      backendRangeFilters,
    ],
    queryFn: () =>
      getNewsSourcesWithPaging({
        pageNumber: page,
        pageSize: pageSize,
        searchText: search || undefined,
        sorts: backendSorts,
        rangeFilters: backendRangeFilters,
        filters: backendFilters.length > 0 ? backendFilters : undefined,
        includeDeleted: backendDeletedFilters.includeDeleted,
        deletedOnly: backendDeletedFilters.deletedOnly,
        group: appliedSourceGroup ? Number(appliedSourceGroup) : undefined,
        region: appliedRegion ? Number(appliedRegion) : undefined,
      }),
  })

  // ── Selection logic (after data is available) ──────────────────────────
  const isAllPageSelected = useMemo(
    () => (data?.items?.length ?? 0) > 0 && (data?.items ?? []).every((item) => selectedIds.has(item.id)),
    [data?.items, selectedIds],
  )

  const handleToggleSelectAll = useCallback(() => {
    if (!data) return
    if (isAllPageSelected) {
      setSelectedIds((prev) => {
        const next = new Set(prev)
        data.items.forEach((item) => next.delete(item.id))
        return next
      })
    } else {
      setSelectedIds((prev) => {
        const next = new Set(prev)
        data.items.forEach((item) => next.add(item.id))
        return next
      })
    }
  }, [data, isAllPageSelected])

  const handleToggleSelect = useCallback((id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }, [])

  // ── Clear selection when data changes (e.g. page, filter, sort) ──────
  useEffect(() => { setSelectedIds(new Set()) }, [data?.items])

  const columns = useMemo(
    () => [
      {
        key: 'select',
        header: (
          <Checkbox
            checked={isAllPageSelected}
            onCheckedChange={handleToggleSelectAll}
            aria-label="Chọn tất cả"
          />
        ),
        render: (item: NewsSourceListItemDto) => (
          <Checkbox
            checked={selectedIds.has(item.id)}
            onCheckedChange={() => handleToggleSelect(item.id)}
            aria-label={`Chọn ${item.name}`}
          />
        ),
      },
      {
        key: 'name',
        header: 'Tên',
        sortable: true,
        render: (item: NewsSourceListItemDto) => (
          <span className="font-medium text-slate-900 dark:text-slate-100">{item.name}</span>
        ),
      },
      {
        key: 'domain',
        header: 'Domain',
        sortable: true,
        render: (item: NewsSourceListItemDto) => (
          <span className="text-xs text-slate-500 dark:text-slate-400">{item.domain}</span>
        ),
      },
      {
        key: 'sourceGroup',
        header: 'Nhóm',
        render: (item: NewsSourceListItemDto) => {
          const label = getNewsSourceGroupLabel(item.sourceGroup as any)
          return (
            <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${getNewsSourceGroupBadgeClass(item.sourceGroup)}`}>
              {label || item.sourceGroup}
            </span>
          )
        },
      },
      {
        key: 'region',
        header: 'Khu vực',
        render: (item: NewsSourceListItemDto) => {
          const label = item.region === CMarketScope.International
            ? 'Quốc tế'
            : item.region === CMarketScope.Domestic
              ? 'Nội địa'
              : 'Cả hai'
          return (
            <span className="text-xs text-slate-600 dark:text-slate-400">{label}</span>
          )
        },
      },
      {
        key: 'fetchMode',
        header: 'Fetch mode',
        render: (item: NewsSourceListItemDto) => (
          <span className="text-xs text-slate-600 dark:text-slate-400">
            {getSourceFetchModeLabel(item.fetchMode) || item.fetchMode}
          </span>
        ),
      },
      {
        key: 'priority',
        header: 'Priority',
        sortable: true,
        render: (item: NewsSourceListItemDto) => (
          <span className="text-xs text-slate-600 dark:text-slate-400">{item.priority}</span>
        ),
      },
      {
        key: 'operationalStatus',
        header: 'Trạng thái vận hành',
        render: (item: NewsSourceListItemDto) => (
          <NewsSourceHealthBadge
            consecutiveFailureCount={item.consecutiveFailureCount}
            isTrusted={item.isTrusted}
            lastFailedAt={item.lastFailedAt}
          />
        ),
      },
      {
        key: 'lastFetchedAt',
        header: 'Fetch gần nhất',
        sortable: true,
        render: (item: NewsSourceListItemDto) => (
          <span className="text-xs text-slate-500 dark:text-slate-400">
            {formatDateTime(item.lastFetchedAt) || '—'}
          </span>
        ),
      },
      {
        key: 'creationTime',
        header: 'Tạo lúc',
        sortable: true,
        render: (item: NewsSourceListItemDto) => formatDateTime(item.creationTime),
      },
      {
        key: 'actions',
        header: '',
        render: (item: NewsSourceListItemDto) =>
          item.isDeleted ? (
            <span className="text-xs text-slate-400 dark:text-slate-500 italic">Đã xoá</span>
          ) : (
            <div className="flex items-center gap-0.5">
              <Button
                variant="ghost"
                size="icon"
                onClick={() => setTestFetchTarget(item)}
                title="Test fetch"
              >
                <Radio className="h-4 w-4" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                onClick={() => doToggle(item.id)}
                title={item.isTrusted ? 'Tắt nguồn' : 'Bật nguồn'}
              >
                {item.isTrusted
                  ? <Power className="h-4 w-4 text-emerald-500" />
                  : <PowerOff className="h-4 w-4 text-slate-400" />
                }
              </Button>
              <Button
                variant="ghost"
                size="icon"
                onClick={() => open({ type: 'edit-news-source', source: item })}
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
    [open, setDeleteTarget, doToggle, isAllPageSelected, handleToggleSelectAll, selectedIds, handleToggleSelect],
  )

  const handleApplyFilter = () => {
    setAppliedCreationFrom(filterCreationFrom)
    setAppliedCreationTo(filterCreationTo)
    setAppliedSourceGroup(filterSourceGroup)
    setAppliedRegion(filterRegion)
    setAppliedOnlyFailing(filterOnlyFailing ? 'true' : '')
    setStatusFilter(localStatusFilter)
    setPage(1)
  }

  const handleResetFilter = () => {
    setFilterCreationFrom('')
    setFilterCreationTo('')
    setAppliedCreationFrom('')
    setAppliedCreationTo('')
    setFilterSourceGroup('')
    setAppliedSourceGroup('')
    setFilterRegion('')
    setAppliedRegion('')
    setFilterOnlyFailing(false)
    setAppliedOnlyFailing('')
    setLocalStatusFilter('active')
    setStatusFilter('active')
    setPage(1)
  }

  const filterContent = (
    <div className="space-y-5">
      {/* Trạng thái */}
      <StatusFilterGroup value={localStatusFilter} onChange={setLocalStatusFilter} />

      {/* Nhóm nguồn */}
      <div>
        <label className="mb-1.5 block text-xs font-medium text-slate-600 dark:text-slate-300">Nhóm nguồn</label>
        <SelectDropdown<{ id: number; label: string }>
          items={NEWS_SOURCE_GROUP_OPTIONS}
          getOptionId={(opt) => String(opt.id)}
          getOptionLabel={(opt) => opt.label}
          value={NEWS_SOURCE_GROUP_OPTIONS.find((opt) => String(opt.id) === filterSourceGroup) ?? null}
          onChange={(opt) => setFilterSourceGroup(opt ? String(opt.id) : '')}
          allowSearch={true}
          searchPlaceholder="Tìm nhóm..."
          placeholder="Tất cả nhóm"
        />
      </div>

      {/* Khu vực */}
      <div>
        <label className="mb-1.5 block text-xs font-medium text-slate-600 dark:text-slate-300">Khu vực</label>
        <SelectDropdown<{ id: number; label: string }>
          items={MARKET_SCOPE_OPTIONS}
          getOptionId={(opt) => String(opt.id)}
          getOptionLabel={(opt) => opt.label}
          value={MARKET_SCOPE_OPTIONS.find((opt) => String(opt.id) === filterRegion) ?? null}
          onChange={(opt) => setFilterRegion(opt ? String(opt.id) : '')}
          placeholder="Tất cả khu vực"
        />
      </div>

      {/* Chỉ hiện nguồn đang lỗi */}
      <label className="flex items-center gap-2 cursor-pointer">
        <Checkbox
          checked={filterOnlyFailing}
          onCheckedChange={(checked) => setFilterOnlyFailing(checked === true)}
        />
        <span className="text-sm text-slate-700 dark:text-slate-300">Chỉ hiện nguồn đang lỗi</span>
      </label>

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
        title="Nguồn tin tức"
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        error={error instanceof Error ? error.message : null}
        onRetry={() => void refetch()}
        emptyMessage="Không có nguồn tin nào phù hợp."
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
        // ── Search ──────────────────────────────────────────────
        search={search}
        onSearchChange={(value) => {
          setSearch(value)
          setPage(1)
        }}
        searchPlaceholder="Tìm kiếm nguồn tin..."
        // ── Filter sidebar ──────────────────────────────────────
        filterContent={filterContent}
        onApplyFilter={handleApplyFilter}
        onResetFilter={handleResetFilter}
        // ── Create button ───────────────────────────────────────
        actions={
          <div className="flex items-center gap-2">
            {selectedIds.size > 0 && (
              <div className="flex items-center gap-2">
                <span className="text-sm text-slate-500 dark:text-slate-400 whitespace-nowrap">
                  Đã chọn {selectedIds.size}
                </span>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setBulkConfirmOpen(true)}
                  disabled={isBulkSetting}
                  className="text-red-600 dark:text-red-400 border-red-200 dark:border-red-800 hover:bg-red-50 hover:dark:bg-red-950/40"
                >
                  <PowerOff className="h-4 w-4" />
                  Tắt hàng loạt
                </Button>
              </div>
            )}
            <Button onClick={() => open({ type: 'create-news-source' })}>
              <Plus className="h-4 w-4" />
              Tạo nguồn tin
            </Button>
          </div>
        }
      />

      {/* Delete confirm dialog */}
      <ConfirmDialog
        open={deleteTarget !== null}
        onClose={() => setDeleteTarget(null)}
        title="Xác nhận xoá nguồn tin"
        message={
          deleteTarget
            ? `Bạn có chắc chắn muốn xoá nguồn tin "${deleteTarget.name}" (${deleteTarget.domain})? Thao tác này sẽ xoá mềm và có thể khôi phục sau.`
            : ''
        }
        onConfirm={handleDeleteConfirm}
        loading={isDeleting}
        confirmLabel="Xoá"
        variant="danger"
      />

      {/* Bulk shutdown confirm dialog */}
      <ConfirmDialog
        open={bulkConfirmOpen}
        onClose={() => setBulkConfirmOpen(false)}
        title="Xác nhận tắt hàng loạt"
        message={
          `Bạn có chắc chắn muốn tắt ${selectedIds.size} nguồn tin đã chọn? ` +
          'Sau khi tắt, hệ thống sẽ không fetch tin từ các nguồn này.'
        }
        onConfirm={handleBulkShutdown}
        loading={isBulkSetting}
        confirmLabel="Tắt"
        variant="warning"
      />

      {/* Test fetch dialog */}
      <NewsSourceTestFetchDialog
        open={testFetchTarget !== null}
        onClose={() => setTestFetchTarget(null)}
        sourceId={testFetchTarget?.id ?? null}
        sourceName={testFetchTarget?.name ?? ''}
      />
    </div>
  )
}
