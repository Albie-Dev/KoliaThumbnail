import { cn } from '../../lib/utils'

export function FormField({ error, className }: { error?: string; className?: string }) {
  if (!error) return null
  return <p className={cn('mt-1 text-xs text-rose-500 dark:text-rose-400', className)}>{error}</p>
}

export function FormGroup({ children, className }: { children: React.ReactNode; className?: string }) {
  return <div className={cn('space-y-2', className)}>{children}</div>
}

export function FormLabel({ required, children, className, ...props }: React.LabelHTMLAttributes<HTMLLabelElement> & { required?: boolean }) {
  return (
    <label className={cn('text-sm font-medium text-slate-700 dark:text-slate-200', className)} {...props}>
      {children}
      {required && <span className="ml-0.5 text-rose-500 dark:text-rose-400">*</span>}
    </label>
  )
}
