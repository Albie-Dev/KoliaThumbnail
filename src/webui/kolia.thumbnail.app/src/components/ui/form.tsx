import { cn } from '../../lib/utils'

export function FormField({ error, className }: { error?: string; className?: string }) {
  if (!error) return null
  return <p className={cn('mt-1 text-xs text-rose-500', className)}>{error}</p>
}

export function FormGroup({ children, className }: { children: React.ReactNode; className?: string }) {
  return <div className={cn('space-y-2', className)}>{children}</div>
}

export function FormLabel({ htmlFor, children, className }: { htmlFor?: string; children: React.ReactNode; className?: string }) {
  return (
    <label htmlFor={htmlFor} className={cn('text-sm font-medium text-slate-700', className)}>
      {children}
    </label>
  )
}
