import { cn } from '../../lib/utils'
import { Checkbox } from './checkbox'
import type { CheckedState } from './checkbox'

interface CheckableThumbnailCardProps {
  imageUrl: string
  title: string
  selected: boolean
  onToggle: () => void
  /** Badge góc trên trái (VD: "YouTube", "Faceless") */
  badge?: string
  badgeColor?: string
  /** Footer text nhỏ (VD: views, thời gian) */
  meta?: string
  className?: string
  disabled?: boolean
}

export function CheckableThumbnailCard({
  imageUrl,
  title,
  selected,
  onToggle,
  badge,
  badgeColor,
  meta,
  className,
  disabled = false,
}: CheckableThumbnailCardProps) {
  return (
    <div
      className={cn(
        'group relative overflow-hidden rounded-lg border bg-white dark:bg-slate-900 transition-all',
        selected
          ? 'border-slate-900 dark:border-slate-100 ring-1 ring-slate-900 dark:ring-slate-100'
          : 'border-slate-200 dark:border-slate-700 hover:border-slate-300 dark:hover:border-slate-600',
        disabled && 'opacity-50',
        className,
      )}
    >
      {/* Checkbox góc trên phải */}
      <div className="absolute right-1.5 top-1.5 z-10">
        <Checkbox
          checked={selected as CheckedState}
          onCheckedChange={() => onToggle()}
          disabled={disabled}
        />
      </div>

      {/* Badge góc trên trái */}
      {badge && (
        <span
          className={cn(
            'absolute left-1.5 top-1.5 z-10 rounded px-1.5 py-0.5 text-[10px] font-medium',
            badgeColor ?? 'bg-black/60 text-white',
          )}
        >
          {badge}
        </span>
      )}

      {/* Image */}
      <div className="aspect-[16/9] overflow-hidden">
        <img
          src={imageUrl}
          alt={title}
          className="h-full w-full object-cover transition-transform group-hover:scale-105"
          loading="lazy"
        />
      </div>

      {/* Info */}
      <div className="p-2">
        <p className="text-xs font-medium text-slate-700 dark:text-slate-200 line-clamp-2">
          {title}
        </p>
        {meta && (
          <p className="mt-1 text-[10px] text-slate-500 dark:text-slate-400 truncate">
            {meta}
          </p>
        )}
      </div>
    </div>
  )
}
