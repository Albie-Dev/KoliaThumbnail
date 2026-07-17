import { cn } from '../../lib/utils'

export type StatusFilter = 'all' | 'active' | 'deleted'

export const statusFilterOptions: {
  value: StatusFilter
  label: string
  dotClassName: string
}[] = [
  { value: 'all', label: 'Tất cả', dotClassName: 'bg-slate-400 dark:bg-slate-500' },
  { value: 'active', label: 'Hoạt động', dotClassName: 'bg-emerald-500' },
  { value: 'deleted', label: 'Đã xoá', dotClassName: 'bg-rose-500' },
]

interface StatusFilterGroupProps {
  value: StatusFilter
  onChange: (value: StatusFilter) => void
}

export function StatusFilterGroup({ value, onChange }: StatusFilterGroupProps) {
  const activeIndex = statusFilterOptions.findIndex((opt) => opt.value === value)
  const count = statusFilterOptions.length

  return (
    <div>
      <label className="block text-[10px] font-semibold text-slate-400 dark:text-slate-500 uppercase tracking-widest mb-1.5">
        Trạng thái
      </label>

      <div className="relative flex rounded-lg bg-slate-100 dark:bg-slate-800 p-0.5 ring-1 ring-inset ring-slate-200/80 dark:ring-slate-700/80">
        {/* Sliding indicator — width/left derived from the same padding box as the buttons, so it always lines up exactly */}
        <div
          className="absolute inset-y-0.5 left-0.5 rounded-md bg-white dark:bg-slate-900 shadow-[0_1px_2px_rgba(15,23,42,0.08)] ring-1 ring-slate-200/60 dark:ring-slate-700/60 transition-transform duration-300 ease-out"
          style={{
            width: `calc((100% - 4px) / ${count})`,
            transform: `translateX(calc(${activeIndex} * 100%))`,
          }}
        />

        {statusFilterOptions.map((opt) => {
          const isActive = value === opt.value
          return (
            <button
              key={opt.value}
              type="button"
              onClick={() => onChange(opt.value)}
              className={cn(
                'relative z-10 flex flex-1 items-center justify-center gap-1 whitespace-nowrap rounded-md px-2 py-1 text-[11px] font-medium leading-none transition-colors duration-200',
                isActive
                  ? 'text-slate-900 dark:text-slate-100'
                  : 'text-slate-500 dark:text-slate-400 hover:text-slate-700 hover:dark:text-slate-200',
              )}
            >
              <span
                className={cn(
                  'h-1 w-1 shrink-0 rounded-full transition-opacity duration-200',
                  opt.dotClassName,
                  isActive ? 'opacity-100' : 'opacity-40',
                )}
              />
              {opt.label}
            </button>
          )
        })}
      </div>
    </div>
  )
}