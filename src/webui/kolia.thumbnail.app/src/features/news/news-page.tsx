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
import { OperationProgress } from '../../components/operation-progress'
import {
  getNewsPaging,
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
  CRelevanceLevel,
  MARKET_SCOPE_OPTIONS,
  NEWS_TIME_RANGE_OPTIONS,
  NEWS_COUNT_FILTER_OPTIONS,
  NEWS_RECOMMENDATION_OPTIONS,
  getRelevanceLevelLabel,
  decodeEmotionTags,
} from '../../types/enums/pipeline.enums'

function ExpandableSummary({ text }: { text: string }) {
  const [expanded, setExpanded] = useState(false)
  const shouldTruncate = text.length > 120

  if (!shouldTruncate) return <p className="text-xs text-slate-600 dark:text-slate-400">{text}</p>

  return (
    <div className="text-xs text-slate-600 dark:text-slate-400">
      <span className={!expanded ? 'line-clamp-2' : ''}>{text}</span>
      <button
        type="button"
        onClick={() => setExpanded(!expanded)}
        className="text-[9px] text-slate-400 dark:text-slate-500 hover:text-blue-500 dark:hover:text-blue-400 ml-0.5"
      >
        {expanded ? 'thu gọn' : 'xem thêm'}
      </button>
    </div>
  )
}

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

  // ── Paging state ─────────────────────────────────────────────────────
  const [page, setPage] = useState(1)
  const pageSize = 20

  // ── Deep analysis ────────────────────────────────────────────────────
  const [deepAnalyses, setDeepAnalyses] = useState<Map<string, NewsDeepAnalysisDto>>(new Map())
  const [analyzingIds, setAnalyzingIds] = useState<Set<string>>(new Set())

  // ── Operation progress (SSE) ─────────────────────────────────────────
  const [progressOpId, setProgressOpId] = useState<string | null>(null)
  const [progressTitle, setProgressTitle] = useState('')

  // ── Fetch suggested keywords ─────────────────────────────────────────
  const { data: suggestedKeywords } = useQuery({
    queryKey: activeProjectId ? qk.news.suggestedKeywords(activeProjectId) : ['news', 'empty'],
    queryFn: () => getSuggestedKeywords(activeProjectId!),
    enabled: !!activeProjectId,
  })

  // ── Fetch news (phân trang) ──────────────────────────────────────────
  const { data: newsResult, isLoading, error, refetch } = useQuery({
    queryKey: activeProjectId ? [...qk.news.list(activeProjectId), page, pageSize] : ['news', 'empty'],
    queryFn: () => getNewsPaging(activeProjectId!, { pageNumber: page, pageSize }),
    enabled: !!activeProjectId,
  })
  const newsItems = newsResult?.items

  // ── Search mutation ──────────────────────────────────────────────────
  const { mutate: doSearch, isPending: isSearching } = useMutation({
    mutationFn: () => {
      const opId = crypto.randomUUID()
      setProgressOpId(opId)
      setProgressTitle('🔍 Đang tìm kiếm tin tức...')
      return searchNews(activeProjectId!, {
        marketScope: marketScope as CMarketScope,
        timeRange: timeRange as CNewsTimeRange,
        countFilter: countFilter as CNewsCountFilter,
        keywordsRaw: keywords.join('; '),
      }, opId)
    },
    onSuccess: (result) => {
      toast.success(`Tìm thấy ${result.items.length} tin tức`)
      setPage(1)
      queryClient.invalidateQueries({ queryKey: qk.news.list(activeProjectId!) })
    },
    onSettled: () => {
      // Giữ progress dialog mở để user thấy log hoàn tất, tự close sau 2s
      setTimeout(() => { progressOpId && setProgressOpId((prev) => prev === progressOpId ? null : prev) }, 2000)
    },
  })

  // ── Select mutation ──────────────────────────────────────────────────
  const { mutate: doSelect } = useMutation({
    mutationFn: ({ id, selected }: { id: string; selected: boolean }) =>
      selectNewsItem(activeProjectId!, id, selected),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: qk.news.list(activeProjectId!) })
    },
  })

  // ── Deep analyze mutation ────────────────────────────────────────────
  const { mutate: doDeepAnalyze } = useMutation({
    mutationFn: async (ids: string[]) => {
      setAnalyzingIds(new Set(ids))
      const opId = crypto.randomUUID()
      setProgressOpId(opId)
      setProgressTitle(`🧠 Phân tích sâu ${ids.length} tin...`)
      const results = await Promise.all(
        ids.map((id) => deepAnalyzeNews(activeProjectId!, id, opId)),
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
    onSettled: () => {
      setAnalyzingIds(new Set())
      setTimeout(() => setProgressOpId(null), 2000)
    },
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
  const totalCount = newsResult?.totalCount ?? 0
  const totalPages = newsResult?.totalPages ?? 1
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
          <TagInput tags={keywords} onChange={setKeywords} variant="textarea" placeholder="Nhập từ khoá, cách nhau bằng Enter hoặc ;" />
        </div>

        {/* Suggested keywords */}
        {suggestedKeywords && suggestedKeywords.length > 0 && (
          <div className="mt-3 flex flex-wrap gap-1.5">
            {suggestedKeywords.map((kw) => {
              const isActive = keywords.includes(kw)
              return (
                <KeywordPillButton
                  key={kw}
                  keyword={kw}
                  active={isActive}
                  onClick={(k) => {
                    if (isActive) {
                      setKeywords(keywords.filter((x) => x !== k))
                    } else {
                      setKeywords([...keywords, k])
                    }
                  }}
                />
              )
            })}
          </div>
        )}
      </div>

      {/* Block 2: Results */}
      <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900">
        {/* Summary bar */}
        <div className="flex items-center justify-between border-b border-slate-200 dark:border-slate-700 px-4 py-3">
          <div className="flex items-center gap-4 text-xs text-slate-500 dark:text-slate-400">
            <span>Tổng: <strong className="text-slate-700 dark:text-slate-200">{totalCount}</strong></span>
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
          <div className="overflow-x-auto max-h-[600px] overflow-y-auto">
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
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 dark:text-slate-400">Đề xuất</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 dark:text-slate-400">Thời gian</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 dark:text-slate-400">Link nguồn</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 dark:text-slate-400">Nhóm</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 dark:text-slate-400">Tóm tắt</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 dark:text-slate-400">Mức độ liên quan</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 dark:text-slate-400">Cảm xúc</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 dark:text-slate-400">Keyword research</th>
                </tr>
              </thead>
              <tbody>
                {newsList.map((item) => {
                  const emotions = decodeEmotionTags(item.emotionTags ?? 0)
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
                            const wasChecked = isInSelected || item.isSelectedByTeam
                            // wasChecked = trạng thái checkbox TRƯỚC khi click
                            // willBeChecked = trạng thái checkbox SAU khi click (luôn ngược lại)
                            if (wasChecked) {
                              setSelectedIds((prev) => {
                                const next = new Set(prev)
                                next.delete(item.id)
                                return next
                              })
                            } else {
                              setSelectedIds((prev) => new Set(prev).add(item.id))
                            }
                            // Gọi API với selected = ngược với server state hiện tại
                            toggleSelect(item.id, item.isSelectedByTeam)
                          }}
                          className="h-3.5 w-3.5 rounded border-slate-300 dark:border-slate-600"
                        />
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
                      <td className="whitespace-nowrap px-3 py-2 text-xs text-slate-500 dark:text-slate-400">
                        {item.publishedTime ? new Date(item.publishedTime).toLocaleDateString('vi-VN') : '—'}
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
                      <td className="px-3 py-2">
                        <Badge variant="outline">
                          {item.marketType === CMarketScope.International ? 'Quốc tế' : 'Nội địa'}
                        </Badge>
                      </td>
                      <td className="max-w-xs px-3 py-2">
                        <ExpandableSummary text={item.summaryOverview} />
                      </td>
                      <td className="px-3 py-2">
                        <Badge variant={item.relevanceLevel === CRelevanceLevel.High ? 'success' : item.relevanceLevel === CRelevanceLevel.Medium ? 'secondary' : 'outline'}>
                          {getRelevanceLevelLabel(item.relevanceLevel)}
                        </Badge>
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
                      <td className="max-w-xs px-3 py-2">
                        <p className="text-xs text-blue-600 dark:text-blue-400 leading-relaxed">
                          {(item.suggestedKeywordsForThumbnail ?? '').split(',').filter(Boolean).map((kw, i) => (
                            <span key={kw.trim() + i}>{i > 0 && <>, </>}{kw.trim()}</span>
                          ))}
                        </p>
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

        {/* Pagination */}
        {!isLoading && !error && totalPages > 1 && (
          <div className="flex items-center justify-between border-t border-slate-200 dark:border-slate-700 px-4 py-3">
            <span className="text-xs text-slate-500 dark:text-slate-400">
              Trang {page} / {totalPages}
            </span>
            <div className="flex items-center gap-2">
              <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage(page - 1)}>
                ← Trước
              </Button>
              <Button variant="outline" size="sm" disabled={page >= totalPages} onClick={() => setPage(page + 1)}>
                Sau →
              </Button>
            </div>
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
              const emotions = decodeEmotionTags(analysis.emotionTags)

              // Parse market reaction JSON object → key-value pairs
              let marketEntries: { label: string; value: string; color: string }[] = []
              try {
                const parsed = JSON.parse(analysis.marketReactionJson)
                if (typeof parsed === 'object' && !Array.isArray(parsed)) {
                  marketEntries = Object.entries(parsed).map(([k, v]) => ({
                    label: k,
                    value: String(v),
                    color: k.toLowerCase().includes('tích cực') || k.toLowerCase().includes('tăng') ? 'emerald' : 'red',
                  }))
                } else if (Array.isArray(parsed)) {
                  marketEntries = parsed.map((s: string) => ({ label: '', value: s, color: 'slate' }))
                }
              } catch { marketEntries = [{ label: '', value: analysis.marketReactionJson, color: 'slate' }] }

              // Parse sentiment JSON object → key-value pairs
              let sentimentEntries: { label: string; value: string }[] = []
              let fearGreedIndex: number | null = null
              try {
                const parsed = JSON.parse(analysis.sentimentOverviewJson)
                if (typeof parsed === 'object' && !Array.isArray(parsed)) {
                  for (const [k, v] of Object.entries(parsed)) {
                    if (k.toLowerCase().includes('fear') || k.toLowerCase().includes('greed') || k.toLowerCase().includes('index')) {
                      fearGreedIndex = Number(v)
                    } else {
                      sentimentEntries.push({ label: k, value: String(v) })
                    }
                  }
                } else if (Array.isArray(parsed)) {
                  sentimentEntries = parsed.map((s: string) => ({ label: '', value: s }))
                }
              } catch { sentimentEntries = [{ label: '', value: analysis.sentimentOverviewJson }] }

              const fgColor = fearGreedIndex !== null
                ? fearGreedIndex < 25 ? 'bg-red-500' : fearGreedIndex < 45 ? 'bg-orange-500' : fearGreedIndex < 55 ? 'bg-yellow-500' : fearGreedIndex < 75 ? 'bg-lime-500' : 'bg-emerald-500'
                : ''

              return (
                <div
                  key={analysis.id}
                  className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 overflow-hidden"
                >
                  {/* Header */}
                  <div className="border-b border-slate-100 dark:border-slate-800 bg-slate-50 dark:bg-slate-800/50 px-4 py-3">
                    <h3 className="text-sm font-semibold text-slate-800 dark:text-slate-200 line-clamp-1">
                      {newsItem?.title ?? 'Phân tích tin tức'}
                    </h3>
                    {newsItem?.sourceName && (
                      <p className="text-xs text-slate-400 dark:text-slate-500 mt-0.5">{newsItem.sourceName}</p>
                    )}
                  </div>

                  <div className="p-4 space-y-5">
                    {/* Macro Event Summary */}
                    <div>
                      <div className="flex items-center gap-2 mb-2">
                        <span className="text-base">📊</span>
                        <h4 className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                          Tóm tắt sự kiện vĩ mô
                        </h4>
                      </div>
                      <ul className="space-y-1.5">
                        {analysis.macroEventSummary.map((item, i) => (
                          <li key={i} className="flex items-start gap-2 text-xs text-slate-700 dark:text-slate-300">
                            <span className="mt-1 h-1.5 w-1.5 shrink-0 rounded-full bg-blue-400" />
                            {item}
                          </li>
                        ))}
                      </ul>
                    </div>

                    {/* Market Reaction & Sentiment side by side */}
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                      {/* Market Reaction */}
                      <div className="rounded-lg border border-slate-100 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-800/30 p-3">
                        <div className="flex items-center gap-2 mb-2">
                          <span className="text-base">📈</span>
                          <h4 className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                            Phản ứng thị trường
                          </h4>
                        </div>
                        <div className="space-y-2">
                          {marketEntries.map((entry, i) => (
                            <div key={i}>
                              {entry.label && (
                                <span className="text-[10px] font-medium text-slate-400 dark:text-slate-500 uppercase block mb-0.5">
                                  {entry.label}
                                </span>
                              )}
                              <span className={`text-xs font-medium ${
                                entry.color === 'emerald' ? 'text-emerald-600 dark:text-emerald-400' :
                                entry.color === 'red' ? 'text-red-600 dark:text-red-400' :
                                'text-slate-600 dark:text-slate-400'
                              }`}>
                                {entry.value}
                              </span>
                            </div>
                          ))}
                        </div>
                      </div>

                      {/* Sentiment */}
                      <div className="rounded-lg border border-slate-100 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-800/30 p-3">
                        <div className="flex items-center gap-2 mb-2">
                          <span className="text-base">🧠</span>
                          <h4 className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                            Đánh giá tâm lý
                          </h4>
                        </div>
                        <div className="space-y-2">
                          {sentimentEntries.map((entry, i) => (
                            <div key={i}>
                              {entry.label && (
                                <span className="text-[10px] font-medium text-slate-400 dark:text-slate-500 uppercase block mb-0.5">
                                  {entry.label}
                                </span>
                              )}
                              <span className="text-xs font-medium text-slate-700 dark:text-slate-300">
                                {entry.value}
                              </span>
                            </div>
                          ))}
                          {fearGreedIndex !== null && (
                            <div className="mt-2">
                              <span className="text-[10px] font-medium text-slate-400 dark:text-slate-500 uppercase block mb-1">
                                Fear & Greed Index
                              </span>
                              <div className="flex items-center gap-2">
                                <div className="flex-1 h-2 rounded-full bg-slate-200 dark:bg-slate-700 overflow-hidden">
                                  <div className={`h-full rounded-full ${fgColor} transition-all`} style={{ width: `${fearGreedIndex}%` }} />
                                </div>
                                <span className={`text-xs font-bold ${
                                  fearGreedIndex < 25 ? 'text-red-600' :
                                  fearGreedIndex < 45 ? 'text-orange-500' :
                                  fearGreedIndex < 55 ? 'text-yellow-500' :
                                  fearGreedIndex < 75 ? 'text-lime-500' :
                                  'text-emerald-500'
                                }`}>
                                  {fearGreedIndex}
                                </span>
                              </div>
                            </div>
                          )}
                        </div>
                      </div>
                    </div>

                    {/* Expectations */}
                    <div>
                      <div className="flex items-center gap-2 mb-2">
                        <span className="text-base">🎯</span>
                        <h4 className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                          Dự đoán kỳ vọng
                        </h4>
                      </div>
                      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                        <div className="rounded-lg border-l-4 border-l-blue-400 bg-blue-50/50 dark:bg-blue-950/20 dark:border-l-blue-600 p-3">
                          <p className="text-[10px] font-bold text-blue-600 dark:text-blue-400 uppercase mb-1">Ngắn hạn</p>
                          <p className="text-xs text-slate-700 dark:text-slate-300 leading-relaxed">{analysis.expectationShortTerm}</p>
                        </div>
                        <div className="rounded-lg border-l-4 border-l-violet-400 bg-violet-50/50 dark:bg-violet-950/20 dark:border-l-violet-600 p-3">
                          <p className="text-[10px] font-bold text-violet-600 dark:text-violet-400 uppercase mb-1">Dài hạn</p>
                          <p className="text-xs text-slate-700 dark:text-slate-300 leading-relaxed">{analysis.expectationLongTerm}</p>
                        </div>
                      </div>
                    </div>

                    {/* Emotions */}
                    {emotions.length > 0 && (
                      <div>
                        <div className="flex items-center gap-2 mb-2">
                          <span className="text-base">💡</span>
                          <h4 className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                            Cảm xúc có thể khai thác
                          </h4>
                        </div>
                        <div className="flex flex-wrap gap-1.5">
                          {emotions.map((em) => {
                            const emojiMap: Record<string, string> = {
                              'Sợ hãi': '😨',
                              'Nghi ngờ': '🤔',
                              'Tò mò': '🤩',
                              'Khẩn cấp': '🚨',
                              'Áp lực quyết định': '😰',
                              'Ngạc nhiên': '😲',
                              'Giận dữ': '😡',
                              'Hy vọng': '🌟',
                            }
                            return (
                              <span key={em} className="inline-flex items-center gap-1 rounded-lg bg-amber-50 dark:bg-amber-950/30 border border-amber-200 dark:border-amber-800/40 px-2.5 py-1 text-xs font-medium text-amber-800 dark:text-amber-300">
                                {emojiMap[em] && <span>{emojiMap[em]}</span>}
                                {em}
                              </span>
                            )
                          })}
                        </div>
                        {analysis.emotionReason && (
                          <p className="mt-2 text-xs text-slate-500 dark:text-slate-400 italic leading-relaxed border-l-2 border-slate-200 dark:border-slate-700 pl-3">
                            {analysis.emotionReason}
                          </p>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              )
            })}
          </div>
        </div>
      )}

      {/* Progress overlay */}
      <OperationProgress
        open={progressOpId !== null}
        onClose={() => setProgressOpId(null)}
        operationId={progressOpId}
        title={progressTitle}
      />
    </div>
  )
}
