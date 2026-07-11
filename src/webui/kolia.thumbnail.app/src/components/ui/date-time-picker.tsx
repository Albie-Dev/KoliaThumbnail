import {
  useId,
  useState,
  useRef,
  useEffect,
  useCallback,
  useMemo,
  type InputHTMLAttributes,
} from 'react'
import {
  Calendar,
  Clock,
  ChevronLeft,
  ChevronRight,
  ChevronUp,
  ChevronDown,
  X,
} from 'lucide-react'
import { cn } from '../../lib/utils'

interface DateTimePickerProps
  extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type' | 'onChange'> {
  label?: string
  value?: string
  onChange?: (e: { target: { value: string } }) => void
}

const WEEKDAYS_VI = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN']

function pad(n: number): string {
  return n.toString().padStart(2, '0')
}

function toLocalValue(d: Date): string {
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(
    d.getHours(),
  )}:${pad(d.getMinutes())}`
}

function parseValue(value: string | undefined): Date | null {
  if (!value) return null
  const [datePart, timePart] = value.split('T')
  if (!datePart) return null
  const [y, m, d] = datePart.split('-').map(Number)
  const [hh, mm] = (timePart ?? '00:00').split(':').map(Number)
  if (!y || !m || !d) return null
  const parsed = new Date(y, m - 1, d, hh || 0, mm || 0)
  return isNaN(parsed.getTime()) ? null : parsed
}

function isSameDay(a: Date, b: Date): boolean {
  return (
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate()
  )
}

function buildMonthGrid(viewDate: Date): { date: Date; inMonth: boolean }[] {
  const year = viewDate.getFullYear()
  const month = viewDate.getMonth()
  const firstOfMonth = new Date(year, month, 1)
  // Monday-first offset
  const leadingOffset = (firstOfMonth.getDay() + 6) % 7
  const start = new Date(year, month, 1 - leadingOffset)

  return Array.from({ length: 42 }, (_, i) => {
    const date = new Date(start)
    date.setDate(start.getDate() + i)
    return { date, inMonth: date.getMonth() === month }
  })
}

/**
 * Fully custom enterprise-grade datetime picker.
 * Renders a styled trigger and an in-flow popover calendar + time stepper —
 * no native <input type="datetime-local"> control surface is shown to the user.
 * A visually-hidden native input mirrors the value so the component stays a
 * drop-in replacement for form libraries that read/write via ref or onChange.
 */
export function DateTimePicker({
  label,
  className,
  id: externalId,
  value,
  onChange,
  disabled,
  required,
  name,
  placeholder,
  ...props
}: DateTimePickerProps) {
  const generatedId = useId()
  const inputId = externalId ?? generatedId

  const [open, setOpen] = useState(false)
  const selectedDate = useMemo(() => parseValue(value), [value])
  const [viewDate, setViewDate] = useState<Date>(selectedDate ?? new Date())

  const containerRef = useRef<HTMLDivElement>(null)
  const hiddenInputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    if (open) setViewDate(selectedDate ?? new Date())
  }, [open, selectedDate])

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    function handleEscape(e: KeyboardEvent) {
      if (e.key === 'Escape') setOpen(false)
    }
    if (open) {
      document.addEventListener('mousedown', handleClickOutside)
      document.addEventListener('keydown', handleEscape)
    }
    return () => {
      document.removeEventListener('mousedown', handleClickOutside)
      document.removeEventListener('keydown', handleEscape)
    }
  }, [open])

  const commit = useCallback(
    (next: Date) => {
      const nextValue = toLocalValue(next)
      const input = hiddenInputRef.current
      if (input) {
        const setter = Object.getOwnPropertyDescriptor(
          window.HTMLInputElement.prototype,
          'value',
        )?.set
        setter?.call(input, nextValue)
        input.dispatchEvent(new Event('input', { bubbles: true }))
      }
      onChange?.({ target: { value: nextValue } })
    },
    [onChange],
  )

  const clear = useCallback(() => {
    const input = hiddenInputRef.current
    if (input) {
      const setter = Object.getOwnPropertyDescriptor(
        window.HTMLInputElement.prototype,
        'value',
      )?.set
      setter?.call(input, '')
      input.dispatchEvent(new Event('input', { bubbles: true }))
    }
    onChange?.({ target: { value: '' } })
  }, [onChange])

  const handleSelectDay = useCallback(
    (day: Date) => {
      const base = selectedDate ?? new Date()
      const next = new Date(day)
      next.setHours(base.getHours(), base.getMinutes(), 0, 0)
      commit(next)
    },
    [selectedDate, commit],
  )

  const adjustTime = useCallback(
    (unit: 'hours' | 'minutes', delta: number) => {
      const base = selectedDate ?? new Date()
      const next = new Date(base)
      if (unit === 'hours') {
        next.setHours((base.getHours() + delta + 24) % 24)
      } else {
        next.setMinutes((base.getMinutes() + delta + 60) % 60)
      }
      commit(next)
    },
    [selectedDate, commit],
  )

  const goToday = useCallback(() => {
    const now = new Date()
    now.setSeconds(0, 0)
    commit(now)
    setOpen(false)
  }, [commit])

  const goNow = useCallback(() => {
    const now = new Date()
    now.setSeconds(0, 0)
    commit(now)
  }, [commit])

  const monthGrid = useMemo(() => buildMonthGrid(viewDate), [viewDate])
  const today = new Date()

  const displayDate = selectedDate
    ? selectedDate.toLocaleDateString('vi-VN', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
      })
    : ''
  const displayTime = selectedDate
    ? `${pad(selectedDate.getHours())}:${pad(selectedDate.getMinutes())}`
    : ''

  const monthLabel = viewDate.toLocaleDateString('vi-VN', {
    month: 'long',
    year: 'numeric',
  })

  return (
    <div className="relative" ref={containerRef}>
      {label && (
        <label
          htmlFor={inputId}
          className="mb-1 block text-[10px] font-semibold text-slate-400 uppercase tracking-widest"
        >
          {label}
        </label>
      )}

      <input
        ref={hiddenInputRef}
        id={inputId}
        type="datetime-local"
        defaultValue={value}
        name={name}
        required={required}
        disabled={disabled}
        className="sr-only"
        tabIndex={-1}
        aria-hidden="true"
        {...props}
      />

      <button
        type="button"
        disabled={disabled}
        onClick={() => setOpen((o) => !o)}
        aria-haspopup="dialog"
        aria-expanded={open}
        className={cn(
          'group flex h-8 w-full items-center gap-2 rounded-md border bg-white px-2.5 text-left shadow-sm transition-all duration-150',
          open
            ? 'border-indigo-400 ring-1 ring-indigo-200'
            : 'border-slate-200 hover:border-slate-300',
          disabled && 'cursor-not-allowed opacity-60',
          className,
        )}
      >
        <Calendar className="h-3.5 w-3.5 flex-shrink-0 text-slate-400" />
        <span
          className={cn(
            'flex-1 truncate text-xs leading-none',
            selectedDate ? 'text-slate-700' : 'text-slate-300',
          )}
        >
          {selectedDate ? `${displayDate}  ${displayTime}` : (placeholder ?? 'dd/mm/yyyy hh:mm')}
        </span>
        {selectedDate && !disabled && (
          <X
            className="h-3 w-3 flex-shrink-0 text-slate-300 opacity-0 transition-opacity hover:text-slate-500 group-hover:opacity-100"
            onClick={(e) => {
              e.stopPropagation()
              clear()
            }}
          />
        )}
      </button>

      {open && (
        <div
          role="dialog"
          className="absolute left-0 top-full z-50 mt-1.5 w-[264px] rounded-lg border border-slate-200 bg-white p-3 shadow-lg shadow-slate-900/5"
        >
          <div className="mb-2 flex items-center justify-between">
            <button
              type="button"
              onClick={() =>
                setViewDate((d) => new Date(d.getFullYear(), d.getMonth() - 1, 1))
              }
              className="flex h-6 w-6 items-center justify-center rounded-md text-slate-400 hover:bg-slate-100 hover:text-slate-600"
            >
              <ChevronLeft className="h-3.5 w-3.5" />
            </button>
            <span className="text-xs font-semibold capitalize text-slate-700">
              {monthLabel}
            </span>
            <button
              type="button"
              onClick={() =>
                setViewDate((d) => new Date(d.getFullYear(), d.getMonth() + 1, 1))
              }
              className="flex h-6 w-6 items-center justify-center rounded-md text-slate-400 hover:bg-slate-100 hover:text-slate-600"
            >
              <ChevronRight className="h-3.5 w-3.5" />
            </button>
          </div>

          <div className="grid grid-cols-7 gap-y-0.5">
            {WEEKDAYS_VI.map((wd) => (
              <div
                key={wd}
                className="flex h-6 items-center justify-center text-[10px] font-medium uppercase text-slate-400"
              >
                {wd}
              </div>
            ))}
            {monthGrid.map(({ date, inMonth }) => {
              const isSelected = selectedDate ? isSameDay(date, selectedDate) : false
              const isToday = isSameDay(date, today)
              return (
                <button
                  key={date.toISOString()}
                  type="button"
                  onClick={() => handleSelectDay(date)}
                  className={cn(
                    'flex h-7 w-7 items-center justify-center rounded-md text-xs transition-colors',
                    isSelected
                      ? 'bg-gradient-to-br from-indigo-500 to-violet-600 font-semibold text-white shadow-sm'
                      : inMonth
                        ? 'text-slate-700 hover:bg-slate-100'
                        : 'text-slate-300 hover:bg-slate-50',
                    !isSelected && isToday && 'ring-1 ring-inset ring-indigo-300 font-semibold text-indigo-600',
                  )}
                >
                  {date.getDate()}
                </button>
              )
            })}
          </div>

          <div className="mt-3 flex items-center justify-center gap-3 border-t border-slate-100 pt-3">
            <Clock className="h-3.5 w-3.5 text-slate-400" />
            <TimeStepper
              value={selectedDate ? selectedDate.getHours() : 0}
              onIncrement={() => adjustTime('hours', 1)}
              onDecrement={() => adjustTime('hours', -1)}
            />
            <span className="text-sm font-semibold text-slate-400">:</span>
            <TimeStepper
              value={selectedDate ? selectedDate.getMinutes() : 0}
              onIncrement={() => adjustTime('minutes', 5)}
              onDecrement={() => adjustTime('minutes', -5)}
            />
          </div>

          <div className="mt-3 flex items-center justify-between border-t border-slate-100 pt-2.5">
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={goToday}
                className="text-[11px] font-medium text-indigo-600 hover:text-indigo-700 hover:underline"
              >
                Hôm nay
              </button>
              <button
                type="button"
                onClick={goNow}
                className="text-[11px] font-medium text-slate-400 hover:text-slate-600 hover:underline"
              >
                Bây giờ
              </button>
            </div>
            <button
              type="button"
              onClick={() => setOpen(false)}
              className="rounded-md bg-indigo-600 px-2.5 py-1 text-[11px] font-medium text-white hover:bg-indigo-700"
            >
              Xong
            </button>
          </div>
        </div>
      )}
    </div>
  )
}

function TimeStepper({
  value,
  onIncrement,
  onDecrement,
}: {
  value: number
  onIncrement: () => void
  onDecrement: () => void
}) {
  return (
    <div className="flex flex-col items-center">
      <button
        type="button"
        onClick={onIncrement}
        className="flex h-3.5 w-6 items-center justify-center text-slate-300 hover:text-indigo-600"
      >
        <ChevronUp className="h-3 w-3" />
      </button>
      <span className="w-6 text-center text-sm font-semibold tabular-nums text-slate-700">
        {pad(value)}
      </span>
      <button
        type="button"
        onClick={onDecrement}
        className="flex h-3.5 w-6 items-center justify-center text-slate-300 hover:text-indigo-600"
      >
        <ChevronDown className="h-3 w-3" />
      </button>
    </div>
  )
}