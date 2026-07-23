import { cn } from '../../lib/utils'

interface KeywordPillButtonProps {
  keyword: string
  onClick: (keyword: string) => void
  active?: boolean
  className?: string
}

export function KeywordPillButton({ keyword, onClick, active = false, className }: KeywordPillButtonProps) {
  return (
    <button
      type="button"
      onClick={() => onClick(keyword)}
      className={cn(
        'inline-flex items-center gap-1 rounded-full border px-2.5 py-0.5 text-xs font-medium transition-colors cursor-pointer',
        active
          ? 'bg-indigo-50 border-indigo-200 text-indigo-700 dark:bg-indigo-950/40 dark:border-indigo-900/60 dark:text-indigo-300'
          : 'bg-white border-slate-200 text-slate-600 dark:bg-slate-900 dark:border-slate-700 dark:text-slate-300 hover:bg-slate-100 hover:dark:bg-slate-800 hover:border-slate-300 hover:dark:border-slate-600',
        className,
      )}
    >
      <span>{active ? '✓' : '+'}</span>
      <span>{keyword}</span>
    </button>
  )
}
