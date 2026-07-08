import * as React from 'react'
import { X } from 'lucide-react'
import { cn } from '../../lib/utils'

const Dialog = React.createContext<{
  open: boolean
  setOpen: (open: boolean) => void
}>({
  open: false,
  setOpen: () => {},
})

export function useDialog() {
  return React.useContext(Dialog)
}

export function DialogProvider({ children, open, setOpen }: { children: React.ReactNode; open: boolean; setOpen: (open: boolean) => void }) {
  return <Dialog.Provider value={{ open, setOpen }}>{children}</Dialog.Provider>
}

export function DialogTrigger({ children, asChild }: { children: React.ReactNode; asChild?: boolean }) {
  const { setOpen } = useDialog()
  return (
    <button
      type="button"
      onClick={() => setOpen(true)}
      className={asChild ? '' : 'inline-flex items-center justify-center'}
    >
      {children}
    </button>
  )
}

export function DialogContent({ children, className }: { children: React.ReactNode; className?: string }) {
  const { open, setOpen } = useDialog()

  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className={cn('relative w-full max-w-md rounded-lg border border-slate-200 bg-white shadow-lg', className)}>
        <button
          type="button"
          onClick={() => setOpen(false)}
          className="absolute right-4 top-4 rounded-md hover:bg-slate-100"
        >
          <X className="h-4 w-4 text-slate-500" />
        </button>
        <div className="p-6">{children}</div>
      </div>
    </div>
  )
}

export function DialogHeader({ children, className }: { children: React.ReactNode; className?: string }) {
  return <div className={cn('mb-4 pr-8', className)}>{children}</div>
}

export function DialogTitle({ children, className }: { children: React.ReactNode; className?: string }) {
  return <h2 className={cn('text-lg font-semibold text-slate-900', className)}>{children}</h2>
}

export function DialogDescription({ children, className }: { children: React.ReactNode; className?: string }) {
  return <p className={cn('mt-1 text-sm text-slate-500', className)}>{children}</p>
}
