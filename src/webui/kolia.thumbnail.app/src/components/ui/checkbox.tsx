import { forwardRef } from 'react'
import { Check, Minus } from 'lucide-react'
import { cn } from '../../lib/utils'

export type CheckedState = boolean | 'indeterminate'

interface CheckboxProps
    extends Omit<React.ButtonHTMLAttributes<HTMLButtonElement>, 'onChange' | 'checked'> {
    checked?: CheckedState
    onCheckedChange?: (checked: boolean) => void
    disabled?: boolean
}

export const Checkbox = forwardRef<HTMLButtonElement, CheckboxProps>(
    ({ checked = false, onCheckedChange, disabled, className, id, ...props }, ref) => {
        const isIndeterminate = checked === 'indeterminate'
        const isChecked = checked === true

        function handleClick() {
            if (disabled) return
            onCheckedChange?.(!isChecked)
        }

        function handleKeyDown(e: React.KeyboardEvent<HTMLButtonElement>) {
            if (e.key === ' ' || e.key === 'Enter') {
                e.preventDefault()
                handleClick()
            }
        }

        return (
            <button
                ref={ref}
                id={id}
                type="button"
                role="checkbox"
                aria-checked={isIndeterminate ? 'mixed' : isChecked}
                disabled={disabled}
                onClick={handleClick}
                onKeyDown={handleKeyDown}
                className={cn(
                    'peer flex h-4 w-4 shrink-0 items-center justify-center rounded border transition-colors duration-150',
                    'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-900/10 focus-visible:ring-offset-1',
                    disabled
                        ? 'cursor-not-allowed border-slate-200 bg-slate-50'
                        : isChecked || isIndeterminate
                            ? 'border-slate-900 bg-slate-900 hover:bg-slate-800'
                            : 'border-slate-300 bg-white hover:border-slate-400',
                    className,
                )}
                {...props}
            >
                {isIndeterminate ? (
                    <Minus className="h-3 w-3 text-white" strokeWidth={3} />
                ) : isChecked ? (
                    <Check className="h-3 w-3 text-white" strokeWidth={3} />
                ) : null}
            </button>
        )
    },
)

Checkbox.displayName = 'Checkbox'