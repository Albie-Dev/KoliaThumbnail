import { useSearchParams } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { Button } from '../components/ui/button'
import { useSidebarContext } from './sidebar-context'

/**
 * Hook đọc/ghi projectId đang active bằng URL search params ("?projectId=...").
 * Dùng ở mọi trang /pipeline/* và /dashboard.
 */
export function useActiveProjectId(): [string | null, (id: string | null) => void] {
  const [searchParams, setSearchParams] = useSearchParams()
  const projectId = searchParams.get('projectId')

  const setProjectId = (id: string | null) => {
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev)
        if (id) {
          next.set('projectId', id)
        } else {
          next.delete('projectId')
        }
        return next
      },
      { replace: true },
    )
  }

  return [projectId, setProjectId]
}

/**
 * Component hiển thị khi chưa có project nào được chọn.
 * Render nội dung thay thế tại chỗ — KHÔNG tự động redirect.
 */
export function EmptyProjectState() {
  const { open } = useSidebarContext()

  return (
    <div className="flex h-full flex-col items-center justify-center gap-4 text-slate-400 dark:text-slate-500">
      <ArchiveIcon className="h-16 w-16 text-slate-300 dark:text-slate-600" />
      <p className="text-lg text-center max-w-md">
        Chưa chọn project — vào <strong>Kho lưu trữ</strong> để chọn hoặc tạo project mới
      </p>
      <div className="flex items-center gap-3">
        <Button onClick={() => open({ type: 'create-project' })}>
          <Plus className="h-4 w-4" />
          Tạo project mới
        </Button>
        <a
          href="/archive"
          className="inline-flex items-center gap-2 rounded-md border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 px-4 py-2 text-sm font-medium text-slate-700 dark:text-slate-200 hover:bg-slate-50 dark:hover:bg-slate-800"
        >
          <ArchiveIcon className="h-4 w-4" />
          Kho lưu trữ
        </a>
      </div>
    </div>
  )
}

function ArchiveIcon({ className }: { className?: string }) {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      className={className}
    >
      <rect width="20" height="5" x="2" y="3" rx="1" />
      <path d="M4 8v11a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8" />
      <path d="M10 12h4" />
    </svg>
  )
}
