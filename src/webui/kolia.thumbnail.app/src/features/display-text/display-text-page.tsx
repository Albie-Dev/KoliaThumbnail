import { useState, useMemo } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { Loader2, X } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '../../components/ui/button'
import { Checkbox } from '../../components/ui/checkbox'
import { useActiveProjectId, EmptyProjectState } from '../../lib/project-context'
import { useStepGuard } from '../../lib/use-step-guard'
import { getNews } from '../news/api'
import { getDisplayTexts, generateDisplayText, selectDisplayTextOption } from './api'
import { qk } from '../../lib/query-keys'

export function DisplayTextPage() {
  const [activeProjectId] = useActiveProjectId()
  useStepGuard('/pipeline/thumbnail/display-text')
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  // Selected news for generation
  const [selectedNewsIds, setSelectedNewsIds] = useState<Set<string>>(new Set())

  // Selected display text options
  const [selectedOptionIds, setSelectedOptionIds] = useState<Set<string>>(new Set())

  // Fetch news with isSelectedByTeam
  const { data: newsItems } = useQuery({
    queryKey: activeProjectId ? qk.news.list(activeProjectId) : ['news', 'empty'],
    queryFn: () => getNews(activeProjectId!),
    enabled: !!activeProjectId,
  })

  const selectedNews = (newsItems ?? []).filter((n) => n.isSelectedByTeam)

  // Fetch display texts
  const { data: displayTexts, isLoading } = useQuery({
    queryKey: activeProjectId ? qk.displayTexts(activeProjectId) : ['dt', 'empty'],
    queryFn: () => getDisplayTexts(activeProjectId!),
    enabled: !!activeProjectId,
  })

  // All options flattened
  const allOptions = useMemo(() => {
    return (displayTexts ?? []).flatMap((req) => req.options)
  }, [displayTexts])

  const hasSelectedOption = allOptions.some((o) => o.isSelected || selectedOptionIds.has(o.id))

  // Generate
  const { mutate: doGenerate, isPending: isGenerating } = useMutation({
    mutationFn: (ids: string[]) => generateDisplayText(activeProjectId!, ids),
    onSuccess: () => {
      toast.success('Đã tạo display text!')
      queryClient.invalidateQueries({ queryKey: qk.displayTexts(activeProjectId!) })
    },
  })

  // Select option
  const { mutate: doSelectOption } = useMutation({
    mutationFn: ({ id, selected }: { id: string; selected: boolean }) =>
      selectDisplayTextOption(activeProjectId!, id, selected),
  })

  if (!activeProjectId) return <EmptyProjectState />

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      <h1 className="text-xl font-bold text-slate-900 dark:text-slate-100">4.1 Tạo display text</h1>

      {/* Block 1: News selection */}
      <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4">
        <h2 className="mb-3 text-sm font-semibold text-slate-700 dark:text-slate-200">
          Tin đã chọn ({selectedNews.length})
        </h2>
        {selectedNews.length === 0 ? (
          <p className="text-sm text-slate-400">Chưa có tin nào được chọn. Vui lòng quay lại bước Tin tức.</p>
        ) : (
          <>
            <div className="mb-3 space-y-1">
              {selectedNews.map((item) => (
                <label key={item.id} className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={selectedNewsIds.has(item.id)}
                    onChange={() => {
                      setSelectedNewsIds((prev) => {
                        const next = new Set(prev)
                        if (next.has(item.id)) next.delete(item.id)
                        else next.add(item.id)
                        return next
                      })
                    }}
                    className="h-3.5 w-3.5"
                  />
                  <span className="text-xs text-slate-600 dark:text-slate-300 truncate">{item.title}</span>
                </label>
              ))}
            </div>
            <div className="flex gap-2">
              <Button variant="outline" size="sm" onClick={() => setSelectedNewsIds(new Set(selectedNews.map((n) => n.id)))}>
                Chọn tất cả
              </Button>
              <Button size="sm" onClick={() => doGenerate(Array.from(selectedNewsIds))} disabled={isGenerating || selectedNewsIds.size === 0}>
                {isGenerating ? 'Đang tạo…' : `Tạo cho tin đã chọn (${selectedNewsIds.size})`}
              </Button>
              <Button size="sm" variant="outline" onClick={() => doGenerate(selectedNews.map((n) => n.id))} disabled={isGenerating}>
                Tạo cho tất cả
              </Button>
            </div>
          </>
        )}
      </div>

      {/* Block 2: Results */}
      <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4">
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-sm font-semibold text-slate-700 dark:text-slate-200">
            Kết quả ({allOptions.length})
          </h2>
          <div className="flex gap-2">
            {allOptions.length > 0 && (
              <Button variant="outline" size="sm" onClick={() => {
                // Clear local selections only
                setSelectedOptionIds(new Set())
              }}>
                <X className="h-3.5 w-3.5" />
                Xoá kết quả
              </Button>
            )}
            <Button
              size="sm"
              disabled={!hasSelectedOption}
              onClick={() => navigate('/pipeline/thumbnail/generate?projectId=' + encodeURIComponent(activeProjectId!))}
            >
              Xác nhận Display Text →
            </Button>
          </div>
        </div>

        {isLoading && (
          <div className="flex justify-center p-6"><Loader2 className="h-5 w-5 animate-spin text-slate-400" /></div>
        )}

        {!isLoading && allOptions.length === 0 && (
          <div className="p-6 text-center text-sm text-slate-400">Chưa có kết quả nào</div>
        )}

        {allOptions.length > 0 && (
          <div className="space-y-2">
            {allOptions.map((opt) => (
              <div key={opt.id} className="flex items-center gap-3 rounded-lg border border-slate-100 dark:border-slate-800 p-3">
                <Checkbox
                  checked={opt.isSelected || selectedOptionIds.has(opt.id)}
                  onCheckedChange={() => {
                    const newSelected = !(opt.isSelected || selectedOptionIds.has(opt.id))
                    if (newSelected) setSelectedOptionIds((prev) => new Set(prev).add(opt.id))
                    else setSelectedOptionIds((prev) => {
                      const next = new Set(prev)
                      next.delete(opt.id)
                      return next
                    })
                    doSelectOption({ id: opt.id, selected: newSelected })
                  }}
                />
                <span className="inline-flex items-center rounded-full bg-indigo-50 dark:bg-indigo-950/40 px-3 py-1 text-sm font-medium text-indigo-700 dark:text-indigo-300">
                  {opt.content}
                </span>
                <span className="text-xs text-slate-400">Tin: {opt.sourceNewsItemId.slice(0, 8)}...</span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
