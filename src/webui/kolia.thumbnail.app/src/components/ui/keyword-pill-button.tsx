import { cn } from '../../lib/utils'

interface KeywordPillButtonProps {
  keyword: string
  onClick: (keyword: string) => void
  className?: string
}

export function KeywordPillButton({ keyword, onClick, className }: KeywordPillButtonProps) {
  return (
    <button
      type="button"
      onClick={() => onClick(keyword)}
      className={cn(
        'inline-flex items-center rounded-full border border-slate-200 dark:border-slate-700 px-2.5 py-0.5 text-xs font-medium',
        'bg-white dark:bg-slate-900 text-slate-600 dark:text-slate-300',
        'hover:bg-slate-100 hover:dark:bg-slate-800 hover:border-slate-300 hover:dark:border-slate-600',
        'transition-colors cursor-pointer',
        className,
      )}
    >
      + {keyword}
    </button>
  )
}
