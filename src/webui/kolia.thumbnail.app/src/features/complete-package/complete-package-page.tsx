import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { CheckCircle2 } from 'lucide-react'
import { Button } from '../../components/ui/button'
import { Badge } from '../../components/ui/badge'
import { useActiveProjectId, EmptyProjectState } from '../../lib/project-context'
import { useStepGuard } from '../../lib/use-step-guard'
import { getCompletePackages, confirmCompletePackage } from './api'
import { getVideoTitles } from '../video-title/api'
import { qk } from '../../lib/query-keys'
import { ApiError } from '../../lib/api/api-error'

export function CompletePackagePage() {
  const [activeProjectId] = useActiveProjectId()
  useStepGuard('/pipeline/complete-set')
  const queryClient = useQueryClient()

  const [selectedTitleIds, setSelectedTitleIds] = useState<Set<string>>(new Set())
  const selectedThumbnailId: string | null = null

  // Fetch existing packages
  const { data: packages } = useQuery({
    queryKey: activeProjectId ? qk.completePackages(activeProjectId) : ['cp', 'empty'],
    queryFn: () => getCompletePackages(activeProjectId!),
    enabled: !!activeProjectId,
  })

  // Fetch video titles for available options
  const { data: videoTitles } = useQuery({
    queryKey: activeProjectId ? qk.videoTitles(activeProjectId) : ['vt', 'empty'],
    queryFn: () => getVideoTitles(activeProjectId!),
    enabled: !!activeProjectId,
  })

  const allTitleOptions = (videoTitles ?? []).flatMap((r) => r.options.filter((o) => o.isSelected))
  const existingPackage = (packages ?? [])[0]

  // Confirm
  const { mutate: doConfirm, isPending: isConfirming } = useMutation({
    mutationFn: () =>
      confirmCompletePackage(activeProjectId!, {
        selectedThumbnailId: selectedThumbnailId!,
        selectedTitleOptionIds: Array.from(selectedTitleIds),
      }),
    onSuccess: () => {
      toast.success('Đã xác nhận bộ hoàn chỉnh!')
      queryClient.invalidateQueries({ queryKey: qk.completePackages(activeProjectId!) })
      queryClient.invalidateQueries({ queryKey: qk.projects.detail(activeProjectId!) })
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : 'Có lỗi xảy ra.'),
  })

  if (!activeProjectId) return <EmptyProjectState />

  // Already has a package
  if (existingPackage) {
    return (
      <div className="mx-auto max-w-4xl space-y-6">
        <div className="flex items-center gap-2">
          <h1 className="text-xl font-bold text-slate-900 dark:text-slate-100">Bộ hoàn chỉnh</h1>
          <Badge variant="success" dot>Đã xác nhận</Badge>
        </div>

        <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 overflow-hidden">
          <div className="aspect-video bg-slate-100 dark:bg-slate-800">
            <img
              src={existingPackage.thumbnailImageUrl}
              alt="Thumbnail"
              className="h-full w-full object-contain"
            />
          </div>
          <div className="p-6 space-y-4">
            <div>
              <h3 className="text-sm font-semibold text-slate-500 dark:text-slate-400 uppercase">Display Text</h3>
              <p className="mt-1 text-sm text-slate-700 dark:text-slate-200">{existingPackage.displayTextSnapshot}</p>
            </div>
            <div>
              <h3 className="text-sm font-semibold text-slate-500 dark:text-slate-400 uppercase">Video Titles</h3>
              <ul className="mt-1 space-y-1">
                {existingPackage.selectedTitles.map((t) => (
                  <li key={t.videoTitleOptionId} className="flex items-center gap-2 text-sm text-slate-600 dark:text-slate-300">
                    <CheckCircle2 className="h-3.5 w-3.5 text-emerald-500" />
                    {t.content}
                  </li>
                ))}
              </ul>
            </div>
            {existingPackage.confirmedAt && (
              <p className="text-xs text-slate-400">
                Xác nhận lúc: {new Date(existingPackage.confirmedAt).toLocaleString('vi-VN')}
              </p>
            )}
          </div>
        </div>
      </div>
    )
  }

  // Need to create package
  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <h1 className="text-xl font-bold text-slate-900 dark:text-slate-100">Bộ hoàn chỉnh</h1>

      <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-6 space-y-6">
        <div>
          <h2 className="text-sm font-semibold text-slate-700 dark:text-slate-200 mb-3">
            Chọn Video Title
          </h2>
          {allTitleOptions.length === 0 ? (
            <p className="text-sm text-slate-400">Chưa có video title nào được chọn.</p>
          ) : (
            <div className="space-y-2">
              {allTitleOptions.map((opt) => (
                <label key={opt.id} className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={selectedTitleIds.has(opt.id)}
                    onChange={() => {
                      setSelectedTitleIds((prev) => {
                        const next = new Set(prev)
                        if (next.has(opt.id)) next.delete(opt.id)
                        else next.add(opt.id)
                        return next
                      })
                    }}
                    className="h-3.5 w-3.5"
                  />
                  <span className="text-sm text-slate-600 dark:text-slate-300">{opt.content}</span>
                </label>
              ))}
            </div>
          )}
        </div>

        <Button
          onClick={() => doConfirm()}
          disabled={isConfirming || selectedTitleIds.size === 0}
          className="w-full"
        >
          {isConfirming ? 'Đang xác nhận…' : 'Xác nhận'}
        </Button>
      </div>
    </div>
  )
}
