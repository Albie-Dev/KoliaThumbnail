import { useState, useCallback } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Search, ExternalLink, Brain, Loader2 } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { Button } from '../../components/ui/button'
import { Badge } from '../../components/ui/badge'
import { TagInput } from '../../components/ui/tag-input'
import { KeywordPillButton } from '../../components/ui/keyword-pill-button'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { useActiveProjectId, EmptyProjectState } from '../../lib/project-context'
import { useStepGuard } from '../../lib/use-step-guard'
import {
  getNews,
  searchNews,
  getSuggestedKeywords,
  selectNewsItem,
  deepAnalyzeNews,
  type NewsDeepAnalysisDto,
} from './api'
import { qk } from '../../lib/query-keys'
import {
  CMarketScope,
  CNewsTimeRange,
  CNewsCountFilter,
  MARKET_SCOPE_OPTIONS,
  NEWS_TIME_RANGE_OPTIONS,
  NEWS_COUNT_FILTER_OPTIONS,
  NEWS_RECOMMENDATION_OPTIONS,
  decodeEmotionTags,
} from '../../types/enums/pipeline.enums'
import { ApiError } from '../../lib/api/api-error'

export function NewsPage() {
  const [activeProjectId] = useActiveProjectId()
  useStepGuard('/pipeline/news')
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  // ── Filter state ─────────────────────────────────────────────────────
  const [marketScope, setMarketScope] = useState<number>(CMarketScope.Both)
  const [timeRange, setTimeRange] = useState<number>(CNewsTimeRange.Last7Days)
  const [countFilter, setCountFilter] = useState<number>(CNewsCountFilter.Top20)
  const [keywords, setKeywords] = useState<string[]>([])
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())

  // ── Deep analysis ────────────────────────────────────────────────────
  const [deepAnalyses, setDeepAnalyses] = useState<Map<string, NewsDeepAnalysisDto>>(new Map())
  const [analyzingIds, setAnalyzingIds] = useState<Set<string>>(new Set())

  // ── Fetch suggested keywords ─────────────────────────────────────────
  const { data: suggestedKeywords } = useQuery({
    queryKey: activeProjectId ? qk.news.suggestedKeywords(activeProjectId) : ['news', 'empty'],
    queryFn: () => getSuggestedKeywords(activeProjectId!),
    enabled: !!activeProjectId,
  })

  // ── Fetch news ───────────────────────────────────────────────────────
  const { data: newsItems, isLoading, error, refetch } = useQuery({
    queryKey: activeProjectId ? qk.news.list(activeProjectId) : ['news', 'empty'],
    queryFn: () => getNews(activeProjectId!),
    enabled: !!activeProjectId,
  })

  // ── Search mutation ──────────────────────────────────────────────────
  const { mutate: doSearch, isPending: isSearching } = useMutation({
    mutationFn: () =>
      searchNews(activeProjectId!, {
        marketScope: marketScope as CMarketScope,
        timeRange: timeRange as CNewsTimeRange,
        countFilter: countFilter as CNewsCountFilter,
        keywordsRaw: keywords.join('; '),
      }),
    onSuccess: (result) => {
      toast.success(`Tìm thấy ${result.totalCount} tin tức`)
      queryClient.invalidateQueries({ queryKey: qk.news.list(activeProjectId!) })
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : 'Có lỗi xảy ra khi tìm tin.'),
  })

  // ── Select mutation ──────────────────────────────────────────────────
  const { mutate: doSelect } = useMutation({
    mutationFn: ({ id, selected }: { id: string; selected: boolean }) =>
      selectNewsItem(activeProjectId!, id, selected),
    onError: (err) => toast.error(err instanceof ApiError ? err.message : 'Có lỗi xảy ra.'),
  })

  // ── Deep analyze mutation ────────────────────────────────────────────
  const { mutate: doDeepAnalyze } = useMutation({
    mutationFn: async (ids: string[]) => {
      setAnalyzingIds(new Set(ids))
      const results = await Promise.all(
        ids.map((id) => deepAnalyzeNews(activeProjectId!, id)),
      )
      return { ids, results }
    },
    onSuccess: ({ ids, results }) => {
      const newMap = new Map(deepAnalyses)
      results.forEach((result) => newMap.set(result.newsItemId, result))
      setDeepAnalyses(newMap)
      toast.success(`Đã phân tích ${ids.length} tin`)
      queryClient.invalidateQueries({ queryKey: qk.news.list(activeProjectId!) })
    },
    onError: (err) => toast.error(err instanceof ApiError ? err.message : 'Có lỗi xảy ra khi phân tích.'),
    onSettled: () => setAnalyzingIds(new Set()),
  })

  // ── Handlers ─────────────────────────────────────────────────────────
  const toggleSelect = useCallback(
    (id: string, isSelected: boolean) => {
      doSelect({ id, selected: !isSelected })
    },
    [doSelect],
  )

  const handleSelectAll = useCallback(() => {
    if (!newsItems) return
    newsItems.forEach((item) => {
      if (!item.isSelectedByTeam) {
        doSelect({ id: item.id, selected: true })
      }
    })
  }, [newsItems, doSelect])

  const handleDeepAnalyze = useCallback(() => {
    const ids = Array.from(selectedIds)
    if (ids.length === 0) return
    doDeepAnalyze(ids)
  }, [selectedIds, doDeepAnalyze])

  const recommendationLabel = (rec: number) =>
    NEWS_RECOMMENDATION_OPTIONS.find((o) => o.id === rec)

  const newsList = newsItems ?? []
  const selectedCount = newsList.filter((n) => n.isSelectedByTeam).length
  const scannedSources = new Set(newsList.map((n) => n.sourceName)).size

  // ── Empty state ──────────────────────────────────────────────────────
  if (!activeProjectId) return <EmptyProjectState />

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      <h1 className="text-xl font-bold text-slate-900 dark:text-slate-100">Tin tức</h1>

      {/* Block 1: Filters */}
      <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4">
        <div className="grid grid-cols-1 gap-4 md:grid-cols-4">
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">
              Phạm vi
            </label>
            <SelectDropdown<{ id: number; label: string }>
              items={MARKET_SCOPE_OPTIONS}
              getOptionId={(o) => String(o.id)}
              getOptionLabel={(o) => o.label}
              value={MARKET_SCOPE_OPTIONS.find((o) => o.id === marketScope) ?? null}
              onChange={(o) => setMarketScope(o?.id ?? CMarketScope.Both)}
              placeholder="Chọn phạm vi"
            />
          </div>
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">
              Thời gian
            </label>
            <SelectDropdown<{ id: number; label: string; warning?: string }>
              items={NEWS_TIME_RANGE_OPTIONS}
              getOptionId={(o) => String(o.id)}
              getOptionLabel={(o) => o.label}
              value={NEWS_TIME_RANGE_OPTIONS.find((o) => o.id === timeRange) ?? null}
              onChange={(o) => setTimeRange(o?.id ?? CNewsTimeRange.Last7Days)}
              placeholder="Chọn thời gian"
            />
          </div>
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">
              Số lượng
            </label>
            <SelectDropdown<{ id: number; label: string }>
              items={NEWS_COUNT_FILTER_OPTIONS}
              getOptionId={(o) => String(o.id)}
              getOptionLabel={(o) => o.label}
              value={NEWS_COUNT_FILTER_OPTIONS.find((o) => o.id === countFilter) ?? null}
              onChange={(o) => setCountFilter(o?.id ?? CNewsCountFilter.Top20)}
              placeholder="Chọn số lượng"
            />
          </div>
          <div className="flex items-end">
            <Button onClick={() => doSearch()} disabled={isSearching || keywords.length === 0} className="w-full">
              {isSearching ? <Loader2 className="h-4 w-4 animate-spin" /> : <Search className="h-4 w-4" />}
              Tìm tin
            </Button>
          </div>
        </div>

        {/* Tags input */}
        <div className="mt-3">
          <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">
            Từ khoá
          </label>
          <TagInput tags={keywords} onChange={setKeywords} placeholder="Nhập từ khoá, cách nhau bằng Enter hoặc ;" />
        </div>

        {/* Suggested keywords */}
        {suggestedKeywords && suggestedKeywords.length > 0 && (
          <div className="mt-3 flex flex-wrap gap-1.5">
            {suggestedKeywords.map((kw) => (
              <KeywordPillButton
                key={kw}
                keyword={kw}
                onClick={(k) => {
                  if (!keywords.includes(k)) setKeywords([...keywords, k])
                }}
              />
            ))}
          </div>
        )}
      </div>

      {/* Block 2: Results */}
      <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900">
        {/* Summary bar */}
        <div className="flex items-center justify-between border-b border-slate-200 dark:border-slate-700 px-4 py-3">
          <div className="flex items-center gap-4 text-xs text-slate-500 dark:text-slate-400">
            <span>Tổng: <strong className="text-slate-700 dark:text-slate-200">{newsList.length}</strong></span>
            <span>Đã chọn: <strong className="text-emerald-600 dark:text-emerald-400">{selectedCount}</strong></span>
            <span>Nguồn đã quét: <strong>{scannedSources}</strong></span>
          </div>
          <div className="flex items-center gap-2">
            <Button variant="outline" size="sm" onClick={handleSelectAll}>
              Chọn tất cả
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={handleDeepAnalyze}
              disabled={selectedIds.size === 0 || analyzingIds.size > 0}
            >
              <Brain className="h-3.5 w-3.5" />
              {analyzingIds.size > 0 ? 'Đang phân tích…' : 'Phân tích sâu'}
            </Button>
            <Button
              size="sm"
              onClick={() => navigate('/pipeline/reference?projectId=' + encodeURIComponent(activeProjectId!))}
              disabled={selectedCount === 0}
            >
              Dùng tin đã chọn →
            </Button>
          </div>
        </div>

        {/* Loading/Error/Empty */}
        {isLoading && (
          <div className="flex items-center justify-center p-8">
            <Loader2 className="h-6 w-6 animate-spin text-slate-400" />
          </div>
        )}
        {error && (
          <div className="flex flex-col items-center gap-2 p-8">
            <p className="text-sm text-red-600 dark:text-red-400">
              {error instanceof Error ? error.message : 'Có lỗi xảy ra'}
            </p>
            <Button variant="outline" size="sm" onClick={() => void refetch()}>Thử lại</Button>
          </div>
        )}

        {/* News table */}
        {!isLoading && !error && newsList.length > 0 && (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800">
                  <th className="w-10 px-3 py-2 text-left">
                    <input
                      type="checkbox"
                      checked={selectedIds.size === newsList.length}
                      onChange={() => {
                        if (selectedIds.size === newsList.length) {
                          setSelectedIds(new Set())
                        } else {
                          setSelectedIds(new Set(newsList.map((n) => n.id)))
                        }
                      }}
                      className="h-3.5 w-3.5 rounded border-slate-300 dark:border-slate-600"
                    />
                  </th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 dark:text-slate-400">Thời gian</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 dark:text-slate-400">Tiêu đề</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 dark:text-slate-400">Tóm tắt</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 dark:text-slate-400">Đề xuất</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 dark:text-slate-400">Cảm xúc</th>
                </tr>
              </thead>
              <tbody>
                {newsList.map((item) => {
                  const emotions = decodeEmotionTags(item.relevanceLevel > 0 ? item.relevanceLevel : 0)
                  const rec = recommendationLabel(item.recommendation)
                  const isInSelected = selectedIds.has(item.id)

                  return (
                    <tr
                      key={item.id}
                      className="border-b border-slate-100 dark:border-slate-800 hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors"
                    >
                      <td className="px-3 py-2">
                        <input
                          type="checkbox"
                          checked={isInSelected || item.isSelectedByTeam}
                          onChange={() => {
                            if (isInSelected) {
                              setSelectedIds((prev) => {
                                const next = new Set(prev)
                                next.delete(item.id)
                                return next
                              })
                            } else {
                              setSelectedIds((prev) => new Set(prev).add(item.id))
                            }
                            toggleSelect(item.id, item.isSelectedByTeam)
                          }}
                          className="h-3.5 w-3.5 rounded border-slate-300 dark:border-slate-600"
                        />
                      </td>
                      <td className="whitespace-nowrap px-3 py-2 text-xs text-slate-500 dark:text-slate-400">
                        {item.scannedTime ? new Date(item.scannedTime).toLocaleDateString('vi-VN') : '—'}
                      </td>
                      <td className="px-3 py-2">
                        <div className="flex flex-col gap-0.5">
                          <a
                            href={item.sourceUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-sm font-medium text-slate-900 dark:text-slate-100 hover:text-blue-600 dark:hover:text-blue-400 inline-flex items-center gap-1"
                          >
                            {item.title}
                            <ExternalLink className="h-3 w-3 shrink-0" />
                          </a>
                          <span className="text-xs text-slate-400 dark:text-slate-500">{item.sourceName}</span>
                        </div>
                      </td>
                      <td className="max-w-xs px-3 py-2">
                        <p className="line-clamp-2 text-xs text-slate-600 dark:text-slate-400">
                          {item.summaryOverview}
                        </p>
                      </td>
                      <td className="px-3 py-2">
                        {rec && (
                          <Badge variant={
                            rec.id === 1 ? 'success' : rec.id === 2 ? 'secondary' : 'destructive'
                          }>
                            {rec.label}
                          </Badge>
                        )}
                      </td>
                      <td className="px-3 py-2">
                        <div className="flex flex-wrap gap-1">
                          {emotions.map((emotion) => (
                            <span
                              key={emotion}
                              className="inline-flex items-center rounded-full bg-slate-100 dark:bg-slate-800 px-2 py-0.5 text-[10px] text-slate-600 dark:text-slate-300"
                            >
                              {emotion}
                            </span>
                          ))}
                        </div>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}

        {/* Empty news */}
        {!isLoading && !error && newsList.length === 0 && (
          <div className="flex flex-col items-center gap-2 p-8 text-sm text-slate-400 dark:text-slate-500">
            Chưa có tin tức nào — hãy tìm kiếm tin tức
          </div>
        )}
      </div>

      {/* Deep analysis panel */}
      {deepAnalyses.size > 0 && (
        <div className="space-y-4">
          <h2 className="text-lg font-semibold text-slate-900 dark:text-slate-100">
            Phân tích sâu từng tin
          </h2>
          <div className="grid grid-cols-1 gap-4">
            {Array.from(deepAnalyses.entries()).map(([newsItemId, analysis]) => {
              const newsItem = newsList.find((n) => n.id === newsItemId)
              let marketReaction: string[] = []
              try {
                const parsed = JSON.parse(analysis.marketReactionJson)
                marketReaction = Array.isArray(parsed) ? parsed : [analysis.marketReactionJson]
              } catch {
                marketReaction = [analysis.marketReactionJson]
              }

              let sentimentOverview: string[] = []
              try {
                const parsed = JSON.parse(analysis.sentimentOverviewJson)
                sentimentOverview = Array.isArray(parsed) ? parsed : [analysis.sentimentOverviewJson]
              } catch {
                sentimentOverview = [analysis.sentimentOverviewJson]
              }

              const emotions = decodeEmotionTags(analysis.emotionTags)

              return (
                <div
                  key={analysis.id}
                  className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4"
                >
                  <h3 className="mb-3 text-sm font-semibold text-slate-700 dark:text-slate-200">
                    {newsItem?.title ?? 'Phân tích tin tức'}
                  </h3>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <h4 className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase mb-1">
                        Tóm tắt sự kiện vĩ mô
                      </h4>
                      <ul className="list-inside list-disc text-xs text-slate-600 dark:text-slate-400">
                        {analysis.macroEventSummary.map((item, i) => (
                          <li key={i}>{item}</li>
                        ))}
                      </ul>
                    </div>
                    <div>
                      <h4 className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase mb-1">
                        Phản ứng thị trường
                      </h4>
                      <ul className="list-inside list-disc text-xs text-slate-600 dark:text-slate-400">
                        {marketReaction.map((item, i) => (
                          <li key={i}>{item}</li>
                        ))}
                      </ul>
                    </div>
                    <div>
                      <h4 className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase mb-1">
                        Dự đoán kỳ vọng
                      </h4>
                      <p className="text-xs text-slate-600 dark:text-slate-400">
                        <strong>Ngắn hạn:</strong> {analysis.expectationShortTerm}
                      </p>
                      <p className="mt-1 text-xs text-slate-600 dark:text-slate-400">
                        <strong>Dài hạn:</strong> {analysis.expectationLongTerm}
                      </p>
                    </div>
                    <div>
                      <h4 className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase mb-1">
                        Đánh giá tâm lý
                      </h4>
                      <ul className="list-inside list-disc text-xs text-slate-600 dark:text-slate-400">
                        {sentimentOverview.map((item, i) => (
                          <li key={i}>{item}</li>
                        ))}
                      </ul>
                    </div>
                  </div>
                  {emotions.length > 0 && (
                    <div className="mt-3 border-t border-slate-100 dark:border-slate-800 pt-3">
                      <h4 className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase mb-1">
                        Cảm xúc có thể khai thác
                      </h4>
                      <div className="flex flex-wrap gap-1">
                        {emotions.map((em) => (
                          <Badge key={em} variant="secondary">{em}</Badge>
                        ))}
                      </div>
                      {analysis.emotionReason && (
                        <p className="mt-1 text-xs text-slate-500 dark:text-slate-400 italic">
                          {analysis.emotionReason}
                        </p>
                      )}
                    </div>
                  )}
                </div>
              )
            })}
          </div>
        </div>
      )}
    </div>
  )
}
