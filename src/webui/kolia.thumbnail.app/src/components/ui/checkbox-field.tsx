import { forwardRef } from 'react'
import { Checkbox, type CheckedState } from './checkbox'
import { cn } from '../../lib/utils'

interface CheckboxFieldProps {
    id?: string
    label: string
    description?: string
    checked: CheckedState
    onCheckedChange: (checked: boolean) => void
    disabled?: boolean
    className?: string
}

export const CheckboxField = forwardRef<HTMLButtonElement, CheckboxFieldProps>(
    ({ id, label, description, checked, onCheckedChange, disabled, className }, ref) => (
        <label
            htmlFor={id}
            className={cn(
                'flex items-start gap-2.5',
                disabled ? 'cursor-not-allowed opacity-60' : 'cursor-pointer',
                className,
            )}
        >
            <Checkbox
                ref={ref}
                id={id}
                checked={checked}
                onCheckedChange={onCheckedChange}
                disabled={disabled}
                className="mt-0.5"
            />
            <span className="flex flex-col gap-0.5">
                <span className="text-sm font-medium leading-none text-slate-900 dark:text-slate-100">{label}</span>
                {description && <span className="text-xs leading-snug text-slate-500 dark:text-slate-400">{description}</span>}
            </span>
        </label>
    ),
)

CheckboxField.displayName = 'CheckboxField'