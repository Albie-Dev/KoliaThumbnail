import { forwardRef, useMemo, useState, type TextareaHTMLAttributes } from 'react'
import { Type, Braces, Code2, Eye, EyeOff, AlertCircle } from 'lucide-react'
import { cn } from '../../lib/utils'

type ContentType = 'text' | 'json' | 'code'

interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  enableViewModes?: boolean
  defaultContentType?: ContentType
  codeLanguage?: string
}

const typeConfig: Record<ContentType, { icon: typeof Type; label: string }> = {
  text: { icon: Type, label: 'Text' },
  json: { icon: Braces, label: 'JSON' },
  code: { icon: Code2, label: 'Code' },
}

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(
  (
    {
      className,
      enableViewModes = false,
      defaultContentType = 'text',
      codeLanguage,
      value,
      defaultValue,
      onChange,
      ...props
    },
    ref,
  ) => {
    const [contentType, setContentType] = useState<ContentType>(defaultContentType)
    const [previewMode, setPreviewMode] = useState(false)
    const [internalValue, setInternalValue] = useState((value ?? defaultValue ?? '') as string)

    const isControlled = value !== undefined
    const currentValue = isControlled ? (value as string) : internalValue

    const handleChange: React.ChangeEventHandler<HTMLTextAreaElement> = (e) => {
      if (!isControlled) setInternalValue(e.target.value)
      onChange?.(e)
    }

    const jsonResult = useMemo(() => {
      if (contentType !== 'json' || !currentValue.trim()) return { valid: true as const, pretty: '' }
      try {
        return { valid: true as const, pretty: JSON.stringify(JSON.parse(currentValue), null, 2) }
      } catch (err) {
        return { valid: false as const, error: err instanceof Error ? err.message : 'JSON không hợp lệ' }
      }
    }, [currentValue, contentType])

    return (
      <div className="w-full">
        <div className="group relative w-full">
          {enableViewModes && (
            <div
              className={cn(
                'absolute right-1.5 top-1.5 z-10 flex items-center gap-0.5 rounded-md border border-slate-200 bg-white/90 p-0.5 opacity-40 shadow-sm backdrop-blur-sm transition-opacity',
                'group-hover:opacity-100 group-focus-within:opacity-100',
                previewMode && 'opacity-100',
              )}
            >
              {(Object.keys(typeConfig) as ContentType[]).map((t) => {
                const { icon: Icon, label } = typeConfig[t]
                return (
                  <button
                    key={t}
                    type="button"
                    title={label}
                    aria-label={label}
                    onClick={() => setContentType(t)}
                    className={cn(
                      'rounded p-1 transition-colors',
                      contentType === t
                        ? 'bg-slate-100 text-slate-900'
                        : 'text-slate-400 hover:text-slate-600',
                    )}
                  >
                    <Icon className="h-3 w-3" />
                  </button>
                )
              })}

              <span className="mx-0.5 h-3 w-px bg-slate-200" />

              <button
                type="button"
                title={previewMode ? 'Chỉnh sửa' : 'Xem preview'}
                onClick={() => setPreviewMode((p) => !p)}
                disabled={contentType === 'json' && !jsonResult.valid}
                className={cn(
                  'rounded p-1 text-slate-400 transition-colors hover:text-slate-600',
                  previewMode && 'bg-slate-100 text-slate-700',
                  contentType === 'json' && !jsonResult.valid && 'cursor-not-allowed opacity-40',
                )}
              >
                {previewMode ? <EyeOff className="h-3 w-3" /> : <Eye className="h-3 w-3" />}
              </button>
            </div>
          )}

          {previewMode ? (
            <div
              className={cn(
                'w-full overflow-auto rounded-md border px-3 py-2 pr-16 text-[13px] leading-5',
                contentType === 'text'
                  ? 'border-slate-300 bg-white text-slate-900'
                  : 'border-slate-700 bg-slate-900 text-slate-100 font-mono',
              )}
              style={{ minHeight: '80px' }}
            >
              {contentType === 'json' ? (
                <pre className="whitespace-pre-wrap">{jsonResult.pretty || '{}'}</pre>
              ) : contentType === 'code' ? (
                <pre className="whitespace-pre-wrap">{currentValue}</pre>
              ) : (
                <p className="whitespace-pre-wrap">
                  {currentValue || <span className="text-slate-400">Không có nội dung</span>}
                </p>
              )}
            </div>
          ) : (
            <textarea
              ref={ref}
              value={isControlled ? value : internalValue}
              onChange={handleChange}
              spellCheck={contentType === 'text' ? props.spellCheck : false}
              className={cn(
                'flex w-full rounded-md border bg-white px-3 py-2 pr-16 text-sm text-slate-900 shadow-sm transition-colors',
                (contentType === 'json' || contentType === 'code') && 'font-mono text-[13px] leading-5',
                contentType === 'json' && !jsonResult.valid ? 'border-red-300' : 'border-slate-300',
                'placeholder:text-[13px] placeholder:font-normal placeholder:text-slate-400',
                'focus:outline-none focus:border-slate-400 focus:ring-2 focus:ring-slate-100',
                'disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-400',
                className,
              )}
              {...props}
            />
          )}
        </div>

        {!previewMode && contentType === 'json' && !jsonResult.valid && (
          <p className="mt-1 flex items-center gap-1 text-xs text-red-500">
            <AlertCircle className="h-3 w-3" /> {jsonResult.error}
          </p>
        )}
      </div>
    )
  },
)

Textarea.displayName = 'Textarea'