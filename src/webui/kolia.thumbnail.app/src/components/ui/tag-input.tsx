import { useState, type KeyboardEvent } from 'react'
import { X } from 'lucide-react'
import { cn } from '../../lib/utils'

interface TagInputProps {
  tags: string[]
  onChange: (tags: string[]) => void
  placeholder?: string
  className?: string
  disabled?: boolean
}

export function TagInput({ tags, onChange, placeholder = 'Nhập từ khoá...', className, disabled = false }: TagInputProps) {
  const [inputValue, setInputValue] = useState('')

  function addTag(value: string) {
    const trimmed = value.trim()
    if (!trimmed) return
    if (tags.includes(trimmed)) return
    onChange([...tags, trimmed])
  }

  function removeTag(index: number) {
    onChange(tags.filter((_, i) => i !== index))
  }

  function handleKeyDown(e: KeyboardEvent<HTMLInputElement>) {
    if (e.key === 'Enter' || e.key === ';') {
      e.preventDefault()
      addTag(inputValue)
      setInputValue('')
    }
    if (e.key === 'Backspace' && !inputValue && tags.length > 0) {
      removeTag(tags.length - 1)
    }
  }

  return (
    <div
      className={cn(
        'flex min-h-[38px] flex-wrap items-center gap-1.5 rounded-md border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 px-2 py-1.5 text-sm',
        'focus-within:ring-2 focus-within:ring-slate-900/10 dark:focus-within:ring-slate-100/20',
        disabled && 'cursor-not-allowed opacity-50',
        className,
      )}
    >
      {tags.map((tag, index) => (
        <span
          key={`${tag}-${index}`}
          className="inline-flex items-center gap-1 rounded-md bg-indigo-50 dark:bg-indigo-950/40 px-2 py-0.5 text-xs font-medium text-indigo-700 dark:text-indigo-300"
        >
          {tag}
          {!disabled && (
            <button
              type="button"
              onClick={() => removeTag(index)}
              className="ml-0.5 rounded-full p-0.5 hover:bg-indigo-200 dark:hover:bg-indigo-900/60"
            >
              <X className="h-3 w-3" />
            </button>
          )}
        </span>
      ))}
      <input
        type="text"
        value={inputValue}
        onChange={(e) => setInputValue(e.target.value)}
        onKeyDown={handleKeyDown}
        onBlur={() => {
          if (inputValue.trim()) {
            addTag(inputValue)
            setInputValue('')
          }
        }}
        placeholder={tags.length === 0 ? placeholder : ''}
        disabled={disabled}
        className="min-w-[80px] flex-1 border-0 bg-transparent p-0 text-sm text-slate-900 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-0"
      />
    </div>
  )
}
