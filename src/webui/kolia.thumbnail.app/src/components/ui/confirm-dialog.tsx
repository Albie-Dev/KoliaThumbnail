import { useEffect, useRef, type ReactNode } from 'react'
import { createPortal } from 'react-dom'
import { AlertTriangle, X } from 'lucide-react'
import { Button } from './button'
import { cn } from '../../lib/utils'

interface ConfirmDialogProps {
  open: boolean
  onClose: () => void
  onConfirm: () => void
  title: string
  message: string | ReactNode
  confirmLabel?: string
  cancelLabel?: string
  variant?: 'danger' | 'warning'
  loading?: boolean
}

export function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  message,
  confirmLabel = 'Xác nhận',
  cancelLabel = 'Hủy',
  variant = 'danger',
  loading = false,
}: ConfirmDialogProps) {
  const confirmRef = useRef<HTMLButtonElement>(null)

  // Focus confirm button when opened
  useEffect(() => {
    if (open) {
      // Small timeout to allow the dialog to render first
      setTimeout(() => confirmRef.current?.focus(), 50)
    }
  }, [open])

  // Close on Escape
  useEffect(() => {
    if (!open) return
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [open, onClose])

  if (!open) return null

  const iconColors = {
    danger: 'text-red-500 bg-red-50',
    warning: 'text-amber-500 bg-amber-50',
  }

  const buttonColors = {
    danger: 'bg-red-600 hover:bg-red-700 focus-visible:ring-red-400',
    warning: 'bg-amber-600 hover:bg-amber-700 focus-visible:ring-amber-400',
  }

  return createPortal(
    <div className="fixed inset-0 z-60 flex items-center justify-center">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-black/40 animate-fade-in"
        onClick={onClose}
      />

      {/* Dialog */}
      <div
        className={cn(
          'relative z-10 w-full max-w-sm rounded-xl border border-slate-200 bg-white shadow-2xl animate-fade-in',
        )}
        role="dialog"
        aria-modal="true"
      >
        {/* Close button */}
        <button
          type="button"
          onClick={onClose}
          className="absolute right-3 top-3 rounded-lg p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-600 transition-colors"
        >
          <X className="h-4 w-4" />
        </button>

        {/* Content */}
        <div className="p-6">
          {/* Icon */}
          <div
            className={cn(
              'mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full',
              iconColors[variant],
            )}
          >
            <AlertTriangle className="h-6 w-6" />
          </div>

          {/* Title */}
          <h3 className="mb-2 text-center text-lg font-semibold text-slate-900">
            {title}
          </h3>

          {/* Message */}
          <div className="mb-6 text-center text-sm text-slate-500">
            {message}
          </div>

          {/* Actions */}
          <div className="flex items-center gap-3">
            <Button
              variant="outline"
              onClick={onClose}
              className="flex-1"
              disabled={loading}
            >
              {cancelLabel}
            </Button>
            <Button
              ref={confirmRef}
              onClick={onConfirm}
              className={cn('flex-1 text-white', buttonColors[variant])}
              disabled={loading}
            >
              {loading ? 'Đang xử lý…' : confirmLabel}
            </Button>
          </div>
        </div>
      </div>
    </div>,
    document.body,
  )
}
