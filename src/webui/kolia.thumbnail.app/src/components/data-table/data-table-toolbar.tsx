import { useRef, useState, useEffect, type ReactNode } from 'react'
import { Search, Columns3, SlidersHorizontal, X, Check, RefreshCw } from 'lucide-react'
import { Input } from '../ui/input'
import { Button } from '../ui/button'
import { cn } from '../../lib/utils'

export interface ColumnDef {
  key: string
  header: string
}

interface DataTableToolbarProps {
  /** Slot bên trái — các action buttons (Thêm mới, Export, …) */
  actions?: ReactNode
  /** Callback refresh — hiển thị nút Refresh bên phải */
  onRefresh?: () => void
  isRefreshing?: boolean
  /** Search */
  search?: string
  searchPlaceholder?: string
  onSearchChange?: (value: string) => void
  onSearchClear?: () => void
  /** Columns toggle */
  columns?: ColumnDef[]
  visibleColumns?: Set<string>
  onToggleColumn?: (key: string) => void
  /** Filter sidebar toggle — ẩn nút nếu không truyền onOpenFilter */
  onOpenFilter?: () => void
}

export function DataTableToolbar({
  actions,
  onRefresh,
  isRefreshing = false,
  search = '',
  searchPlaceholder = 'Tìm kiếm…',
  onSearchChange,
  onSearchClear,
  columns = [],
  visibleColumns,
  onToggleColumn,
  onOpenFilter,
}: DataTableToolbarProps) {
  const [localSearch, setLocalSearch] = useState(search)
  const [columnsOpen, setColumnsOpen] = useState(false)
  const columnsRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    setLocalSearch(search)
  }, [search])

  useEffect(() => {
    const handler = setTimeout(() => {
      if (localSearch !== search) {
        onSearchChange?.(localSearch)
      }
    }, 400)
    return () => clearTimeout(handler)
  }, [localSearch, search, onSearchChange])

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (columnsRef.current && !columnsRef.current.contains(event.target as Node)) {
        setColumnsOpen(false)
      }
    }
    if (columnsOpen) {
      document.addEventListener('mousedown', handleClickOutside)
    }
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [columnsOpen])

  const handleClear = () => {
    setLocalSearch('')
    onSearchClear?.()
  }

  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      {/* Left — Actions */}
      <div className="flex flex-wrap items-center gap-2">
        {actions && <div className="flex flex-wrap items-center gap-2">{actions}</div>}
      </div>

      {/* Right — Search / Columns / Filter / Refresh */}
      <div className="flex flex-wrap items-center gap-2">
        {/* Search — h-9 khớp với Button size default */}
        {onSearchChange && (
          <div className="relative">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400 dark:text-slate-500" />
            <Input
              value={localSearch}
              onChange={(e) => setLocalSearch(e.target.value)}
              placeholder={searchPlaceholder}
              className="h-9 w-56 pl-9 pr-8"
            />
            {localSearch && (
              <button
                type="button"
                onClick={handleClear}
                className="absolute right-2 top-1/2 -translate-y-1/2 rounded p-0.5 text-slate-400 dark:text-slate-500 hover:text-slate-600 hover:dark:text-slate-300"
              >
                <X className="h-3.5 w-3.5" />
              </button>
            )}
          </div>
        )}

        {/* Columns Toggle Dropdown — h-9 */}
        {columns.length > 0 && onToggleColumn && visibleColumns && (
          <div ref={columnsRef} className="relative">
            <Button
              variant="outline"
              size="default"
              className="h-9 gap-1.5"
              onClick={() => setColumnsOpen((prev) => !prev)}
            >
              <Columns3 className="h-4 w-4" />
              <span className="hidden sm:inline">Cột</span>
            </Button>

            {columnsOpen && (
              <div className="absolute right-0 top-full z-50 mt-1 w-48 rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 py-1 shadow-lg">
                <p className="px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-slate-400 dark:text-slate-500">
                  Hiển thị cột
                </p>
                {columns.map((col) => {
                  const isVisible = visibleColumns.has(col.key)
                  return (
                    <button
                      key={col.key}
                      type="button"
                      onClick={() => onToggleColumn(col.key)}
                      className={cn(
                        'flex w-full items-center gap-2 px-3 py-2 text-sm transition-colors hover:bg-slate-50 hover:dark:bg-slate-900',
                        isVisible ? 'text-slate-800 dark:text-slate-100' : 'text-slate-400 dark:text-slate-500',
                      )}
                    >
                      <span
                        className={cn(
                          'flex h-4 w-4 items-center justify-center rounded border transition-colors',
                          isVisible
                            ? 'border-slate-900 bg-slate-900 text-white dark:border-slate-100 dark:bg-slate-100 dark:text-slate-900'
                            : 'border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900',
                        )}
                      >
                        {isVisible && <Check className="h-3 w-3" />}
                      </span>
                      {col.header}
                    </button>
                  )
                })}
              </div>
            )}
          </div>
        )}

        {/* Filter — h-9 */}
        {onOpenFilter && (
          <Button
            variant="outline"
            size="default"
            className="h-9 gap-1.5"
            onClick={onOpenFilter}
          >
            <SlidersHorizontal className="h-4 w-4" />
            <span className="hidden sm:inline">Lọc</span>
          </Button>
        )}

        {/* Refresh button — di chuyển sang bên phải và nằm sau cùng, độ cao h-9 */}
        {onRefresh && (
          <Button
            variant="outline"
            size="default"
            className="h-9 w-9 p-0 shrink-0"
            onClick={onRefresh}
            disabled={isRefreshing}
            title="Làm mới"
          >
            <RefreshCw className={cn('h-4 w-4', isRefreshing && 'animate-spin')} />
          </Button>
        )}
      </div>
    </div>
  )
}
