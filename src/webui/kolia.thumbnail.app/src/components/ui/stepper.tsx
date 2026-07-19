import { cn } from '../../lib/utils'

export type StepStatus = 'completed' | 'needs-approval' | 'waiting' | 'in-progress' | 'failed'

export interface StepItem {
  number: number
  label: string
  status: StepStatus
  description?: string | null
  onClick?: () => void
}

const statusStyles: Record<StepStatus, { bg: string; text: string; border: string }> = {
  completed: {
    bg: 'bg-emerald-500 dark:bg-emerald-400',
    text: 'text-emerald-700 dark:text-emerald-300',
    border: 'border-emerald-200 dark:border-emerald-800',
  },
  'needs-approval': {
    bg: 'bg-amber-500 dark:bg-amber-400',
    text: 'text-amber-700 dark:text-amber-300',
    border: 'border-amber-300 dark:border-amber-700 ring-2 ring-amber-300 dark:ring-amber-700',
  },
  waiting: {
    bg: 'bg-slate-300 dark:bg-slate-600',
    text: 'text-slate-400 dark:text-slate-500',
    border: 'border-slate-200 dark:border-slate-700',
  },
  'in-progress': {
    bg: 'bg-blue-500 dark:bg-blue-400',
    text: 'text-blue-700 dark:text-blue-300',
    border: 'border-blue-200 dark:border-blue-800',
  },
  failed: {
    bg: 'bg-rose-500 dark:bg-rose-400',
    text: 'text-rose-700 dark:text-rose-300',
    border: 'border-rose-200 dark:border-rose-800',
  },
}

const statusLabels: Record<StepStatus, string> = {
  completed: 'Hoàn thành',
  'needs-approval': 'Cần duyệt',
  waiting: 'Chờ',
  'in-progress': 'Đang thực hiện',
  failed: 'Thất bại',
}

interface StepperProps {
  steps: StepItem[]
  className?: string
}

export function Stepper({ steps, className }: StepperProps) {
  return (
    <div className={cn('grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-3', className)}>
      {steps.map((step) => {
        const style = statusStyles[step.status]
        return (
          <button
            key={step.number}
            type="button"
            onClick={step.onClick}
            disabled={!step.onClick}
            className={cn(
              'flex flex-col items-center gap-2 rounded-xl border p-4 text-center transition-all',
              style.border,
              step.onClick ? 'cursor-pointer hover:shadow-md' : 'cursor-default',
              'bg-white dark:bg-slate-900',
            )}
          >
            {/* Số thứ tự */}
            <span
              className={cn(
                'flex h-8 w-8 items-center justify-center rounded-full text-sm font-bold text-white',
                style.bg,
              )}
            >
              {step.number}
            </span>

            {/* Trạng thái */}
            <span className={cn('text-xs font-semibold', style.text)}>
              {statusLabels[step.status]}
            </span>

            {/* Tên bước */}
            <span className="text-sm font-medium text-slate-700 dark:text-slate-200 line-clamp-2">
              {step.label}
            </span>

            {/* Mô tả */}
            {step.description && (
              <span className="text-xs text-slate-500 dark:text-slate-400 line-clamp-2">
                {step.description}
              </span>
            )}
          </button>
        )
      })}
    </div>
  )
}
