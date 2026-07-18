import { useMemo, useState, useEffect, useCallback } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Search, SlidersHorizontal, RefreshCw, X } from 'lucide-react'
import { toast } from 'sonner'
import { Input } from '../../components/ui/input'
import { Button } from '../../components/ui/button'
import { ConfirmDialog } from '../../components/ui/confirm-dialog'
import { DateTimePicker } from '../../components/ui/date-time-picker'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { StatusFilterGroup, type StatusFilter } from '../../components/filters/status-filter'
import { DataTableFilterSidebar } from '../../components/data-table/data-table-filter-sidebar'
import { DataTablePagination } from '../../components/data-table/data-table-pagination'
import { DataTableEmptyState } from '../../components/data-table/data-table-empty-state'
import { DataTableErrorState } from '../../components/data-table/data-table-error-state'
import { useDataTableState } from '../../components/data-table/use-data-table-state'
import { useQueryState, parseAsString } from 'nuqs'
import { ApiError } from '../../lib/api/api-error'
import { SortDirection, FilterOperator, type SortRequestDto, type RangeFilterRequestDto } from '../../types/paging.types'
import { getProjectsWithPaging, deleteProject, type ProjectBaseDto } from './api'
import { ProjectCard } from './project-card'
import { PROJECT_STATUS_OPTIONS } from './project-type'

export function ProjectsPage() {
  const queryClient = useQueryClient()
  const { page, setPage, pageSize, setPageSize, search, setSearch } = useDataTableState(1, 12)

  const [isFilterOpen, setIsFilterOpen] = useState(false)

  // ── Status filter (active/deleted/all — soft delete) ─────────────────
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('active')
  const [localStatusFilter, setLocalStatusFilter] = useState<StatusFilter>('active')

  // ── Delete confirm state ──────────────────────────────────────────────
  const [deleteTarget, setDeleteTarget] = useState<ProjectBaseDto | null>(null)

  const { mutate: remove, isPending: isDeleting } = useMutation({
    mutationFn: (id: string) => deleteProject(id),
    onSuccess: () => {
      toast.success('Đã xoá project thành công.')
      queryClient.invalidateQueries({ queryKey: ['projects'] })
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

  // ── Applied filter values (sync với URL) ──────────────────────────────
  const [appliedCreationFrom, setAppliedCreationFrom] = useQueryState('creationFrom', parseAsString.withDefault(''))
  const [appliedCreationTo, setAppliedCreationTo] = useQueryState('creationTo', parseAsString.withDefault(''))
  const [appliedProjectStatus, setAppliedProjectStatus] = useQueryState('projectStatus', parseAsString.withDefault(''))

  // ── Local filter inputs (controlled trong Sidebar, chỉ apply khi bấm "Áp dụng") ──
  const [filterCreationFrom, setFilterCreationFrom] = useState(appliedCreationFrom)
  const [filterCreationTo, setFilterCreationTo] = useState(appliedCreationTo)
  const [filterProjectStatus, setFilterProjectStatus] = useState(appliedProjectStatus)

  useEffect(() => setLocalStatusFilter(statusFilter), [statusFilter])
  useEffect(() => setFilterCreationFrom(appliedCreationFrom), [appliedCreationFrom])
  useEffect(() => setFilterCreationTo(appliedCreationTo), [appliedCreationTo])
  useEffect(() => setFilterProjectStatus(appliedProjectStatus), [appliedProjectStatus])

  const backendDeletedFilters = useMemo(() => {
    if (statusFilter === 'all') return { includeDeleted: true }
    if (statusFilter === 'deleted') return { includeDeleted: true, deletedOnly: true }
    return {}
  }, [statusFilter])

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

  // Mặc định sắp xếp theo cập nhật gần nhất — phù hợp với 1 kho lưu trữ
  const backendSorts = useMemo<SortRequestDto[]>(
    () => [{ field: 'LastModificationTime', direction: SortDirection.Desc }],
    [],
  )

  const { data, isLoading, isFetching, error, refetch } = useQuery({
    queryKey: [
      'projects',
      page,
      pageSize,
      search,
      statusFilter,
      appliedProjectStatus,
      appliedCreationFrom,
      appliedCreationTo,
    ],
    queryFn: () =>
      getProjectsWithPaging({
        pageNumber: page,
        pageSize,
        searchText: search || undefined,
        sorts: backendSorts,
        rangeFilters: backendRangeFilters,
        filters: appliedProjectStatus
          ? [{ field: 'Status', operator: FilterOperator.Equal, values: [Number(appliedProjectStatus)] }]
          : undefined,
        includeDeleted: backendDeletedFilters.includeDeleted,
        deletedOnly: backendDeletedFilters.deletedOnly,
      }),
  })

  const handleApplyFilter = () => {
    setAppliedCreationFrom(filterCreationFrom)
    setAppliedCreationTo(filterCreationTo)
    setAppliedProjectStatus(filterProjectStatus)
    setStatusFilter(localStatusFilter)
    setPage(1)
  }

  const handleResetFilter = () => {
    setFilterCreationFrom('')
    setFilterCreationTo('')
    setAppliedCreationFrom('')
    setAppliedCreationTo('')
    setFilterProjectStatus('')
    setAppliedProjectStatus('')
    setLocalStatusFilter('active')
    setStatusFilter('active')
    setPage(1)
  }

  const filterContent = (
    <div className="space-y-5">
      <StatusFilterGroup value={localStatusFilter} onChange={setLocalStatusFilter} />

      <div>
        <label className="mb-1.5 block text-xs font-medium text-slate-600 dark:text-slate-300">Trạng thái project</label>
        <SelectDropdown<{ id: number; label: string }>
          items={PROJECT_STATUS_OPTIONS}
          getOptionId={(opt) => String(opt.id)}
          getOptionLabel={(opt) => opt.label}
          value={PROJECT_STATUS_OPTIONS.find((opt) => String(opt.id) === filterProjectStatus) ?? null}
          onChange={(opt) => setFilterProjectStatus(opt ? String(opt.id) : '')}
          allowSearch={false}
          placeholder="Tất cả trạng thái"
        />
      </div>

      <div className="space-y-3">
        <DateTimePicker label="Từ ngày" value={filterCreationFrom} onChange={(e) => setFilterCreationFrom(e.target.value)} />
        <DateTimePicker label="Đến ngày" value={filterCreationTo} onChange={(e) => setFilterCreationTo(e.target.value)} />
      </div>
    </div>
  )

  const items = data?.items ?? []

  return (
    <div className="mx-auto max-w-7xl">
      <DataTableFilterSidebar
        isOpen={isFilterOpen}
        onClose={() => setIsFilterOpen(false)}
        onApply={handleApplyFilter}
        onReset={handleResetFilter}
      >
        {filterContent}
      </DataTableFilterSidebar>

      <section className="rounded-2xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 shadow-sm">
        {/* ── Header / Toolbar ─────────────────────────────────────── */}
        <div className="border-b border-slate-100 dark:border-slate-800 px-6 py-4">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h2 className="text-lg font-semibold text-slate-900 dark:text-slate-100">Kho lưu trữ</h2>
              <p className="text-sm text-slate-500 dark:text-slate-400">Quản lý các project trong pipeline tạo Thumbnail & Title</p>
            </div>

            <div className="flex flex-wrap items-center gap-2">
              <div className="relative">
                <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400 dark:text-slate-500" />
                <Input
                  value={search}
                  onChange={(e) => {
                    setSearch(e.target.value)
                    setPage(1)
                  }}
                  placeholder="Tìm theo tên project, video title..."
                  className="h-9 w-64 pl-9 pr-8"
                />
                {search && (
                  <button
                    type="button"
                    onClick={() => {
                      setSearch('')
                      setPage(1)
                    }}
                    className="absolute right-2 top-1/2 -translate-y-1/2 rounded p-0.5 text-slate-400 dark:text-slate-500 hover:text-slate-600 hover:dark:text-slate-300"
                  >
                    <X className="h-3.5 w-3.5" />
                  </button>
                )}
              </div>

              <Button variant="outline" size="default" className="h-9 gap-1.5" onClick={() => setIsFilterOpen(true)}>
                <SlidersHorizontal className="h-4 w-4" />
                <span className="hidden sm:inline">Lọc</span>
              </Button>

              <Button
                variant="outline"
                size="default"
                className="h-9 w-9 p-0 shrink-0"
                onClick={() => void refetch()}
                disabled={isFetching}
                title="Làm mới"
              >
                <RefreshCw className={isFetching ? 'h-4 w-4 animate-spin' : 'h-4 w-4'} />
              </Button>
            </div>
          </div>
        </div>

        {/* ── Body: grid ───────────────────────────────────────────── */}
        <div className="px-6 py-6">
          {isLoading ? (
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
              {Array.from({ length: pageSize }).map((_, index) => (
                <div key={index} className="overflow-hidden rounded-xl border border-slate-200 dark:border-slate-700">
                  <div className="aspect-video w-full animate-pulse bg-slate-100 dark:bg-slate-800" />
                  <div className="space-y-2 px-3 py-2.5">
                    <div className="h-3.5 w-3/4 animate-pulse rounded bg-slate-100 dark:bg-slate-800" />
                    <div className="h-3 w-1/2 animate-pulse rounded bg-slate-100 dark:bg-slate-800" />
                  </div>
                </div>
              ))}
            </div>
          ) : error ? (
            <DataTableErrorState
              title="Kho lưu trữ"
              message={error instanceof Error ? error.message : 'Có lỗi xảy ra khi tải dữ liệu.'}
              onRetry={() => void refetch()}
            />
          ) : items.length === 0 ? (
            <DataTableEmptyState title="Kho lưu trữ" message="Không có project nào phù hợp." />
          ) : (
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
              {items.map((project) => (
                <ProjectCard
                  key={project.id}
                  project={project}
                  onEdit={(p) => {
                    // TODO: điều hướng tới bước hiện tại của project trong pipeline
                    // (chưa có route chi tiết project — cần xác nhận thêm)
                    toast.info(`Chỉnh sửa project "${p.name}" — chưa cấu hình route chi tiết.`)
                  }}
                  onRemove={(p) => setDeleteTarget(p)}
                />
              ))}
            </div>
          )}
        </div>

        {/* ── Footer: pagination ───────────────────────────────────── */}
        {data && (
          <div className="border-t border-slate-100 dark:border-slate-800 px-6 py-4">
            <DataTablePagination
              page={data.pageNumber}
              pageSize={data.pageSize}
              totalPages={data.totalPages}
              totalCount={data.totalCount}
              onPageChange={(next) => setPage(next)}
              onPageSizeChange={(next) => {
                setPageSize(next)
                setPage(1)
              }}
            />
          </div>
        )}
      </section>

      <ConfirmDialog
        open={deleteTarget !== null}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDeleteConfirm}
        title="Xoá project"
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
