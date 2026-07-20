import { useMemo, useState, useCallback } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Pencil } from 'lucide-react'
import { toast } from 'sonner'

import { Button } from '../../components/ui/button'
import { Badge } from '../../components/ui/badge'
import { DataTable } from '../../components/data-table/data-table'
import { useDataTableState } from '../../components/data-table/use-data-table-state'
import { ConfirmDialog } from '../../components/ui/confirm-dialog'

import { useSidebarContext } from '../../lib/sidebar-context'
import { formatDateTime } from '../../lib/date-formatter'

import { getFunctionConfigsPaging, type AIFunctionConfigSummaryDto } from './api'
import { getFunctionTypeLabel } from './function-type'
import { getAIProvidersWithPaging } from '../ai-providers/api'
import { fetchAIProviderConfigurations } from '../ai-configurations/api'
import { SortDirection } from '../../types/paging.types'

export function AiFunctionConfigsPage() {
  const { open } = useSidebarContext()
  const queryClient = useQueryClient()

  const { page, setPage, pageSize, setPageSize, search, setSearch, sortBy, sortOrder, handleSort } =
    useDataTableState(1, 10)

  const [deleteTarget, setDeleteTarget] = useState<AIFunctionConfigSummaryDto | null>(null)

  // Pre-fetch providers & configs for sidebar edit form (load once in background)
  useQuery({
    queryKey: ['ai-providers', 'all'],
    queryFn: async () => {
      const res = await getAIProvidersWithPaging({ pageNumber: 1, pageSize: 1000, includeDeleted: false })
      return res.items ?? []
    },
    staleTime: 5 * 60 * 1000,
  })

  useQuery({
    queryKey: ['ai-configurations', 'all'],
    queryFn: async () => {
      const res = await fetchAIProviderConfigurations({ pageNumber: 1, pageSize: 1000, includeDeleted: false })
      return res.items ?? []
    },
    staleTime: 5 * 60 * 1000,
  })

  const { mutate: remove, isPending: isDeleting } = useMutation({
    mutationFn: (id: string) => deleteFunctionConfig(id),
    onSuccess: () => {
      toast.success('Đã xoá cấu hình chức năng.')
      queryClient.invalidateQueries({ queryKey: ['ai-function-configs'] })
    },
  })

  const handleDeleteConfirm = useCallback(() => {
    if (!deleteTarget) return
    remove(deleteTarget.id, { onSuccess: () => setDeleteTarget(null) })
  }, [deleteTarget, remove])

  const backendSorts = useMemo(() => {
    if (!sortBy || !sortOrder) return []
    const fieldMap: Record<string, string> = {
      functionType: 'FunctionType',
      model: 'Model',
      creationTime: 'CreationTime',
    }
    return [{ field: fieldMap[sortBy] ?? 'FunctionType', direction: sortOrder === 'desc' ? SortDirection.Desc : SortDirection.Asc }]
  }, [sortBy, sortOrder])

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['ai-function-configs', page, pageSize, search, sortBy, sortOrder],
    queryFn: () =>
      getFunctionConfigsPaging({
        pageNumber: page,
        pageSize,
        searchText: search || undefined,
        sorts: backendSorts.length > 0 ? backendSorts : undefined,
      }),
  })

  const handleEdit = async (id: string) => {
    try {
      const { getFunctionConfigById } = await import('./api')
      const detail = await getFunctionConfigById(id)
      open({ type: 'edit-ai-function-config', config: detail })
    } catch {
      toast.error('Không thể tải thông tin cấu hình.')
    }
  }

  const columns = useMemo(
    () => [
      {
        key: 'functionType',
        header: 'Chức năng',
        sortable: true,
        render: (item: AIFunctionConfigSummaryDto) => (
          <Badge variant="outline" className="text-xs font-medium">
            {getFunctionTypeLabel(item.functionType)}
          </Badge>
        ),
      },
      {
        key: 'model',
        header: 'Model',
        sortable: true,
        render: (item: AIFunctionConfigSummaryDto) => (
          <span className="font-mono text-xs">{item.model || <span className="text-slate-400">—</span>}</span>
        ),
      },
      {
        key: 'primaryProviderName',
        header: 'Provider',
        render: (item: AIFunctionConfigSummaryDto) => (
          <span>{item.primaryProviderName || <span className="text-slate-400">—</span>}</span>
        ),
      },
      {
        key: 'primaryConfigName',
        header: 'Config',
        render: (item: AIFunctionConfigSummaryDto) => (
          <span>{item.primaryConfigName || <span className="text-slate-400">—</span>}</span>
        ),
      },
      {
        key: 'fallbackCount',
        header: 'Fallback',
        render: (item: AIFunctionConfigSummaryDto) =>
          item.fallbackCount > 0 ? (
            <Badge variant="secondary" className="text-xs">
              {item.fallbackCount}
            </Badge>
          ) : (
            <span className="text-xs text-slate-400">—</span>
          ),
      },
      {
        key: 'creationTime',
        header: 'Tạo lúc',
        sortable: true,
        render: (item: AIFunctionConfigSummaryDto) => (
          <span className="text-xs text-slate-500">{formatDateTime(item.creationTime)}</span>
        ),
      },
      {
        key: 'actions',
        header: '',
        render: (item: AIFunctionConfigSummaryDto) => (
          <div className="flex justify-end gap-1">
            <Button
              variant="ghost"
              size="sm"
              onClick={(e) => {
                e.stopPropagation()
                handleEdit(item.id)
              }}
              title="Chỉnh sửa"
            >
              <Pencil className="h-4 w-4" />
            </Button>
          </div>
        ),
      },
    ],
    [],
  )

  return (
    <div className="mx-auto max-w-6xl space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-slate-900 dark:text-slate-100">Cấu hình chức năng AI</h1>
          <p className="text-sm text-slate-500 dark:text-slate-400 mt-1">
            Quản lý provider, config, model và fallback cho từng chức năng
          </p>
        </div>
      </div>

      <DataTable
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        error={error instanceof Error ? error.message : null}
        onRetry={() => void refetch()}
        page={page}
        pageSize={pageSize}
        totalCount={data?.totalCount ?? 0}
        totalPages={data?.totalPages ?? 1}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        search={search}
        onSearchChange={setSearch}
        sortBy={sortBy}
        sortOrder={sortOrder}
        onSort={handleSort}
        searchPlaceholder="Tìm kiếm chức năng..."
      />

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        title="Xoá cấu hình chức năng"
        message={`Bạn có chắc muốn xoá cấu hình cho "${deleteTarget ? getFunctionTypeLabel(deleteTarget.functionType) : ''}"?`}
        variant="danger"
        confirmLabel={isDeleting ? 'Đang xoá…' : 'Xoá'}
        onConfirm={handleDeleteConfirm}
      />
    </div>
  )
}

async function deleteFunctionConfig(id: string): Promise<void> {
  const { httpClient } = await import('../../lib/api/http-client')
  await httpClient.delete(`/api/v1/ai-function-configs/${id}`)
}
