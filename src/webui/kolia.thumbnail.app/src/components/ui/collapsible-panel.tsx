import { type ReactNode, useState } from 'react'
import { ChevronDown, ChevronRight } from 'lucide-react'
import { cn } from '../../lib/utils'

interface CollapsiblePanelProps {
  title: string
  children: ReactNode
  defaultOpen?: boolean
  className?: string
}

export function CollapsiblePanel({ title, children, defaultOpen = true, className }: CollapsiblePanelProps) {
  const [isOpen, setIsOpen] = useState(defaultOpen)

  return (
    <div className={cn('rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900', className)}>
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className="flex w-full items-center gap-2 px-4 py-3 text-left text-sm font-semibold text-slate-700 dark:text-slate-200 hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors rounded-t-lg"
      >
        {isOpen ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
        {title}
      </button>

      {isOpen && (
        <div className="border-t border-slate-200 dark:border-slate-700 p-4">
          {children}
        </div>
      )}
    </div>
  )
}
