import { cn } from '../../lib/utils'

interface ScoreBadgeProps {
  /** Giá trị điểm (0–100 hoặc bất kỳ) */
  score: number
  className?: string
  /** Tuỳ chỉnh label hiển thị, mặc định là số */
  label?: string
}

/**
 * Map điểm → màu badge:
 * - 0–39: đỏ (thấp)
 * - 40–69: vàng (trung bình)
 * - 70–100: xanh lá (cao)
 */
function getScoreColor(score: number): string {
  if (score >= 70) return 'bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-300'
  if (score >= 40) return 'bg-amber-50 dark:bg-amber-950/40 text-amber-700 dark:text-amber-300'
  return 'bg-rose-50 dark:bg-rose-950/40 text-rose-700 dark:text-rose-300'
}

export function ScoreBadge({ score, className, label }: ScoreBadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-md px-2 py-0.5 text-xs font-medium',
        getScoreColor(score),
        className,
      )}
    >
      {label ?? score}
    </span>
  )
}
