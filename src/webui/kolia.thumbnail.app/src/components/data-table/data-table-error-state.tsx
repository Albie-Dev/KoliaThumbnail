export function DataTableErrorState({ title, message, onRetry }: { title: string; message: string; onRetry?: () => void }) {
  return (
    <div className="rounded-2xl border border-rose-200 dark:border-rose-800 bg-rose-50 dark:bg-rose-950/40 p-6 shadow-sm">
      <h3 className="text-lg font-semibold text-rose-700 dark:text-rose-300">{title}</h3>
      <p className="mt-2 text-sm text-rose-600 dark:text-rose-400">{message}</p>
      {onRetry ? (
        <button type="button" onClick={onRetry} className="mt-4 rounded-lg border border-rose-300 dark:border-rose-700 bg-white dark:bg-slate-900 px-4 py-2 text-sm font-medium text-rose-700 dark:text-rose-300 hover:bg-rose-100 hover:dark:bg-rose-900/40">
          Thử lại
        </button>
      ) : null}
    </div>
  )
}
