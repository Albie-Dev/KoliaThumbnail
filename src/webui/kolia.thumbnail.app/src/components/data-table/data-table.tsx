import { useState, useMemo, type ReactNode } from 'react'
import { ArrowUp, ArrowDown } from 'lucide-react'
import { DataTableEmptyState } from './data-table-empty-state'
import { DataTableErrorState } from './data-table-error-state'
import { DataTablePagination } from './data-table-pagination'
import { DataTableSkeleton } from './data-table-skeleton'
import { DataTableToolbar } from './data-table-toolbar'
import { DataTableFilterSidebar } from './data-table-filter-sidebar'
import { cn } from '../../lib/utils'

export interface DataTableColumn<T> {
  key: string
  header: ReactNode
  sortable?: boolean
  render?: (item: T) => ReactNode
}

interface DataTableProps<T> {
  columns: DataTableColumn<T>[]
  data: T[]
  isLoading: boolean
  error?: string | null
  onRetry?: () => void
  emptyMessage?: string
  title?: string
  // ── Pagination ────────────────────────────────────────
  page?: number
  pageSize?: number
  totalPages?: number
  totalCount?: number
  onPageChange?: (page: number) => void
  onPageSizeChange?: (size: number) => void
  // ── Sort ─────────────────────────────────────────────
  onSort?: (column: string) => void
  sortBy?: string
  sortOrder?: 'asc' | 'desc' | null
  // ── Toolbar — Left Actions ────────────────────────────
  /** Slot bên trái toolbar: <Button>, <DropdownMenu>, … */
  actions?: ReactNode
  // ── Toolbar — Search ─────────────────────────────────
  search?: string
  searchPlaceholder?: string
  onSearchChange?: (value: string) => void
  onSearchClear?: () => void
  // ── Filter Sidebar ────────────────────────────────────
  /** Body của Filter Sidebar. Nếu không truyền, nút Filter sẽ ẩn. */
  filterContent?: ReactNode
  onApplyFilter?: () => void
  onResetFilter?: () => void
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
  onSort,
  sortBy,
  sortOrder,
  actions,
  search,
  searchPlaceholder,
  onSearchChange,
  onSearchClear,
  filterContent,
  onApplyFilter,
  onResetFilter,
}: DataTableProps<T>) {
  // ── Columns visibility state ──────────────────────────
  const [hiddenColumns, setHiddenColumns] = useState<Set<string>>(new Set())
  const [isFilterOpen, setIsFilterOpen] = useState(false)

  const visibleColumns = useMemo(
    () => new Set(columns.filter((c) => !hiddenColumns.has(c.key)).map((c) => c.key)),
    [columns, hiddenColumns],
  )

  const displayedColumns = useMemo(
    () => columns.filter((c) => visibleColumns.has(c.key)),
    [columns, visibleColumns],
  )

  const handleToggleColumn = (key: string) => {
    setHiddenColumns((prev) => {
      const next = new Set(prev)
      if (next.has(key)) {
        next.delete(key)
      } else {
        next.add(key)
      }
      return next
    })
  }

  // ── Toolbar column defs (always all columns) ──────────
  const columnDefs = useMemo(
    () => columns.map((c) => ({ key: c.key, header: c.header })),
    [columns],
  )

  // ── Loading / Error early returns ─────────────────────
  if (isLoading) {
    return (
      <>
        {filterContent !== undefined && (
          <DataTableFilterSidebar
            isOpen={isFilterOpen}
            onClose={() => setIsFilterOpen(false)}
            onApply={onApplyFilter}
            onReset={onResetFilter}
          >
            {filterContent}
          </DataTableFilterSidebar>
        )}
        <DataTableSkeleton title={title} />
      </>
    )
  }

  if (error) {
    return (
      <>
        {filterContent !== undefined && (
          <DataTableFilterSidebar
            isOpen={isFilterOpen}
            onClose={() => setIsFilterOpen(false)}
            onApply={onApplyFilter}
            onReset={onResetFilter}
          >
            {filterContent}
          </DataTableFilterSidebar>
        )}
        <DataTableErrorState title={title} message={error} onRetry={onRetry} />
      </>
    )
  }

  return (
    <>
      {/* Filter Sidebar — rendered outside main flow so it overlays */}
      {filterContent !== undefined && (
        <DataTableFilterSidebar
          isOpen={isFilterOpen}
          onClose={() => setIsFilterOpen(false)}
          onApply={onApplyFilter}
          onReset={onResetFilter}
        >
          {filterContent}
        </DataTableFilterSidebar>
      )}

      <section className="rounded-2xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 shadow-sm">
        {/* ── Header ─────────────────────────────────── */}
        <div className="border-b border-slate-100 dark:border-slate-800 px-6 py-4">
          <DataTableToolbar
            actions={actions}
            onRefresh={onRetry}
            isRefreshing={isLoading}
            search={search}
            searchPlaceholder={searchPlaceholder}
            onSearchChange={onSearchChange}
            onSearchClear={onSearchClear}
            columns={columnDefs}
            visibleColumns={visibleColumns}
            onToggleColumn={handleToggleColumn}
            onOpenFilter={filterContent !== undefined ? () => setIsFilterOpen(true) : undefined}
          />
        </div>

        {/* ── Body ───────────────────────────────────── */}
        {data.length === 0 ? (
          <div className="px-6 py-6">
            <DataTableEmptyState title={title} message={emptyMessage} />
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-slate-200 dark:divide-slate-700">
              <thead>
                <tr className="bg-slate-50 dark:bg-slate-900 text-left text-sm font-semibold text-slate-700 dark:text-slate-200">
                  {displayedColumns.map((column) => (
                    <th
                      key={column.key}
                      className={cn(
                        'px-4 py-3',
                        column.sortable && 'cursor-pointer select-none hover:bg-slate-100 hover:dark:bg-slate-800 transition-colors',
                      )}
                      onClick={() => column.sortable && onSort?.(column.key)}
                    >
                      <div className="flex items-center gap-2">
                        <span>{column.header}</span>
                        {column.sortable && sortBy === column.key && (
                          sortOrder === 'asc' ? (
                            <ArrowUp className="h-4 w-4 text-blue-600 dark:text-blue-400" />
                          ) : (
                            <ArrowDown className="h-4 w-4 text-blue-600 dark:text-blue-400" />
                          )
                        )}
                      </div>
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 dark:divide-slate-800 bg-white dark:bg-slate-900">
                {data.map((item, index) => (
                  <tr key={index} className="text-sm text-slate-700 dark:text-slate-200 hover:bg-slate-50 hover:dark:bg-slate-900 transition-colors">
                    {displayedColumns.map((column) => (
                      <td key={column.key} className="px-4 py-3">
                        {column.render ? column.render(item) : null}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* ── Footer (Pagination) ─────────────────────── */}
        {typeof page === 'number' &&
          typeof pageSize === 'number' &&
          typeof totalPages === 'number' &&
          typeof totalCount === 'number' ? (
          <div className="border-t border-slate-100 dark:border-slate-800 px-6 py-4">
            <DataTablePagination
              page={page}
              pageSize={pageSize}
              totalPages={totalPages}
              totalCount={totalCount}
              onPageChange={onPageChange ?? (() => undefined)}
              onPageSizeChange={onPageSizeChange}
            />
          </div>
        ) : null}
      </section>
    </>
  )
}
