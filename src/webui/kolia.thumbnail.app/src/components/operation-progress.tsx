import { useEffect, useRef, useState, useCallback } from 'react'
import { X, Loader2, CheckCircle2, AlertCircle } from 'lucide-react'
import { Button } from './ui/button'

interface ProgressLog {
  message: string
  isError: boolean
  timestamp: string
}

interface Props {
  open: boolean
  onClose: () => void
  operationId: string | null
  title?: string
}

function resolveBaseUrl() {
  return import.meta.env.VITE_API_BASE_URL ?? 'https://holes-interactive-variations-given.trycloudflare.com'
}

export function OperationProgress({ open, onClose, operationId, title }: Props) {
  const [logs, setLogs] = useState<ProgressLog[]>([])
  const [status, setStatus] = useState<'running' | 'completed' | 'failed'>('running')
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const scrollRef = useRef<HTMLDivElement>(null)

  const baseUrl = resolveBaseUrl()

  useEffect(() => {
    if (!open || !operationId) return

    setLogs([])
    setStatus('running')
    setErrorMessage(null)

    const evtSource = new EventSource(`${baseUrl}/api/v1/progress/${operationId}/stream`)

    evtSource.onmessage = (event) => {
      if (!event.data || event.data === '') return
      try {
        const log = JSON.parse(event.data) as ProgressLog
        setLogs((prev) => [...prev, log])
      } catch {
        // ignore
      }
    }

    evtSource.addEventListener('done', (event) => {
      try {
        const data = JSON.parse(event.data)
        setStatus(data.status)
        setErrorMessage(data.errorMessage ?? null)
      } catch {
        setStatus('completed')
      }
      evtSource.close()
    })

    evtSource.onerror = () => {
      // Nếu connection bị đóng mà chưa nhận được done event, coi như completed
      setStatus((prev) => (prev === 'running' ? 'completed' : prev))
      evtSource.close()
    }

    return () => {
      evtSource.close()
    }
  }, [open, operationId, baseUrl])

  // Auto scroll to bottom when new logs arrive
  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight
    }
  }, [logs])

  const handleClose = useCallback(() => {
    onClose()
  }, [onClose])

  if (!open) return null

  const isRunning = status === 'running'

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="relative w-full max-w-lg rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-slate-200 dark:border-slate-700 px-5 py-3">
          <div className="flex items-center gap-2">
            {isRunning ? (
              <Loader2 className="h-4 w-4 animate-spin text-blue-500" />
            ) : status === 'completed' ? (
              <CheckCircle2 className="h-4 w-4 text-emerald-500" />
            ) : (
              <AlertCircle className="h-4 w-4 text-red-500" />
            )}
            <h3 className="text-sm font-semibold text-slate-900 dark:text-slate-100">
              {title || 'Đang xử lý...'}
            </h3>
          </div>
          {!isRunning && (
            <Button variant="ghost" size="icon" onClick={handleClose}>
              <X className="h-4 w-4" />
            </Button>
          )}
        </div>

        {/* Log area */}
        <div
          ref={scrollRef}
          className="h-64 overflow-y-auto p-4 space-y-1.5 font-mono text-[11px] leading-relaxed"
          style={{ scrollBehavior: 'smooth' }}
        >
          {logs.length === 0 && isRunning && (
            <p className="text-slate-400 dark:text-slate-500 italic">Đang kết nối...</p>
          )}
          {logs.map((log, i) => (
            <div
              key={i}
              className={`flex items-start gap-1.5 ${log.isError ? 'text-red-500 dark:text-red-400' : 'text-slate-600 dark:text-slate-400'}`}
            >
              <span className="shrink-0 text-[9px] text-slate-400 dark:text-slate-600 w-16 text-right">
                {new Date(log.timestamp).toLocaleTimeString('vi-VN')}
              </span>
              <span className={log.isError ? 'font-medium' : ''}>{log.message}</span>
            </div>
          ))}
          {!isRunning && (
            <div className="pt-2 text-center">
              <span className={`text-xs font-medium ${
                status === 'completed' ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-600 dark:text-red-400'
              }`}>
                {status === 'completed' ? '✅ Hoàn thành' : `❌ Thất bại: ${errorMessage || ''}`}
              </span>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
