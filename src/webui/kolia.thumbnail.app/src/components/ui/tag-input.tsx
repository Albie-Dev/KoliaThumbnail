import { useState, useRef, type KeyboardEvent, type ClipboardEvent } from 'react'
import { X } from 'lucide-react'
import { cn } from '../../lib/utils'

interface TagInputProps {
  tags: string[]
  onChange: (tags: string[]) => void
  placeholder?: string
  className?: string
  disabled?: boolean
  variant?: 'input' | 'textarea'
}

export function TagInput({
  tags,
  onChange,
  placeholder = 'Nhập từ khoá...',
  className,
  disabled = false,
  variant = 'input',
}: TagInputProps) {
  const [inputValue, setInputValue] = useState('')
  const inputRef = useRef<HTMLInputElement | HTMLTextAreaElement>(null)

  function addTags(value: string) {
    const newTags = value
      .split(/[,;\n\t]+/)
      .map((t) => t.trim())
      .filter((t) => t.length > 0 && !tags.includes(t))

    if (newTags.length > 0) {
      const uniqueNewTags = Array.from(new Set(newTags))
      onChange([...tags, ...uniqueNewTags])
    }
  }

  function removeTag(index: number) {
    onChange(tags.filter((_, i) => i !== index))
  }

  function handleKeyDown(e: KeyboardEvent<HTMLInputElement | HTMLTextAreaElement>) {
    if (e.key === 'Enter' || e.key === ';') {
      e.preventDefault()
      addTags(inputValue)
      setInputValue('')
    }
    if (e.key === 'Backspace' && !inputValue && tags.length > 0) {
      removeTag(tags.length - 1)
    }
  }

  function handleInputChange(val: string) {
    if (val.includes(',') || val.includes(';') || val.includes('\n')) {
      addTags(val)
      setInputValue('')
    } else {
      setInputValue(val)
    }
  }

  function handlePaste(e: ClipboardEvent<HTMLInputElement | HTMLTextAreaElement>) {
    e.preventDefault()
    const pastedText = e.clipboardData.getData('text')
    addTags(pastedText)
    setInputValue('')
  }

  const handleBlur = () => {
    if (inputValue.trim()) {
      addTags(inputValue)
      setInputValue('')
    }
  }

  const handleContainerClick = () => {
    inputRef.current?.focus()
  }

  return (
    <div
      onClick={handleContainerClick}
      className={cn(
        'flex flex-wrap gap-1.5 rounded-md border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 px-3 py-2 text-sm cursor-text',
        'focus-within:ring-2 focus-within:ring-slate-900/10 dark:focus-within:ring-slate-100/20',
        variant === 'textarea' ? 'min-h-[120px] items-start content-start' : 'min-h-[38px] items-center',
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
              onClick={(e) => {
                e.stopPropagation()
                removeTag(index)
              }}
              className="ml-0.5 rounded-full p-0.5 hover:bg-indigo-200 dark:hover:bg-indigo-900/60"
            >
              <X className="h-3 w-3" />
            </button>
          )}
        </span>
      ))}
      {variant === 'textarea' ? (
        <textarea
          ref={inputRef as React.RefObject<HTMLTextAreaElement>}
          value={inputValue}
          onChange={(e) => handleInputChange(e.target.value)}
          onKeyDown={handleKeyDown}
          onPaste={handlePaste}
          onBlur={handleBlur}
          placeholder={tags.length === 0 ? placeholder : ''}
          disabled={disabled}
          rows={1}
          className="min-w-[120px] flex-1 resize-none border-0 bg-transparent p-0 text-sm text-slate-900 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-0"
        />
      ) : (
        <input
          ref={inputRef as React.RefObject<HTMLInputElement>}
          type="text"
          value={inputValue}
          onChange={(e) => handleInputChange(e.target.value)}
          onKeyDown={handleKeyDown}
          onPaste={handlePaste}
          onBlur={handleBlur}
          placeholder={tags.length === 0 ? placeholder : ''}
          disabled={disabled}
          className="min-w-[80px] flex-1 border-0 bg-transparent p-0 text-sm text-slate-900 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-0"
        />
      )}
    </div>
  )
}
