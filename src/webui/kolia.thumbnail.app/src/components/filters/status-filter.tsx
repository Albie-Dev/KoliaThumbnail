import { cn } from '../../lib/utils'

export type StatusFilter = 'all' | 'active' | 'deleted'

export const statusFilterOptions: { value: StatusFilter; label: string }[] = [
  { value: 'all', label: 'Tất cả' },
  { value: 'active', label: 'Đang hoạt động' },
  { value: 'deleted', label: 'Đã xoá' },
]

interface StatusFilterGroupProps {
  value: StatusFilter
  onChange: (value: StatusFilter) => void
}

export function StatusFilterGroup({ value, onChange }: StatusFilterGroupProps) {
  return (
    <div>
      <label className="block text-[10px] font-semibold text-slate-400 uppercase tracking-widest mb-1.5">
        Trạng thái
      </label>
      <div className="flex rounded-md border border-slate-200 bg-slate-50 p-px">
        {statusFilterOptions.map((opt) => (
          <button
            key={opt.value}
            type="button"
            onClick={() => onChange(opt.value)}
            className={cn(
              'flex-1 rounded px-2 py-1 text-xs font-medium transition-all duration-150',
              value === opt.value
                ? 'bg-white text-slate-900 shadow-sm'
                : 'text-slate-400 hover:text-slate-600',
            )}
          >
            {opt.label}
          </button>
        ))}
      </div>
    </div>
  )
}
