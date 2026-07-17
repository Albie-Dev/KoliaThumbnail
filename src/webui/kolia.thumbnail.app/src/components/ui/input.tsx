import * as React from 'react'
import { cn } from '../../lib/utils'

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> { }

const Input = React.forwardRef<HTMLInputElement, InputProps>(({ className, type, ...props }, ref) => {
  return (
    <input
      type={type}
      className={cn(
        'flex h-9 w-full rounded-md border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 px-3 py-1 text-sm text-slate-900 dark:text-slate-100 shadow-sm outline-none transition',
        'placeholder:text-[13px] placeholder:font-normal placeholder:text-slate-400 placeholder:dark:text-slate-500',
        'focus:border-slate-400 focus:dark:border-slate-500 focus:ring-2 focus:ring-slate-200 focus:dark:ring-slate-700',
        'disabled:cursor-not-allowed disabled:bg-slate-50 disabled:dark:bg-slate-900 disabled:text-slate-400 disabled:dark:text-slate-500',
        className,
      )}
      ref={ref}
      {...props}
    />
  )
})
Input.displayName = 'Input'

export { Input }