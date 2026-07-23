import { useState, useCallback, useMemo, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Pencil, Trash2, Archive, Search, RefreshCw } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { Button } from '../../components/ui/button'
import { Input } from '../../components/ui/input'
import { ConfirmDialog } from '../../components/ui/confirm-dialog'
import { DataTablePagination } from '../../components/data-table/data-table-pagination'
import { useDataTableState } from '../../components/data-table/use-data-table-state'
import { useSidebarContext } from '../../lib/sidebar-context'
import { useActiveProjectId } from '../../lib/project-context'
import { formatDateTime } from '../../lib/date-formatter'
import { getProjectsWithPaging, deleteProject, renameProject, type ProjectSummaryDto } from './api'
import { CProjectStatus, getProjectStatusLabel, getProjectStatusBadgeClass, PROJECT_STATUS_OPTIONS } from '../../types/enums/pipeline.enums'
import { SortDirection, type SortRequestDto } from '../../types/paging.types'

const PROJECTS_QK = ['projects'] as const

export function ProjectsPage() {
  const { open } = useSidebarContext()
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const [activeProjectId, setActiveProjectId] = useActiveProjectId()

  const { page, setPage, pageSize, setPageSize, search, setSearch, sortBy, sortOrder } =
    useDataTableState(1, 12)

  const [localSearch, setLocalSearch] = useState(search)

  useEffect(() => {
    setLocalSearch(search)
  }, [search])

  useEffect(() => {
    const handler = setTimeout(() => {
      if (localSearch !== search) {
        setSearch(localSearch)
        setPage(1)
      }
    }, 400)
    return () => clearTimeout(handler)
  }, [localSearch, search, setSearch, setPage])

  const [statusFilter, setStatusFilter] = useState<number | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<ProjectSummaryDto | null>(null)
  const [renameTarget, setRenameTarget] = useState<ProjectSummaryDto | null>(null)
  const [renameValue, setRenameValue] = useState('')

  const invalidateList = () => queryClient.invalidateQueries({ queryKey: PROJECTS_QK })

  const { mutate: remove, isPending: isDeleting } = useMutation({
    mutationFn: (id: string) => deleteProject(id),
    onSuccess: () => { toast.success('Đã xoá project thành công.'); setDeleteTarget(null); invalidateList() },
  })

  const { mutate: rename, isPending: isRenaming } = useMutation({
    mutationFn: ({ id, name }: { id: string; name: string }) => renameProject(id, name),
    onSuccess: () => { toast.success('Đã đổi tên project thành công.'); setRenameTarget(null); setRenameValue(''); invalidateList() },
  })

  const handleDeleteConfirm = useCallback(() => { if (!deleteTarget) return; remove(deleteTarget.id) }, [deleteTarget, remove])
  const handleRenameConfirm = useCallback(() => { if (!renameTarget || !renameValue.trim()) return; rename({ id: renameTarget.id, name: renameValue.trim() }) }, [renameTarget, renameValue, rename])

  const backendSorts = useMemo<SortRequestDto[]>(() => {
    if (!sortBy || !sortOrder) return [{ field: 'CreationTime', direction: SortDirection.Desc }]
    const fieldMap: Record<string, string> = { name: 'Name', status: 'Status', creationTime: 'CreationTime', lastActivityTime: 'LastActivityTime' }
    return [{ field: fieldMap[sortBy] ?? 'CreationTime', direction: sortOrder === 'desc' ? SortDirection.Desc : SortDirection.Asc }]
  }, [sortBy, sortOrder])

  const { data, isLoading, error, refetch, isFetching } = useQuery({
    queryKey: [...PROJECTS_QK, page, pageSize, search, statusFilter, sortBy, sortOrder],
    queryFn: () => getProjectsWithPaging({ pageNumber: page, pageSize, searchText: search || undefined, sorts: backendSorts }),
  })

  const filteredProjects = (data?.items ?? []).filter((p) => {
    if (statusFilter == null) return true
    return p.status === statusFilter
  })

  const projects = filteredProjects
  const totalFiltered = statusFilter != null ? filteredProjects.length : data?.totalCount ?? 0
  const totalPagesFiltered = statusFilter != null ? Math.ceil(filteredProjects.length / pageSize) : data?.totalPages ?? 1

  function handleCardClick(project: ProjectSummaryDto) {
    setActiveProjectId(project.id)
    navigate('/dashboard?projectId=' + encodeURIComponent(project.id))
  }

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      {/* ── Toolbar ────────────────────────────────────────────────────── */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-slate-900 dark:text-slate-100">Kho lưu trữ</h1>
          <p className="text-sm text-slate-500 dark:text-slate-400">Quản lý tất cả các project của bạn</p>
        </div>
        <Button onClick={() => open({ type: 'create-project' })}>
          <Plus className="h-4 w-4" />
          Tạo project mới
        </Button>
      </div>

      {/* ── Search / Filter / Refresh bar ──────────────────────────────── */}
      <div className="flex flex-wrap items-center gap-3">
        <div className="relative flex-1 min-w-[200px] max-w-sm">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
          <Input
            value={localSearch}
            onChange={(e) => setLocalSearch(e.target.value)}
            placeholder="Tìm kiếm project..."
            className="pl-9"
          />
        </div>

        <select
          value={statusFilter ?? ''}
          onChange={(e) => { setStatusFilter(e.target.value ? Number(e.target.value) : null); setPage(1) }}
          className="rounded-md border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 px-3 py-2 text-sm text-slate-700 dark:text-slate-200 focus:outline-none focus:ring-2 focus:ring-slate-900/10 dark:focus:ring-slate-100/20"
        >
          <option value="">Tất cả trạng thái</option>
          {PROJECT_STATUS_OPTIONS.map((opt) => (
            <option key={opt.id} value={opt.id}>
              {opt.label}
            </option>
          ))}
        </select>

        <Button variant="outline" size="icon" onClick={() => void refetch()} disabled={isFetching} title="Làm mới">
          <RefreshCw className={`h-4 w-4 ${isFetching ? 'animate-spin' : ''}`} />
        </Button>
      </div>

      {/* ── Loading ────────────────────────────────────────────────────── */}
      {isLoading && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <div
              key={i}
              className="animate-pulse rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4"
            >
              <div className="mb-3 h-32 rounded-lg bg-slate-200 dark:bg-slate-700" />
              <div className="h-4 w-3/4 rounded bg-slate-200 dark:bg-slate-700" />
              <div className="mt-2 h-3 w-1/2 rounded bg-slate-200 dark:bg-slate-700" />
            </div>
          ))}
        </div>
      )}

      {/* ── Error ──────────────────────────────────────────────────────── */}
      {error && !isLoading && (
        <div className="flex flex-col items-center justify-center gap-3 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-12">
          <p className="text-sm text-red-600 dark:text-red-400">
            {error instanceof Error ? error.message : 'Có lỗi xảy ra khi tải dữ liệu'}
          </p>
          <Button variant="outline" onClick={() => void refetch()}>Thử lại</Button>
        </div>
      )}

      {/* ── Empty ──────────────────────────────────────────────────────── */}
      {!isLoading && !error && projects.length === 0 && (
        <div className="flex flex-col items-center justify-center gap-4 rounded-xl border border-dashed border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 p-12">
          <Archive className="h-12 w-12 text-slate-300 dark:text-slate-600" />
          <p className="text-sm text-slate-500 dark:text-slate-400">
            {search || statusFilter != null ? 'Không tìm thấy project phù hợp' : 'Chưa có project nào'}
          </p>
          <Button onClick={() => open({ type: 'create-project' })}>
            <Plus className="h-4 w-4" />
            Tạo project mới
          </Button>
        </div>
      )}

      {/* ── Grid ───────────────────────────────────────────────────────── */}
      {!isLoading && !error && projects.length > 0 && (
        <>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {projects.map((project) => {
              const status = project.status as CProjectStatus
              const stepDisplay = Math.min(project.currentStepNumber, 5)

              const isSelected = project.id === activeProjectId

              return (
                <div
                  key={project.id}
                  className={`group relative cursor-pointer overflow-hidden rounded-xl border bg-white dark:bg-slate-900 shadow-sm transition-all hover:shadow-md ${
                    isSelected
                      ? 'border-indigo-400 dark:border-indigo-500 ring-2 ring-indigo-200 dark:ring-indigo-800'
                      : 'border-slate-200 dark:border-slate-700'
                  }`}
                  onClick={() => handleCardClick(project)}
                  onKeyDown={(e) => { if (e.key === 'Enter') handleCardClick(project) }}
                  tabIndex={0}
                  role="button"
                >
                  {/* Cover */}
                  <div className="relative h-32 bg-slate-100 dark:bg-slate-800">
                    {project.thumbnailCoverUrl ? (
                      <img src={project.thumbnailCoverUrl} alt={project.name} className="h-full w-full object-cover" />
                    ) : (
                      <div className="flex h-full items-center justify-center">
                        <Archive className="h-8 w-8 text-slate-300 dark:text-slate-600" />
                      </div>
                    )}
                    <span className={`absolute left-2 top-2 inline-flex items-center rounded-md px-2 py-0.5 text-xs font-medium ${getProjectStatusBadgeClass(status)}`}>
                      {getProjectStatusLabel(status)}
                    </span>
                    <span className="absolute right-2 top-2 inline-flex items-center rounded-md bg-black/50 px-2 py-0.5 text-xs font-medium text-white">
                      {stepDisplay}/5
                    </span>
                    <div className="absolute inset-0 flex items-start justify-end gap-1 bg-black/0 p-2 opacity-0 transition-all group-hover:bg-black/20 group-hover:opacity-100">
                      <Button
                        variant="secondary"
                        size="icon"
                        className="h-7 w-7"
                        onClick={(e) => {
                          e.stopPropagation()
                          setRenameTarget(project)
                          setRenameValue(project.name)
                        }}
                        title="Đổi tên"
                      >
                        <Pencil className="h-3.5 w-3.5" />
                      </Button>
                      <Button
                        variant="secondary"
                        size="icon"
                        className="h-7 w-7 hover:bg-red-100 hover:dark:bg-red-950/40 hover:text-red-600 hover:dark:text-red-400"
                        onClick={(e) => {
                          e.stopPropagation()
                          setDeleteTarget(project)
                        }}
                        title="Xoá"
                      >
                        <Trash2 className="h-3.5 w-3.5" />
                      </Button>
                    </div>
                  </div>

                  {/* Body */}
                  <div className="p-3">
                    <h3 className="truncate text-sm font-semibold text-slate-900 dark:text-slate-100">
                      {project.name}
                    </h3>
                    <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
                      {project.lastActivityTime
                        ? `Hoạt động: ${formatDateTime(project.lastActivityTime)}`
                        : `Tạo: ${formatDateTime(project.creationTime)}`}
                    </p>
                  </div>
                </div>
              )
            })}
          </div>

          {/* ── Pagination ──────────────────────────────────────────────── */}
          <DataTablePagination
            page={page}
            pageSize={pageSize}
            totalPages={totalPagesFiltered}
            totalCount={totalFiltered}
            onPageChange={setPage}
            onPageSizeChange={(size) => { setPageSize(size); setPage(1) }}
          />
        </>
      )}

      {/* ── Dialogs ────────────────────────────────────────────────────── */}
      <ConfirmDialog
        open={deleteTarget !== null}
        onClose={() => setDeleteTarget(null)}
        title="Xoá project"
        message={deleteTarget ? `Bạn có chắc chắn muốn xoá project "${deleteTarget.name}"? Hành động này không thể hoàn tác.` : ''}
        confirmLabel={isDeleting ? 'Đang xoá…' : 'Xoá'}
        variant="danger"
        onConfirm={handleDeleteConfirm}
        loading={isDeleting}
      />

      <ConfirmDialog
        open={renameTarget !== null}
        onClose={() => { setRenameTarget(null); setRenameValue('') }}
        title="Đổi tên project"
        message={
          <div className="mt-2">
            <Input value={renameValue} onChange={(e) => setRenameValue(e.target.value)} placeholder="Nhập tên mới" autoFocus />
          </div>
        }
        confirmLabel={isRenaming ? 'Đang lưu…' : 'Lưu'}
        variant="warning"
        onConfirm={handleRenameConfirm}
        loading={isRenaming}
      />
    </div>
  )
}
