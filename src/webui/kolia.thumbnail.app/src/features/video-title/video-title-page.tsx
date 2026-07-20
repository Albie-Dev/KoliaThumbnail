import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { Loader2, Send } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '../../components/ui/button'
import { Input } from '../../components/ui/input'
import { Textarea } from '../../components/ui/textarea'
import { Badge } from '../../components/ui/badge'
import { Checkbox } from '../../components/ui/checkbox'
import { TagInput } from '../../components/ui/tag-input'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { useActiveProjectId, EmptyProjectState } from '../../lib/project-context'
import { useStepGuard } from '../../lib/use-step-guard'
import { getNews } from '../news/api'
import { getVideoTitles, generateVideoTitle, regenerateVideoTitle, regenerateWithFeedback, selectVideoTitleOption } from './api'
import { qk } from '../../lib/query-keys'
import { CTitleStyle, TITLE_STYLE_OPTIONS } from '../../types/enums/pipeline.enums'

export function VideoTitlePage() {
  const [activeProjectId] = useActiveProjectId()
  useStepGuard('/pipeline/video-title')
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  // Form state
  const [style, setStyle] = useState<number>(CTitleStyle.NeutralClear)
  const [keywords, setKeywords] = useState<string[]>([])
  const [requestedCount, setRequestedCount] = useState(5)
  const [feedbackText, setFeedbackText] = useState('')

  // Fetch data
  const { data: newsItems } = useQuery({
    queryKey: activeProjectId ? qk.news.list(activeProjectId) : ['news', 'empty'],
    queryFn: () => getNews(activeProjectId!),
    enabled: !!activeProjectId,
  })

  const { data: videoTitles, isLoading } = useQuery({
    queryKey: activeProjectId ? qk.videoTitles(activeProjectId) : ['vt', 'empty'],
    queryFn: () => getVideoTitles(activeProjectId!),
    enabled: !!activeProjectId,
  })

  const selectedNews = (newsItems ?? []).filter((n) => n.isSelectedByTeam)
  const allOptions = (videoTitles ?? []).flatMap((r) => r.options)
  const hasSelected = allOptions.some((o) => o.isSelected)

  // Generate
  const { mutate: doGenerate, isPending: isGenerating } = useMutation({
    mutationFn: () =>
      generateVideoTitle(activeProjectId!, {
        selectedThumbnailIds: [],
        selectedNewsItemIds: selectedNews.map((n) => n.id),
        style: style as CTitleStyle,
        keywordsRaw: keywords.join('; '),
        requestedCount,
      }),
    onSuccess: () => {
      toast.success('Đã tạo video title!')
      queryClient.invalidateQueries({ queryKey: qk.videoTitles(activeProjectId!) })
    },
  })

  // Regenerate
  const { mutate: doRegenerate, isPending: isRegenerating } = useMutation({
    mutationFn: (requestId: string) => regenerateVideoTitle(activeProjectId!, requestId),
    onSuccess: () => {
      toast.success('Đã tạo thêm phương án!')
      queryClient.invalidateQueries({ queryKey: qk.videoTitles(activeProjectId!) })
    },
  })

  // Feedback
  const { mutate: doFeedback, isPending: isFeedbacking } = useMutation({
    mutationFn: ({ requestId, text }: { requestId: string; text: string }) =>
      regenerateWithFeedback(activeProjectId!, requestId, text),
    onSuccess: () => {
      toast.success('Đã gửi feedback và tạo lại!')
      setFeedbackText('')
      queryClient.invalidateQueries({ queryKey: qk.videoTitles(activeProjectId!) })
    },
  })

  // Select option
  const { mutate: doSelectOption } = useMutation({
    mutationFn: ({ id, selected }: { id: string; selected: boolean }) =>
      selectVideoTitleOption(activeProjectId!, id, selected),
  })

  if (!activeProjectId) return <EmptyProjectState />

  const latestRequest = (videoTitles ?? []).length > 0 ? videoTitles![videoTitles!.length - 1] : null

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      <h1 className="text-xl font-bold text-slate-900 dark:text-slate-100">5. Tạo video title</h1>

      {/* Block 1: Settings */}
      <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4">
        <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">Phong cách</label>
            <SelectDropdown<{ id: number; label: string }>
              items={TITLE_STYLE_OPTIONS}
              getOptionId={(o) => String(o.id)}
              getOptionLabel={(o) => o.label}
              value={TITLE_STYLE_OPTIONS.find((o) => o.id === style) ?? null}
              onChange={(o) => setStyle(o?.id ?? CTitleStyle.NeutralClear)}
              placeholder="Chọn phong cách"
            />
          </div>
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">Số lượng title</label>
            <Input
              type="number"
              min={1}
              max={10}
              value={requestedCount}
              onChange={(e) => setRequestedCount(Number(e.target.value))}
            />
          </div>
          <div className="flex items-end">
            <Button onClick={() => doGenerate()} disabled={isGenerating || selectedNews.length === 0} className="w-full">
              {isGenerating ? <Loader2 className="h-4 w-4 animate-spin" /> : null}
              Tạo Video Title
            </Button>
          </div>
        </div>
        <div className="mt-3">
          <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">Từ khoá</label>
          <TagInput tags={keywords} onChange={setKeywords} placeholder="Nhập từ khoá..." />
        </div>
        <p className="mt-2 text-xs text-slate-400">Tin đã chọn: {selectedNews.length}</p>
      </div>

      {/* Block 2: Results */}
      <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4">
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-sm font-semibold text-slate-700 dark:text-slate-200">
            Kết quả ({allOptions.length})
          </h2>
          <div className="flex gap-2">
            {latestRequest && (
              <Button variant="outline" size="sm" onClick={() => doRegenerate(latestRequest.id)} disabled={isRegenerating}>
                Tạo thêm phương án
              </Button>
            )}
            <Button size="sm" disabled={!hasSelected} onClick={() => navigate('/pipeline/complete-set?projectId=' + encodeURIComponent(activeProjectId!))}>
              Xác nhận hoàn thành →
            </Button>
          </div>
        </div>

        {isLoading && (
          <div className="flex justify-center p-6"><Loader2 className="h-5 w-5 animate-spin text-slate-400" /></div>
        )}

        {!isLoading && allOptions.length === 0 && (
          <div className="p-6 text-center text-sm text-slate-400">Chưa có kết quả nào</div>
        )}

        {/* Options list */}
        {allOptions.map((opt) => (
          <div key={opt.id} className="flex items-center gap-3 rounded-lg border border-slate-100 dark:border-slate-800 p-3 mb-2">
            <Checkbox
              checked={opt.isSelected}
              onCheckedChange={() => doSelectOption({ id: opt.id, selected: !opt.isSelected })}
            />
            <span className="text-sm text-slate-700 dark:text-slate-200 flex-1">{opt.content}</span>
            <Badge variant="secondary">Round {opt.generationRound}</Badge>
          </div>
        ))}

        {/* Feedback section */}
        {latestRequest && (
          <div className="mt-4 border-t border-slate-200 dark:border-slate-700 pt-4">
            <h3 className="mb-2 text-sm font-medium text-slate-700 dark:text-slate-200">Feedback</h3>
            <div className="flex gap-2">
              <Textarea
                value={feedbackText}
                onChange={(e) => setFeedbackText(e.target.value)}
                placeholder="Nhập feedback để cải thiện title..."
                className="flex-1"
              />
              <Button
                onClick={() => doFeedback({ requestId: latestRequest.id, text: feedbackText })}
                disabled={isFeedbacking || !feedbackText.trim()}
              >
                <Send className="h-4 w-4" />
                {isFeedbacking ? 'Đang xử lý…' : 'Gửi'}
              </Button>
            </div>
            {/* Feedback history */}
            {latestRequest.feedbacks.length > 0 && (
              <div className="mt-3 space-y-1">
                <p className="text-xs text-slate-500">Lịch sử feedback:</p>
                {latestRequest.feedbacks.map((fb) => (
                  <p key={fb.id} className="text-xs text-slate-400 italic">
                    " {fb.feedbackText} " (Round {fb.appliedToRound})
                  </p>
                ))}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
