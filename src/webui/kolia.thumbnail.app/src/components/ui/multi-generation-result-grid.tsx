import { cn } from '../../lib/utils'
import { Checkbox } from './checkbox'
import type { CheckedState } from './checkbox'

export interface GenerationOption {
  id: string
  imageUrl: string
  label: string
  selected: boolean
  onToggle: () => void
}

export interface GenerationSet {
  id: string
  label: string
  options: GenerationOption[]
}

interface MultiGenerationResultGridProps {
  sets: GenerationSet[]
  className?: string
}

export function MultiGenerationResultGrid({ sets, className }: MultiGenerationResultGridProps) {
  if (sets.length === 0) {
    return (
      <div className="flex items-center justify-center rounded-lg border border-dashed border-slate-300 dark:border-slate-600 p-8 text-sm text-slate-400 dark:text-slate-500">
        Chưa có kết quả nào
      </div>
    )
  }

  return (
    <div className={cn('space-y-6', className)}>
      {sets.map((set) => (
        <div key={set.id}>
          {/* Set label */}
          <h4 className="mb-2 text-sm font-semibold text-slate-700 dark:text-slate-200">
            {set.label}
          </h4>

          {/* Options grid */}
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5">
            {set.options.map((option) => (
              <div
                key={option.id}
                className={cn(
                  'group relative overflow-hidden rounded-lg border bg-white dark:bg-slate-900 transition-all',
                  option.selected
                    ? 'border-slate-900 dark:border-slate-100 ring-1 ring-slate-900 dark:ring-slate-100'
                    : 'border-slate-200 dark:border-slate-700 hover:border-slate-300 dark:hover:border-slate-600',
                )}
              >
                <div className="absolute right-1 top-1 z-10">
                  <Checkbox
                    checked={option.selected as CheckedState}
                    onCheckedChange={() => option.onToggle()}
                  />
                </div>

                <div className="aspect-[16/9] overflow-hidden">
                  <img
                    src={option.imageUrl}
                    alt={option.label}
                    className="h-full w-full object-cover transition-transform group-hover:scale-105"
                    loading="lazy"
                  />
                </div>

                <div className="p-1.5">
                  <p className="text-[11px] font-medium text-slate-600 dark:text-slate-300 text-center truncate">
                    {option.label}
                  </p>
                </div>
              </div>
            ))}
          </div>
        </div>
      ))}
    </div>
  )
}
