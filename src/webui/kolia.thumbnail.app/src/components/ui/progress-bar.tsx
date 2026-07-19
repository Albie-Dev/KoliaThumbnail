import { cn } from '../../lib/utils'

interface ProgressBarProps {
  /** Giá trị phần trăm (0–100) */
  value: number
  className?: string
  /** Màu sắc tuỳ chỉnh cho thanh progress */
  barClassName?: string
  /** Hiển thị % text bên phải */
  showLabel?: boolean
}

export function ProgressBar({ value, className, barClassName, showLabel = true }: ProgressBarProps) {
  const clamped = Math.min(100, Math.max(0, value))

  return (
    <div className={cn('flex items-center gap-3', className)}>
      <div className="h-2 flex-1 overflow-hidden rounded-full bg-slate-200 dark:bg-slate-700">
        <div
          className={cn(
            'h-full rounded-full bg-slate-900 dark:bg-slate-100 transition-all duration-500',
            barClassName,
          )}
          style={{ width: `${clamped}%` }}
        />
      </div>
      {showLabel && (
        <span className="text-xs font-medium text-slate-600 dark:text-slate-400 tabular-nums">
          {clamped}%
        </span>
      )}
    </div>
  )
}
