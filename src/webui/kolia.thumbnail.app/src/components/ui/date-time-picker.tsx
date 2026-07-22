import {
  useId,
  useState,
  useRef,
  useEffect,
  useCallback,
  useMemo,
  type InputHTMLAttributes,
} from 'react'
import { createPortal } from 'react-dom'
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
  const triggerRef = useRef<HTMLButtonElement>(null)
  const menuRef = useRef<HTMLDivElement>(null)
  const hiddenInputRef = useRef<HTMLInputElement>(null)
  const [menuStyle, setMenuStyle] = useState<{ top: number; left: number } | null>(null)

  const updateMenuPosition = useCallback(() => {
    if (!triggerRef.current) return
    const rect = triggerRef.current.getBoundingClientRect()
    
    // Default fallback or measured height of the date time picker dialog is ~330px
    const menuHeight = menuRef.current ? menuRef.current.getBoundingClientRect().height : 330
    const menuWidth = 264
    
    let top = rect.bottom + 6
    let left = rect.left

    const spaceBelow = window.innerHeight - rect.bottom
    const spaceAbove = rect.top

    if (spaceBelow < menuHeight && spaceAbove > spaceBelow) {
      top = rect.top - menuHeight - 6
    }

    // Align horizontally
    if (left + menuWidth > window.innerWidth) {
      left = Math.max(8, window.innerWidth - menuWidth - 8)
    }
    if (left < 8) {
      left = 8
    }

    setMenuStyle({ top, left })
  }, [])

  useEffect(() => {
    if (open) setViewDate(selectedDate ?? new Date())
  }, [open, selectedDate])

  useEffect(() => {
    if (!open) {
      setMenuStyle(null)
      return
    }

    updateMenuPosition()

    function handleClickOutside(e: MouseEvent) {
      const target = e.target as Node
      if (containerRef.current?.contains(target)) return
      if (menuRef.current?.contains(target)) return
      setOpen(false)
    }

    function handleEscape(e: KeyboardEvent) {
      if (e.key === 'Escape') setOpen(false)
    }

    function handleReposition() {
      updateMenuPosition()
    }

    document.addEventListener('mousedown', handleClickOutside)
    document.addEventListener('keydown', handleEscape)
    window.addEventListener('scroll', handleReposition, true)
    window.addEventListener('resize', handleReposition)

    const rafId = requestAnimationFrame(() => {
      updateMenuPosition()
    })

    return () => {
      document.removeEventListener('mousedown', handleClickOutside)
      document.removeEventListener('keydown', handleEscape)
      window.removeEventListener('scroll', handleReposition, true)
      window.removeEventListener('resize', handleReposition)
      cancelAnimationFrame(rafId)
    }
  }, [open, updateMenuPosition])

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
          className="mb-1 block text-[10px] font-semibold text-slate-400 dark:text-slate-500 uppercase tracking-widest"
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
        ref={triggerRef}
        type="button"
        disabled={disabled}
        onClick={() => setOpen((o) => !o)}
        aria-haspopup="dialog"
        aria-expanded={open}
        className={cn(
          'group flex h-8 w-full items-center gap-2 rounded-md border bg-white dark:bg-slate-900 px-2.5 text-left shadow-sm transition-all duration-150',
          open
            ? 'border-indigo-400 dark:border-indigo-600 ring-1 ring-indigo-200 dark:ring-indigo-800'
            : 'border-slate-200 dark:border-slate-700 hover:border-slate-300 hover:dark:border-slate-600',
          disabled && 'cursor-not-allowed opacity-60',
          className,
        )}
      >
        <Calendar className="h-3.5 w-3.5 flex-shrink-0 text-slate-400 dark:text-slate-500" />
        <span
          className={cn(
            'flex-1 truncate text-xs leading-none',
            selectedDate ? 'text-slate-700 dark:text-slate-200' : 'text-slate-300 dark:text-slate-600',
          )}
        >
          {selectedDate ? `${displayDate}  ${displayTime}` : (placeholder ?? 'dd/mm/yyyy hh:mm')}
        </span>
        {selectedDate && !disabled && (
          <X
            className="h-3 w-3 flex-shrink-0 text-slate-300 dark:text-slate-600 opacity-0 transition-opacity hover:text-slate-500 hover:dark:text-slate-400 group-hover:opacity-100"
            onClick={(e) => {
              e.stopPropagation()
              clear()
            }}
          />
        )}
      </button>

      {open && menuStyle && createPortal(
        <div
          ref={menuRef}
          role="dialog"
          className="fixed z-50 w-[264px] rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-3 shadow-lg shadow-slate-900/5"
          style={{
            top: menuStyle.top,
            left: menuStyle.left,
          }}
        >
          <div className="mb-2 flex items-center justify-between">
            <button
              type="button"
              onClick={() =>
                setViewDate((d) => new Date(d.getFullYear(), d.getMonth() - 1, 1))
              }
              className="flex h-6 w-6 items-center justify-center rounded-md text-slate-400 dark:text-slate-500 hover:bg-slate-100 hover:dark:bg-slate-800 hover:text-slate-600 hover:dark:text-slate-300"
            >
              <ChevronLeft className="h-3.5 w-3.5" />
            </button>
            <span className="text-xs font-semibold capitalize text-slate-700 dark:text-slate-200">
              {monthLabel}
            </span>
            <button
              type="button"
              onClick={() =>
                setViewDate((d) => new Date(d.getFullYear(), d.getMonth() + 1, 1))
              }
              className="flex h-6 w-6 items-center justify-center rounded-md text-slate-400 dark:text-slate-500 hover:bg-slate-100 hover:dark:bg-slate-800 hover:text-slate-600 hover:dark:text-slate-300"
            >
              <ChevronRight className="h-3.5 w-3.5" />
            </button>
          </div>

          <div className="grid grid-cols-7 gap-y-0.5">
            {WEEKDAYS_VI.map((wd) => (
              <div
                key={wd}
                className="flex h-6 items-center justify-center text-[10px] font-medium uppercase text-slate-400 dark:text-slate-500"
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
                        ? 'text-slate-700 dark:text-slate-200 hover:bg-slate-100 hover:dark:bg-slate-800'
                        : 'text-slate-300 dark:text-slate-600 hover:bg-slate-50 hover:dark:bg-slate-900',
                    !isSelected && isToday && 'ring-1 ring-inset ring-indigo-300 dark:ring-indigo-700 font-semibold text-indigo-600 dark:text-indigo-400',
                  )}
                >
                  {date.getDate()}
                </button>
              )
            })}
          </div>

          <div className="mt-3 flex items-center justify-center gap-3 border-t border-slate-100 dark:border-slate-800 pt-3">
            <Clock className="h-3.5 w-3.5 text-slate-400 dark:text-slate-500" />
            <TimeStepper
              value={selectedDate ? selectedDate.getHours() : 0}
              onIncrement={() => adjustTime('hours', 1)}
              onDecrement={() => adjustTime('hours', -1)}
            />
            <span className="text-sm font-semibold text-slate-400 dark:text-slate-500">:</span>
            <TimeStepper
              value={selectedDate ? selectedDate.getMinutes() : 0}
              onIncrement={() => adjustTime('minutes', 5)}
              onDecrement={() => adjustTime('minutes', -5)}
            />
          </div>

          <div className="mt-3 flex items-center justify-between border-t border-slate-100 dark:border-slate-800 pt-2.5">
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={goToday}
                className="text-[11px] font-medium text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 hover:dark:text-indigo-300 hover:underline"
              >
                Hôm nay
              </button>
              <button
                type="button"
                onClick={goNow}
                className="text-[11px] font-medium text-slate-400 dark:text-slate-500 hover:text-slate-600 hover:dark:text-slate-300 hover:underline"
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
        </div>,
        document.body
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
        className="flex h-3.5 w-6 items-center justify-center text-slate-300 dark:text-slate-600 hover:text-indigo-600 hover:dark:text-indigo-400"
      >
        <ChevronUp className="h-3 w-3" />
      </button>
      <span className="w-6 text-center text-sm font-semibold tabular-nums text-slate-700 dark:text-slate-200">
        {pad(value)}
      </span>
      <button
        type="button"
        onClick={onDecrement}
        className="flex h-3.5 w-6 items-center justify-center text-slate-300 dark:text-slate-600 hover:text-indigo-600 hover:dark:text-indigo-400"
      >
        <ChevronDown className="h-3 w-3" />
      </button>
    </div>
  )
}