import type { ReactNode } from 'react'
import { DataTableEmptyState } from './data-table-empty-state'
import { DataTableErrorState } from './data-table-error-state'
import { DataTablePagination } from './data-table-pagination'
import { DataTableSkeleton } from './data-table-skeleton'

interface DataTableProps<T> {
  columns: Array<{
    key: string
    header: string
    render?: (item: T) => ReactNode
  }>
  data: T[]
  isLoading: boolean
  error?: string | null
  onRetry?: () => void
  emptyMessage?: string
  title?: string
  page?: number
  pageSize?: number
  totalPages?: number
  totalCount?: number
  onPageChange?: (page: number) => void
  onPageSizeChange?: (size: number) => void
}

export function DataTable<T>({
  columns,
  data,
  isLoading,
  error,
  onRetry,
  emptyMessage = 'Không có dữ liệu',
  title = 'Danh sách',
  page,
  pageSize,
  totalPages,
  totalCount,
  onPageChange,
  onPageSizeChange,
}: DataTableProps<T>) {
  if (isLoading) {
    return <DataTableSkeleton title={title} />
  }

  if (error) {
    return <DataTableErrorState title={title} message={error} onRetry={onRetry} />
  }

  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
      <div className="mb-4 flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-slate-900">{title}</h2>
          <p className="text-sm text-slate-500">Trang dữ liệu hiện tại</p>
        </div>
      </div>
      {data.length === 0 ? (
        <DataTableEmptyState title={title} message={emptyMessage} />
      ) : (
        <>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-slate-200">
              <thead>
                <tr className="bg-slate-50 text-left text-sm font-semibold text-slate-700">
                  {columns.map((column) => (
                    <th key={column.key} className="px-4 py-3">
                      {column.header}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 bg-white">
                {data.map((item, index) => (
                  <tr key={index} className="text-sm text-slate-700 hover:bg-slate-50">
                    {columns.map((column) => (
                      <td key={column.key} className="px-4 py-3">
                        {column.render ? column.render(item) : null}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {typeof page === 'number' && typeof pageSize === 'number' && typeof totalPages === 'number' && typeof totalCount === 'number' ? (
            <DataTablePagination
              page={page}
              pageSize={pageSize}
              totalPages={totalPages}
              totalCount={totalCount}
              onPageChange={onPageChange ?? (() => undefined)}
              onPageSizeChange={onPageSizeChange}
            />
          ) : null}
        </>
      )}
    </section>
  )
}
