import { useState } from 'react'
import { ChevronDown } from 'lucide-react'
import { cn } from '../../lib/utils'

interface FormSectionProps {
  title: string
  description?: string
  children: React.ReactNode
  collapsible?: boolean
  defaultOpen?: boolean
}

export function FormSection({
  title,
  description,
  children,
  collapsible = false,
  defaultOpen = true,
}: FormSectionProps) {
  const [isOpen, setIsOpen] = useState(defaultOpen)

  return (
    <div className="rounded-xl border border-slate-200/70 bg-white shadow-sm shadow-slate-200/50">
      <button
        type="button"
        onClick={() => collapsible && setIsOpen((v) => !v)}
        className={cn(
          'flex w-full items-start justify-between gap-3 p-5',
          collapsible ? 'cursor-pointer' : 'cursor-default',
          isOpen && 'pb-4',
        )}
      >
        <div className="text-left">
          <span className="text-[11px] font-semibold uppercase tracking-wider text-indigo-500">
            {title}
          </span>
          {description && <p className="mt-1 text-xs text-slate-400">{description}</p>}
        </div>

        {collapsible && (
          <ChevronDown
            className={cn(
              'mt-0.5 h-4 w-4 shrink-0 text-slate-400 transition-transform duration-200',
              isOpen && 'rotate-180',
            )}
          />
        )}
      </button>

      <div
        className={cn(
          'grid transition-all duration-200 ease-out',
          isOpen ? 'grid-rows-[1fr] opacity-100' : 'grid-rows-[0fr] opacity-0',
        )}
      >
        <div className="overflow-hidden">
          <div className="space-y-4 px-5 pb-5">{children}</div>
        </div>
      </div>
    </div>
  )
}