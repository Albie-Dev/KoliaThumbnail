import { useMemo, useState, useCallback, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Pencil, Trash2, KeyRound } from 'lucide-react'
import { toast } from 'sonner'
import { DataTable } from '../../components/data-table/data-table'
import { useDataTableState } from '../../components/data-table/use-data-table-state'
import { Button } from '../../components/ui/button'
import { ConfirmDialog } from '../../components/ui/confirm-dialog'
import { Badge } from '../../components/ui/badge'
import { useSidebarContext } from '../../lib/sidebar-context'
import { formatDateTime } from '../../lib/date-formatter'
import { StatusFilterGroup, type StatusFilter } from '../../components/filters/status-filter'
import {
  getGoogleServiceAccountsWithPaging,
  deleteGoogleServiceAccount,
  type GoogleServiceAccountSummaryDto,
} from './api'
import {
  SortDirection,
  type SortRequestDto,
} from '../../types/paging.types'

export function GoogleServicesPage() {
  const { open } = useSidebarContext()
  const queryClient = useQueryClient()
  const { page, setPage, pageSize, setPageSize, search, setSearch, sortBy, sortOrder, handleSort } =
    useDataTableState(1, 10)

  const [statusFilter, setStatusFilter] = useState<StatusFilter>('active')
  const [localStatusFilter, setLocalStatusFilter] = useState<StatusFilter>(statusFilter)
  const [deleteTarget, setDeleteTarget] = useState<GoogleServiceAccountSummaryDto | null>(null)

  const { mutate: remove, isPending: isDeleting } = useMutation({
    mutationFn: (id: string) => deleteGoogleServiceAccount(id),
    onSuccess: () => {
      toast.success('Đã xoá service account thành công.')
      queryClient.invalidateQueries({ queryKey: ['google-services'] })
    },
  })

  const handleRefresh = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ['google-services'] })
  }, [queryClient])

  const handleDeleteConfirm = useCallback(() => {
    if (!deleteTarget) return
    remove(deleteTarget.id, {
      onSuccess: () => setDeleteTarget(null),
    })
  }, [deleteTarget, remove])

  // Sync local status filter when applied status changes (e.g. sidebar opens)
  useEffect(() => {
    setLocalStatusFilter(statusFilter)
  }, [statusFilter])

  const handleApplyFilter = useCallback(() => {
    setStatusFilter(localStatusFilter)
  }, [localStatusFilter])

  const handleResetFilter = useCallback(() => {
    setLocalStatusFilter('active')
    setStatusFilter('active')
  }, [])

  const includeDeleted = statusFilter === 'deleted' ? true : statusFilter === 'all' ? true : undefined
  const deletedOnly = statusFilter === 'deleted' ? true : undefined

  const backendSorts = useMemo<SortRequestDto[]>(() => {
    if (!sortBy || !sortOrder) return []
    const fieldMap: Record<string, string> = {
      name: 'Name',
      clientEmail: 'ClientEmail',
      creationTime: 'CreationTime',
    }
    return [{ field: fieldMap[sortBy] || 'CreationTime', direction: sortOrder === 'asc' ? SortDirection.Asc : SortDirection.Desc }]
  }, [sortBy, sortOrder])

  const { data, isLoading, error } = useQuery({
    queryKey: ['google-services', page, pageSize, search, statusFilter, backendSorts],
    queryFn: () =>
      getGoogleServiceAccountsWithPaging({
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
        header: 'Tên',
        sortable: true,
        className: 'font-medium',
        render: (row: GoogleServiceAccountSummaryDto) => (
          <div className="flex items-center gap-2">
            <KeyRound className="h-4 w-4 text-amber-500" />
            <span>{row.name}</span>
          </div>
        ),
      },
      {
        key: 'clientEmail',
        header: 'Email',
        sortable: true,
        render: (row: GoogleServiceAccountSummaryDto) => (
          <span className="text-xs text-slate-500 dark:text-slate-400">{row.clientEmail}</span>
        ),
      },
      {
        key: 'projectId',
        header: 'Project ID',
        render: (row: GoogleServiceAccountSummaryDto) => (
          <span className="text-xs text-slate-500">{row.projectId || '—'}</span>
        ),
      },
      {
        key: 'isEnabled',
        header: 'Trạng thái',
        render: (row: GoogleServiceAccountSummaryDto) => (
          row.isEnabled
            ? <Badge variant="success">Đang hoạt động</Badge>
            : <Badge variant="warning">Đã tắt</Badge>
        ),
      },
      {
        key: 'totalJobs',
        header: 'Số jobs',
        render: (row: GoogleServiceAccountSummaryDto) => (
          <span className="text-xs text-slate-500">{row.totalJobs}</span>
        ),
      },
      {
        key: 'creationTime',
        header: 'Ngày tạo',
        sortable: true,
        render: (row: GoogleServiceAccountSummaryDto) => (
          <span className="text-xs text-slate-500">{formatDateTime(row.creationTime)}</span>
        ),
      },
      {
        key: 'actions',
        header: '',
        className: 'w-[100px] text-right',
        render: (row: GoogleServiceAccountSummaryDto) => (
          <div className="flex justify-end gap-1">
            <Button
              variant="ghost"
              size="icon"
              onClick={() => open({ type: 'edit-google-service', id: row.id })}
            >
              <Pencil className="h-4 w-4" />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              onClick={() => setDeleteTarget(row)}
              className="text-red-500 hover:text-red-700"
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        ),
      },
    ],
    [open],
  )

  const filterContent = (
    <div className="space-y-5">
      <StatusFilterGroup value={localStatusFilter} onChange={setLocalStatusFilter} />
    </div>
  )

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center gap-4">
        <h1 className="text-xl font-semibold text-slate-800 dark:text-slate-100">
          Google Service Accounts
        </h1>
      </div>

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
        filterContent={filterContent}
        onApplyFilter={handleApplyFilter}
        onResetFilter={handleResetFilter}
        onRetry={handleRefresh}
        actions={
          <Button onClick={() => open({ type: 'create-google-service' })} className="gap-2">
            <Plus className="h-4 w-4" />
            Thêm mới
          </Button>
        }
        emptyMessage="Chưa có Google Service Account nào."
      />

      <ConfirmDialog
        open={deleteTarget !== null}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDeleteConfirm}
        title="Xoá Service Account"
        message={`Bạn có chắc chắn muốn xoá "${deleteTarget?.name}"? Hành động này có thể ảnh hưởng đến các job đang sử dụng.`}
        confirmLabel={isDeleting ? 'Đang xoá...' : 'Xoá'}
        variant="danger"
      />
    </div>
  )
}
