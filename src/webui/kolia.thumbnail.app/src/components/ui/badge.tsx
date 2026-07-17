import { forwardRef, type HTMLAttributes } from 'react'
import { cn } from '../../lib/utils'

export type BadgeVariant = 'default' | 'secondary' | 'success' | 'warning' | 'destructive' | 'outline'

interface BadgeProps extends HTMLAttributes<HTMLSpanElement> {
  variant?: BadgeVariant
  /** Chấm tròn nhỏ trước label, dùng cho trạng thái (online/offline, enabled/disabled) */
  dot?: boolean
}

const variantClasses: Record<BadgeVariant, string> = {
  default: 'bg-slate-900 text-white dark:bg-slate-100 dark:text-slate-900',
  secondary: 'bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300',
  success: 'bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-300',
  warning: 'bg-amber-50 dark:bg-amber-950/40 text-amber-700 dark:text-amber-300',
  destructive: 'bg-rose-50 dark:bg-rose-950/40 text-rose-700 dark:text-rose-300',
  outline: 'border border-slate-200 dark:border-slate-700 text-slate-600 dark:text-slate-300 bg-transparent',
}

const dotClasses: Record<BadgeVariant, string> = {
  default: 'bg-white dark:bg-slate-900',
  secondary: 'bg-slate-400 dark:bg-slate-500',
  success: 'bg-emerald-500',
  warning: 'bg-amber-500',
  destructive: 'bg-rose-500',
  outline: 'bg-slate-400 dark:bg-slate-500',
}

export const Badge = forwardRef<HTMLSpanElement, BadgeProps>(
  ({ variant = 'default', dot = false, className, children, ...props }, ref) => (
    <span
      ref={ref}
      className={cn(
        'inline-flex items-center gap-1.5 rounded-md px-2 py-0.5 text-xs font-medium leading-none transition-colors',
        variantClasses[variant],
        className,
      )}
      {...props}
    >
      {dot && <span className={cn('h-1.5 w-1.5 shrink-0 rounded-full', dotClasses[variant])} />}
      {children}
    </span>
  ),
)

Badge.displayName = 'Badge'