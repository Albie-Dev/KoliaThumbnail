import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from 'lucide-react'
import { Button } from '../ui/button'
import { SelectDropdown } from '../selects/select-dropdown'

interface DataTablePaginationProps {
  page: number
  pageSize: number
  totalPages: number
  totalCount: number
  onPageChange: (page: number) => void
  onPageSizeChange?: (size: number) => void
}

export function DataTablePagination({
  page,
  pageSize,
  totalPages,
  totalCount,
  onPageChange,
  onPageSizeChange,
}: DataTablePaginationProps) {
  // Generate pages to display in pagination bar (e.g. 1, 2, 3...)
  const getPageNumbers = () => {
    const pages = []
    const range = 1 // Show current page +/- range

    for (let i = 1; i <= totalPages; i++) {
      if (
        i === 1 ||
        i === totalPages ||
        (i >= page - range && i <= page + range)
      ) {
        pages.push(i)
      } else if (pages[pages.length - 1] !== '...') {
        pages.push('...')
      }
    }
    return pages
  }

  const pages = getPageNumbers()

  return (
    <div className="flex flex-col items-center justify-between gap-4 py-1.5 sm:flex-row">
      {/* Left side: details */}
      <div className="flex items-center gap-3 text-sm text-slate-500">
        <span className="font-medium text-slate-700">Tổng số {totalCount} mục</span>
        {onPageSizeChange && (
          <div className="flex items-center gap-1.5 border-l border-slate-200 pl-3">
            <span className="text-xs">Hiển thị</span>
            <SelectDropdown<number>
              items={[10, 20, 50, 100]}
              value={pageSize}
              onChange={(v) => v !== null && onPageSizeChange(v)}
              getOptionId={(item) => String(item)}
              getOptionLabel={(item) => `${item}`}
              allowSearch={false}
              placeholder="Hiển thị"
              className="w-[80px]"
            />
          </div>
        )}
      </div>

      {/* Right side: navigation */}
      <div className="flex items-center gap-1.5">
        {/* First page */}
        <Button
          variant="outline"
          size="sm"
          className="h-8 w-8 p-0"
          onClick={() => onPageChange(1)}
          disabled={page <= 1}
        >
          <ChevronsLeft className="h-4 w-4" />
        </Button>

        {/* Previous page */}
        <Button
          variant="outline"
          size="sm"
          className="h-8 w-8 p-0"
          onClick={() => onPageChange(page - 1)}
          disabled={page <= 1}
        >
          <ChevronLeft className="h-4 w-4" />
        </Button>

        {/* Page numbers */}
        <div className="flex items-center gap-1">
          {pages.map((p, idx) => {
            if (p === '...') {
              return (
                <span key={`dots-${idx}`} className="px-2 text-sm text-slate-400">
                  ...
                </span>
              )
            }

            const isCurrent = p === page
            return (
              <Button
                key={p}
                variant={isCurrent ? 'default' : 'outline'}
                size="sm"
                className={`h-8 w-8 p-0 text-xs font-semibold ${
                  isCurrent ? 'bg-slate-900 text-white hover:bg-slate-900' : 'text-slate-700 hover:bg-slate-50'
                }`}
                onClick={() => onPageChange(Number(p))}
              >
                {p}
              </Button>
            )
          })}
        </div>

        {/* Next page */}
        <Button
          variant="outline"
          size="sm"
          className="h-8 w-8 p-0"
          onClick={() => onPageChange(page + 1)}
          disabled={page >= totalPages}
        >
          <ChevronRight className="h-4 w-4" />
        </Button>

        {/* Last page */}
        <Button
          variant="outline"
          size="sm"
          className="h-8 w-8 p-0"
          onClick={() => onPageChange(totalPages)}
          disabled={page >= totalPages}
        >
          <ChevronsRight className="h-4 w-4" />
        </Button>
      </div>
    </div>
  )
}
