import { useMemo, useState, useCallback } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Play, XCircle, Trash2, FileText, ExternalLink } from 'lucide-react'
import { toast } from 'sonner'
import { DataTable } from '../../components/data-table/data-table'
import { useDataTableState } from '../../components/data-table/use-data-table-state'
import { Button } from '../../components/ui/button'
import { Badge } from '../../components/ui/badge'
import { ConfirmDialog } from '../../components/ui/confirm-dialog'
import { DialogProvider, DialogContent } from '../../components/ui/dialog'
import { useSidebarContext } from '../../lib/sidebar-context'
import { formatDateTime } from '../../lib/date-formatter'
import { StatusFilterGroup, type StatusFilter } from '../../components/filters/status-filter'
import {
  getScheduledJobsWithPaging,
  cancelScheduledJob,
  deleteScheduledJob,
  retryScheduledJob,
  getJobLogs,
  type ScheduledJobSummaryDto,
  type LogEntry,
} from './api'
import { getJobStatusLabel, getJobStatusBadgeClass, getGoogleServiceTypeLabel } from './schema'
import {
  SortDirection,
  type SortRequestDto,
} from '../../types/paging.types'

export function ScheduledJobsPage() {
  const { open } = useSidebarContext()
  const queryClient = useQueryClient()
  const { page, setPage, pageSize, setPageSize, search, setSearch, sortBy, sortOrder, handleSort } =
    useDataTableState(1, 10)

  const [statusFilter, setStatusFilter] = useState<StatusFilter>('active')
  const [deleteTarget, setDeleteTarget] = useState<ScheduledJobSummaryDto | null>(null)
  const [cancelTarget, setCancelTarget] = useState<ScheduledJobSummaryDto | null>(null)
  const [logTarget, setLogTarget] = useState<ScheduledJobSummaryDto | null>(null)
  const [logs, setLogs] = useState<LogEntry[]>([])
  const [isLogsLoading, setIsLogsLoading] = useState(false)

  const { mutate: remove, isPending: isDeleting } = useMutation({
    mutationFn: (id: string) => deleteScheduledJob(id),
    onSuccess: () => {
      toast.success('Đã xoá job.')
      queryClient.invalidateQueries({ queryKey: ['scheduled-jobs'] })
    },
  })

  const { mutate: cancel, isPending: isCancelling } = useMutation({
    mutationFn: (id: string) => cancelScheduledJob(id),
    onSuccess: () => {
      toast.success('Đã huỷ job.')
      queryClient.invalidateQueries({ queryKey: ['scheduled-jobs'] })
    },
  })

  const { mutate: retry, isPending: isRetrying } = useMutation({
    mutationFn: (id: string) => retryScheduledJob(id),
    onSuccess: () => {
      toast.success('Đã yêu cầu chạy lại job.')
      queryClient.invalidateQueries({ queryKey: ['scheduled-jobs'] })
    },
  })

  const handleDeleteConfirm = useCallback(() => {
    if (!deleteTarget) return
    remove(deleteTarget.id, { onSuccess: () => setDeleteTarget(null) })
  }, [deleteTarget, remove])

  const handleCancelConfirm = useCallback(() => {
    if (!cancelTarget) return
    cancel(cancelTarget.id, { onSuccess: () => setCancelTarget(null) })
  }, [cancelTarget, cancel])

  const handleViewLogs = useCallback(async (job: ScheduledJobSummaryDto) => {
    setLogTarget(job)
    setIsLogsLoading(true)
    try {
      const result = await getJobLogs(job.id)
      setLogs(result)
    } catch {
      toast.error('Không thể tải logs.')
      setLogs([])
    } finally {
      setIsLogsLoading(false)
    }
  }, [])

  const includeDeleted = statusFilter === 'deleted' ? true : statusFilter === 'all' ? true : undefined
  const deletedOnly = statusFilter === 'deleted' ? true : undefined

  const backendSorts = useMemo<SortRequestDto[]>(() => {
    if (!sortBy || !sortOrder) return []
    const fieldMap: Record<string, string> = {
      name: 'Name',
      status: 'Status',
      creationTime: 'CreationTime',
      scheduledAt: 'ScheduledAt',
    }
    return [{ field: fieldMap[sortBy] || 'CreationTime', direction: sortOrder === 'asc' ? SortDirection.Asc : SortDirection.Desc }]
  }, [sortBy, sortOrder])

  const { data, isLoading, error } = useQuery({
    queryKey: ['scheduled-jobs', page, pageSize, search, statusFilter, backendSorts],
    queryFn: () =>
      getScheduledJobsWithPaging({
        pageNumber: page,
        pageSize,
        searchText: search || undefined,
        sorts: backendSorts,
        includeDeleted,
        deletedOnly,
      }),
  })

  const columns = useMemo(
    () => [
      {
        key: 'name',
        header: 'Tên job',
        sortable: true,
        className: 'font-medium',
        render: (row: ScheduledJobSummaryDto) => (
          <div className="flex items-center gap-2">
            <FileText className="h-4 w-4 text-blue-500" />
            <div className="flex flex-col">
              <span>{row.name}</span>
              <span className="text-xs text-slate-400">{getGoogleServiceTypeLabel(row.sourceType)}</span>
            </div>
          </div>
        ),
      },
      {
        key: 'sourceUrl',
        header: 'URL nguồn',
        render: (row: ScheduledJobSummaryDto) => (
          <a
            href={row.sourceUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="flex items-center gap-1 text-xs text-blue-500 hover:underline truncate max-w-[200px]"
          >
            <ExternalLink className="h-3 w-3 shrink-0" />
            <span className="truncate">{row.sourceUrl}</span>
          </a>
        ),
      },
      {
        key: 'serviceAccountName',
        header: 'Service Account',
        render: (row: ScheduledJobSummaryDto) => (
          <span className="text-xs text-slate-500">{row.serviceAccountName || '—'}</span>
        ),
      },
      {
        key: 'status',
        header: 'Trạng thái',
        sortable: true,
        render: (row: ScheduledJobSummaryDto) => (
          <Badge className={getJobStatusBadgeClass(row.status)}>
            {getJobStatusLabel(row.status)}
          </Badge>
        ),
      },
      {
        key: 'scheduledAt',
        header: 'Lịch chạy',
        sortable: true,
        render: (row: ScheduledJobSummaryDto) => (
          <span className="text-xs text-slate-500">
            {row.scheduledAt ? formatDateTime(row.scheduledAt) : 'Ngay lập tức'}
          </span>
        ),
      },
      {
        key: 'creationTime',
        header: 'Ngày tạo',
        sortable: true,
        render: (row: ScheduledJobSummaryDto) => (
          <span className="text-xs text-slate-500">{formatDateTime(row.creationTime)}</span>
        ),
      },
      {
        key: 'actions',
        header: '',
        className: 'w-[140px] text-right',
        render: (row: ScheduledJobSummaryDto) => (
          <div className="flex justify-end gap-1">
            <Button
              variant="ghost"
              size="icon"
              onClick={() => handleViewLogs(row)}
              title="Xem logs"
            >
              <FileText className="h-4 w-4" />
            </Button>
            {row.status === 1 && (
              <Button
                variant="ghost"
                size="icon"
                onClick={() => setCancelTarget(row)}
                title="Huỷ job"
                className="text-orange-500 hover:text-orange-700"
              >
                <XCircle className="h-4 w-4" />
              </Button>
            )}
            {row.status === 4 && (
              <Button
                variant="ghost"
                size="icon"
                onClick={() => retry(row.id)}
                disabled={isRetrying}
                title="Chạy lại"
                className="text-green-500 hover:text-green-700"
              >
                <Play className="h-4 w-4" />
              </Button>
            )}
            {(row.status === 3 || row.status === 4 || row.status === 5) && (
              <Button
                variant="ghost"
                size="icon"
                onClick={() => setDeleteTarget(row)}
                title="Xoá"
                className="text-red-500 hover:text-red-700"
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            )}
          </div>
        ),
      },
    ],
    [handleViewLogs, retry, isRetrying, setCancelTarget, setDeleteTarget],
  )

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold text-slate-800 dark:text-slate-100">
          Scheduled Import Jobs
        </h1>
          <Button onClick={() => open({ type: 'create-scheduled-job' })}>
          <Plus className="mr-1 h-4 w-4" /> Tạo Job mới
        </Button>
      </div>

      <StatusFilterGroup value={statusFilter} onChange={setStatusFilter} />

      <DataTable
        data={data?.items ?? []}
        columns={columns}
        isLoading={isLoading}
        error={(error as Error)?.message}
        page={page}
        pageSize={pageSize}
        totalCount={data?.totalCount ?? 0}
        totalPages={data?.totalPages ?? 0}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        search={search}
        onSearchChange={setSearch}
        sortBy={sortBy}
        sortOrder={sortOrder}
        onSort={handleSort}
        emptyMessage="Chưa có Scheduled Import Job nào."
      />

      {/* Cancel confirm */}
      <ConfirmDialog
        open={cancelTarget !== null}
        onClose={() => setCancelTarget(null)}
        onConfirm={handleCancelConfirm}
        title="Huỷ Job"
        message={`Bạn có chắc muốn huỷ job "${cancelTarget?.name}"?`}
        confirmLabel={isCancelling ? 'Đang huỷ...' : 'Huỷ'}
        variant="danger"
      />

      {/* Delete confirm */}
      <ConfirmDialog
        open={deleteTarget !== null}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDeleteConfirm}
        title="Xoá Job"
        message={`Bạn có chắc muốn xoá job "${deleteTarget?.name}"?`}
        confirmLabel={isDeleting ? 'Đang xoá...' : 'Xoá'}
        variant="danger"
      />

      {/* Logs dialog */}
      <DialogProvider open={logTarget !== null} setOpen={() => setLogTarget(null)}>
        <DialogContent className="max-w-2xl max-h-[70vh] overflow-y-auto">
          <h3 className="text-lg font-semibold mb-4 text-slate-800 dark:text-slate-100">
            Logs: {logTarget?.name}
          </h3>
          {isLogsLoading ? (
            <div className="py-8 text-center text-sm text-slate-400">Đang tải logs...</div>
          ) : logs.length === 0 ? (
            <div className="py-8 text-center text-sm text-slate-400">Chưa có log nào.</div>
          ) : (
            <div className="flex flex-col gap-1">
              {logs.map((log, i) => (
                <div
                  key={i}
                  className={`rounded px-3 py-1.5 text-xs font-mono ${
                    log.level === 'Error'
                      ? 'bg-red-50 text-red-700 dark:bg-red-900/20 dark:text-red-300'
                      : log.level === 'Warning'
                      ? 'bg-yellow-50 text-yellow-700 dark:bg-yellow-900/20 dark:text-yellow-300'
                      : 'bg-slate-50 text-slate-600 dark:bg-slate-800 dark:text-slate-300'
                  }`}
                >
                  <span className="opacity-60 mr-2">
                    {new Date(log.timestamp).toLocaleString('vi-VN')}
                  </span>
                  <span className="font-bold mr-1">[{log.level}]</span>
                  {log.message}
                </div>
              ))}
            </div>
          )}
        </DialogContent>
      </DialogProvider>
    </div>
  )
}
