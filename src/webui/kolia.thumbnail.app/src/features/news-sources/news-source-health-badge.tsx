interface Props {
  consecutiveFailureCount: number
  isTrusted: boolean
  lastFailedAt: string | null
}

export function NewsSourceHealthBadge({ consecutiveFailureCount, isTrusted }: Props) {
  if (!isTrusted) {
    return (
      <span className="inline-flex items-center gap-1 rounded-full bg-slate-100 dark:bg-slate-800 px-2.5 py-0.5 text-xs font-medium text-slate-500 dark:text-slate-400">
        Đã tắt
      </span>
    )
  }
  if (consecutiveFailureCount >= 3) {
    return (
      <span className="inline-flex items-center gap-1 rounded-full bg-red-50 dark:bg-red-950/40 px-2.5 py-0.5 text-xs font-medium text-red-600 dark:text-red-400">
        Lỗi liên tục ({consecutiveFailureCount})
      </span>
    )
  }
  if (consecutiveFailureCount >= 1) {
    return (
      <span className="inline-flex items-center gap-1 rounded-full bg-amber-50 dark:bg-amber-950/40 px-2.5 py-0.5 text-xs font-medium text-amber-700 dark:text-amber-400">
        Cảnh báo ({consecutiveFailureCount})
      </span>
    )
  }
  return (
    <span className="inline-flex items-center gap-1 rounded-full bg-emerald-50 dark:bg-emerald-950/40 px-2.5 py-0.5 text-xs font-medium text-emerald-700 dark:text-emerald-400">
      Hoạt động tốt
    </span>
  )
}
