import { useState, useEffect } from 'react'
import { useMutation } from '@tanstack/react-query'
import { Loader2, ExternalLink } from 'lucide-react'
import { Button } from '../../components/ui/button'
import { DialogProvider, DialogContent, DialogHeader, DialogTitle } from '../../components/ui/dialog'
import { TagInput } from '../../components/ui/tag-input'
import { testFetchNewsSource } from './api'

interface Props {
  open: boolean
  onClose: () => void
  sourceId: string | null
  sourceName: string
}

export function NewsSourceTestFetchDialog({ open, onClose, sourceId, sourceName }: Props) {
  const [keywords, setKeywords] = useState<string[]>([])
  const { mutate, data, isPending, reset } = useMutation({
    mutationFn: () => testFetchNewsSource(sourceId!, keywords),
  })

  useEffect(() => {
    if (open) {
      reset()
      setKeywords([])
    }
  }, [open, reset])

  return (
    <DialogProvider open={open} setOpen={(v) => { if (!v) onClose() }}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>Test fetch: {sourceName}</DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          <TagInput
            tags={keywords}
            onChange={setKeywords}
            placeholder="Nhập keyword để test (bỏ trống = mặc định)"
          />
          <Button onClick={() => mutate()} disabled={isPending || !sourceId}>
            {isPending ? (
              <><Loader2 className="h-4 w-4 animate-spin" /> Đang test…</>
            ) : (
              'Chạy test fetch'
            )}
          </Button>

          {data && (
            <div className="rounded-lg border border-slate-200 dark:border-slate-700 p-3 space-y-2">
              <p className="text-sm text-slate-700 dark:text-slate-300">
                Kết quả: <strong>{data.success ? 'Thành công' : 'Thất bại'}</strong>
                {' — '}Tier dùng: {data.tierUsed} — {data.itemCount} tin
              </p>
              {data.errorMessage && (
                <p className="text-sm text-red-600 dark:text-red-400">{data.errorMessage}</p>
              )}
              {data.items && data.items.length > 0 && (
                <ul className="space-y-1 text-xs text-slate-600 dark:text-slate-300 max-h-60 overflow-y-auto">
                  {data.items.map((it, idx) => (
                    <li key={it.sourceUrl + idx} className="truncate">
                      <a
                        href={it.sourceUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="inline-flex items-center gap-1 text-blue-600 dark:text-blue-400 hover:underline"
                      >
                        {it.title}
                        <ExternalLink className="h-3 w-3 shrink-0" />
                      </a>
                      {it.publishedTime && (
                        <span className="ml-2 text-slate-400 dark:text-slate-500">
                          {new Date(it.publishedTime).toLocaleDateString('vi-VN')}
                        </span>
                      )}
                    </li>
                  ))}
                </ul>
              )}
            </div>
          )}
        </div>
      </DialogContent>
    </DialogProvider>
  )
}
