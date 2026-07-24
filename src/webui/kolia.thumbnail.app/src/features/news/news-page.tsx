import { useState, useCallback, useMemo } from 'react'

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Search, ExternalLink, Brain, Loader2, Eye, ChevronUp } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { DataTable, type DataTableColumn } from '../../components/data-table/data-table'
import { useDataTableState } from '../../components/data-table/use-data-table-state'
import { SortDirection } from '../../types/paging.types'
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
  getNewsSources,
  selectNewsItem,
  getDeepAnalysis,
  deepAnalyzeNews,
  type NewsDeepAnalysisDto,
  type NewsSourceSelectDto,
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
  decodeEmotionTags,
  getEmotionBadgeClass,
  RELEVANCE_LEVEL_OPTIONS,
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
  const [selectedSources, setSelectedSources] = useState<NewsSourceSelectDto[]>([])

  // ── Paging state ─────────────────────────────────────────────────────
  const { page, setPage, pageSize, setPageSize, search, setSearch, sortBy, sortOrder, handleSort } = useDataTableState(1, 20)

  // ── Deep analysis ────────────────────────────────────────────────────
  const [deepAnalyses, setDeepAnalyses] = useState<Map<string, NewsDeepAnalysisDto>>(new Map())
  const [analyzingIds, setAnalyzingIds] = useState<Set<string>>(new Set())
  // ID của tin đang được xem deep analysis panel (null = không hiển thị)
  const [viewingDeepId, setViewingDeepId] = useState<string | null>(null)

  // ── Operation progress (SSE) ─────────────────────────────────────────
  const [progressOpId, setProgressOpId] = useState<string | null>(null)
  const [progressTitle, setProgressTitle] = useState('')

  // ── Fetch suggested keywords ─────────────────────────────────────────
  const { data: suggestedKeywords } = useQuery({
    queryKey: activeProjectId ? qk.news.suggestedKeywords(activeProjectId) : ['news', 'empty'],
    queryFn: () => getSuggestedKeywords(activeProjectId!),
    enabled: !!activeProjectId,
  })

  // ── Fetch all available news sources for "Select All" ───────────────────
  const { data: allSources } = useQuery({
    queryKey: activeProjectId ? ['news-sources', activeProjectId, marketScope] : ['news-sources', 'empty'],
    queryFn: () => getNewsSources(activeProjectId!, {
      pageNumber: 1,
      pageSize: 1000, // Get all sources
      region: marketScope as CMarketScope,
    }),
    enabled: !!activeProjectId,
  })

  // ── Fetch news (phân trang) ──────────────────────────────────────────
  const { data: newsResult, isLoading, error, refetch } = useQuery({
    queryKey: activeProjectId ? [...qk.news.list(activeProjectId), page, pageSize, search, sortBy, sortOrder] : ['news', 'empty'],
    queryFn: () => {
      const sorts = sortBy ? [{
        field: sortBy.charAt(0).toUpperCase() + sortBy.slice(1),
        direction: sortOrder === 'desc' ? SortDirection.Desc : SortDirection.Asc
      }] : undefined

      return getNewsPaging(activeProjectId!, {
        pageNumber: page,
        pageSize,
        searchText: search || undefined,
        sorts
      })
    },
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
        selectedSourceIds: selectedSources.map((s) => s.id),
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

  const handleSelectAllSources = useCallback(() => {
    if (!allSources?.items || allSources.items.length === 0) return
    setSelectedSources(allSources.items)
  }, [allSources])

  const handleClearSources = useCallback(() => {
    setSelectedSources([])
  }, [])

  const handleDeepAnalyze = useCallback(() => {
    const ids = Array.from(selectedIds)
    if (ids.length === 0) return

    // Lọc ra các tin chưa được phân tích hoặc phân tích bị lỗi (Status = Failed / 2)
    const unanalyzedIds = ids.filter((id) => {
      const existing = deepAnalyses.get(id)
      return !existing || existing.status === 2 // 2 = Failed
    })

    if (unanalyzedIds.length === 0) {
      toast.info('Tất cả bản tin đã chọn đều đã có phân tích sâu.')
      return
    }

    doDeepAnalyze(unanalyzedIds)
  }, [selectedIds, deepAnalyses, doDeepAnalyze])

  // Xem deep analysis của 1 tin: load nếu chưa có trong cache, rồi toggle panel
  const handleViewDeep = useCallback(async (id: string) => {
    if (viewingDeepId === id) {
      setViewingDeepId(null)
      return
    }
    if (!deepAnalyses.has(id) && activeProjectId) {
      try {
        const analysis = await getDeepAnalysis(activeProjectId, id)
        setDeepAnalyses((prev) => new Map(prev).set(id, analysis))
      } catch {
        toast.error('Không tải được phân tích sâu.')
        return
      }
    }
    setViewingDeepId(id)
  }, [viewingDeepId, deepAnalyses, activeProjectId])

  // Phân tích sâu ngay từ cột trong bảng (1 tin)
  const handleDeepAnalyzeSingle = useCallback((id: string) => {
    doDeepAnalyze([id])
  }, [doDeepAnalyze])

  const recommendationLabel = (rec: number) =>
    NEWS_RECOMMENDATION_OPTIONS.find((o) => o.id === rec)

  const newsList = newsItems ?? []

  const columns: DataTableColumn<any>[] = useMemo(() => [
    {
      key: 'select',
      header: (
        <input
          type="checkbox"
          checked={newsList.length > 0 && selectedIds.size === newsList.length}
          onChange={() => {
            if (newsList.length > 0 && selectedIds.size === newsList.length) {
              setSelectedIds(new Set())
            } else {
              setSelectedIds(new Set(newsList.map((n) => n.id)))
            }
          }}
          className="h-3.5 w-3.5 rounded border-slate-300 dark:border-slate-600"
        />
      ),
      render: (item) => {
        const isInSelected = selectedIds.has(item.id)
        return (
          <input
            type="checkbox"
            checked={isInSelected || item.isSelectedByTeam}
            onChange={() => {
              const wasChecked = isInSelected || item.isSelectedByTeam
              if (wasChecked) {
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
        )
      }
    },
    {
      key: 'recommendation',
      header: 'Đề xuất',
      sortable: true,
      render: (item) => {
        const rec = recommendationLabel(item.recommendation)
        if (!rec) return null
        return (
          <Badge variant={rec.id === 1 ? 'success' : rec.id === 2 ? 'secondary' : 'destructive'}
            className='flex flex-wrap gap-1 min-w-[100px]'>
            {rec.label}
          </Badge>
        )
      }
    },
    {
      key: 'publishedTime',
      header: 'Thời gian',
      sortable: true,
      render: (item) => item.publishedTime ? new Date(item.publishedTime).toLocaleDateString('vi-VN') : '—'
    },
    {
      key: 'title',
      header: 'Link nguồn',
      sortable: true,
      render: (item) => (
        <div className="flex flex-col gap-0.5 min-w-[300px]">
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
      )
    },
    {
      key: 'marketType',
      header: 'Nhóm',
      sortable: true,
      render: (item) => (
        <Badge variant={item.marketType === CMarketScope.International ? 'lime' : 'secondary'}
          className="flex flex-col gap-0.5 min-w-[60px]">
          {item.marketType === CMarketScope.International ? 'Quốc tế' : 'Nội địa'}
        </Badge>
      )
    },
    {
      key: 'summaryOverview',
      header: 'Tóm tắt',
      render: (item) => (
        <div className="min-w-[250px]">
          <ExpandableSummary text={item.summaryOverview} />
        </div>
      )
    },
    {
      key: 'relevanceLevel',
      header: 'Liên quan',
      sortable: true,
      render: (item) => {
        const relevanceOption =
          RELEVANCE_LEVEL_OPTIONS.find(o => o.id === item.relevanceLevel) ??
          RELEVANCE_LEVEL_OPTIONS.find(o => o.id === CRelevanceLevel.None)!

        const highlightedDataScore =
          item.totalScore -
          item.relevanceToTopicScore -
          item.importanceImpactScore -
          item.emotionPotentialScore -
          item.noveltyDataScore

        return (
          <div className='flex flex-wrap gap-1 min-w-[80px]'
            title={`Điểm chi tiết:
      • Độ liên quan với chủ đề: ${item.relevanceToTopicScore}/30
      • Độ quan trọng/tác động thị trường: ${item.importanceImpactScore}/20
      • Khả năng tạo cảm xúc: ${item.emotionPotentialScore}/20
      • Độ mới của tin: ${item.noveltyDataScore}/15
      • Dữ liệu/số liệu nổi bật: ${highlightedDataScore}/15
      • Tổng: ${item.totalScore}/100`}
          >
            <Badge className={relevanceOption.badgeClass}>
              {relevanceOption.label}
            </Badge>
          </div>
        )
      }
    },
    {
      key: 'emotionTags',
      header: 'Cảm xúc',
      render: (item) => {
        const emotions = decodeEmotionTags(item.emotionTags ?? 0)
        return (
          <div className="flex flex-wrap gap-1 min-w-[150px]">
            {emotions.map((emotion) => {
              const emojiMap: Record<string, string> = {
                'Sợ hãi': '😨',
                'Nghi ngờ': '🤔',
                'Tò mò': '🤩',
                'Khẩn cấp': '🚨',
                'Áp lực quyết định': '😰',
                'Ngạc nhiên': '😲',
                'Giận dữ': '😡',
                'Hy vọng': '🌟',
                'FOMO': '📈',
              }
              return (
                <span
                  key={emotion}
                  className={`inline-flex items-center gap-1 rounded-full border px-2 py-0.5 text-[10px] font-medium ${getEmotionBadgeClass(emotion)}`}
                >
                  {emojiMap[emotion] && <span>{emojiMap[emotion]}</span>}
                  {emotion}
                </span>
              )
            })}
          </div>
        )
      }
    },
    {
      key: 'suggestedKeywordsForThumbnail',
      header: 'Keyword research',
      render: (item) => (
        <div className="min-w-[150px]">
          <p className="text-xs text-blue-600 dark:text-blue-400 leading-relaxed">
            {(item.suggestedKeywordsForThumbnail ?? '').split(',').filter(Boolean).map((kw: string, i: number) => (
              <span key={kw.trim() + i}>{i > 0 && <>, </>}{kw.trim()}</span>
            ))}
          </p>
        </div>
      )
    },
    {
      key: 'deepAnalysis',
      header: 'Hành động',
      stickyRight: true,
      render: (item) => {
        const isAnalyzing = analyzingIds.has(item.id)
        const isViewing = viewingDeepId === item.id

        if (item.hasDeepAnalysis || deepAnalyses.has(item.id)) {
          return (
            <div className="flex justify-center">
              <button
                type="button"
                onClick={() => handleViewDeep(item.id)}
                title={isViewing ? 'Đóng phân tích sâu' : 'Xem phân tích sâu'}
                aria-label={isViewing ? 'Đóng phân tích sâu' : 'Xem phân tích sâu'}
                className="inline-flex h-7 w-7 items-center justify-center rounded-md border border-blue-200 dark:border-blue-800 bg-blue-50 dark:bg-blue-950/40 text-blue-700 dark:text-blue-300 hover:bg-blue-100 dark:hover:bg-blue-900/50 transition-colors"
              >
                {isViewing ? <ChevronUp className="h-3.5 w-3.5" /> : <Eye className="h-3.5 w-3.5" />}
              </button>
            </div>
          )
        }

        return (
          <div className="flex justify-center">
            <button
              type="button"
              disabled={isAnalyzing}
              onClick={() => handleDeepAnalyzeSingle(item.id)}
              title={isAnalyzing ? 'Đang phân tích…' : 'Phân tích sâu'}
              aria-label={isAnalyzing ? 'Đang phân tích…' : 'Phân tích sâu'}
              className="inline-flex h-7 w-7 items-center justify-center rounded-md border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isAnalyzing
                ? <Loader2 className="h-3.5 w-3.5 animate-spin" />
                : <Brain className="h-3.5 w-3.5" />
              }
            </button>
          </div>
        )
      }
    }
  ], [newsList, selectedIds, toggleSelect, recommendationLabel, analyzingIds, viewingDeepId, deepAnalyses, handleViewDeep, handleDeepAnalyzeSingle])

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

        {/* News Source Selector */}
        <div className="mt-3">
          <div className="mb-1 flex items-center justify-between">
            <label className="block text-xs font-medium text-slate-600 dark:text-slate-300">
              Nguồn tin cụ thể
            </label>
            <label className="flex items-center gap-1.5 cursor-pointer">
              <input
                type="checkbox"
                checked={selectedSources.length > 0}
                onChange={(e) => {
                  if (e.target.checked) {
                    handleSelectAllSources()
                  } else {
                    handleClearSources()
                  }
                }}
                className="h-3 w-3 rounded border-slate-300 dark:border-slate-600"
              />
              <span className="text-[10px] text-slate-500 dark:text-slate-400">
                {selectedSources.length > 0 ? `${selectedSources.length} nguồn` : 'Tất cả'}
              </span>
            </label>
          </div>
          <div className="relative">
            <SelectDropdown<NewsSourceSelectDto>
              fetchData={async (params) => {
                const result = await getNewsSources(activeProjectId!, {
                  ...params,
                  region: marketScope as CMarketScope,
                })
                return {
                  items: result.items,
                  pageInfo: {
                    pageNumber: result.pageNumber,
                    pageSize: result.pageSize,
                    totalRecords: result.totalCount,
                    totalPages: result.totalPages,
                    hasNextPage: result.pageNumber < result.totalPages,
                    hasPreviousPage: result.pageNumber > 1,
                  },
                }
              }}
              getOptionId={(s) => s.id}
              getOptionLabel={(s) => {
                const regionLabel = s.region === CMarketScope.Domestic ? '🇻🇳 ' : s.region === CMarketScope.International ? '🌍 ' : '🌐 '
                return `${regionLabel}${s.name}`
              }}
              placeholder="Tìm nguồn tin..."
              multiple={true}
              value={selectedSources}
              onChange={setSelectedSources}
              pageSize={20}
            />
          </div>
          {selectedSources.length === 0 && (
            <p className="mt-1 text-[10px] text-slate-400 dark:text-slate-500">
              Tự động tìm theo phạm vi thị trường đã chọn
            </p>
          )}
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
      <DataTable
        columns={columns}
        data={newsList}
        isLoading={isLoading}
        error={error instanceof Error ? error.message : error ? 'Có lỗi xảy ra' : null}
        onRetry={() => void refetch()}
        title="Tin tức"
        emptyMessage="Chưa có tin tức nào — hãy tìm kiếm tin tức"
        page={page}
        pageSize={pageSize}
        totalPages={totalPages}
        totalCount={totalCount}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        search={search}
        searchPlaceholder="Tìm nhanh trong trang hiện tại..."
        onSearchChange={setSearch}
        onSearchClear={() => setSearch('')}
        sortBy={sortBy}
        sortOrder={sortOrder}
        onSort={handleSort}
        actions={
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-4 text-xs text-slate-500 dark:text-slate-400 border-r border-slate-200 dark:border-slate-700 pr-4">
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
        }
      />

      {/* Deep analysis panel — chỉ hiện khi user click "Xem" từ cột trong bảng */}
      {viewingDeepId && deepAnalyses.has(viewingDeepId) && (() => {
        const analysis = deepAnalyses.get(viewingDeepId)!
        const newsItem = newsList.find((n) => n.id === viewingDeepId)
        const emotions = decodeEmotionTags(analysis.emotionTags)

        let sentimentLabel = 'Trung lập'
        let sentimentBadgeClass = 'bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-300'
        const sVal = String(analysis.sentimentOverview?.sentiment ?? '').toLowerCase()
        if (sVal === '1' || sVal.includes('optimistic') || sVal.includes('lạc quan')) {
          sentimentLabel = '📈 Lạc quan'
          sentimentBadgeClass = 'bg-emerald-100 text-emerald-800 dark:bg-emerald-950/50 dark:text-emerald-300 border-emerald-200 dark:border-emerald-800'
        } else if (sVal === '2' || sVal.includes('pessimistic') || sVal.includes('bi quan')) {
          sentimentLabel = '📉 Bi quan'
          sentimentBadgeClass = 'bg-red-100 text-red-800 dark:bg-red-950/50 dark:text-red-300 border-red-200 dark:border-red-800'
        } else if (sVal === '4' || sVal.includes('mixed') || sVal.includes('giằng co')) {
          sentimentLabel = '🔄 Giằng co'
          sentimentBadgeClass = 'bg-amber-100 text-amber-800 dark:bg-amber-950/50 dark:text-amber-300 border-amber-200 dark:border-amber-800'
        } else {
          sentimentLabel = '⚖️ Trung lập'
          sentimentBadgeClass = 'bg-slate-100 text-slate-800 dark:bg-slate-800 dark:text-slate-300 border-slate-200 dark:border-slate-700'
        }

        return (
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold text-slate-900 dark:text-slate-100">
                Phân tích sâu (4 Tầng)
              </h2>
              <button
                type="button"
                onClick={() => setViewingDeepId(null)}
                className="text-xs text-slate-400 hover:text-slate-600 dark:hover:text-slate-200"
              >
                ✕ Đóng
              </button>
            </div>
            <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 overflow-hidden">
              {/* Header */}
              <div className="border-b border-slate-100 dark:border-slate-800 bg-slate-50 dark:bg-slate-800/50 px-4 py-3 flex items-center justify-between flex-wrap gap-2">
                <div>
                  <h3 className="text-sm font-semibold text-slate-800 dark:text-slate-200 line-clamp-1">
                    {newsItem?.title ?? 'Phân tích tin tức'}
                  </h3>
                  {newsItem?.sourceName && (
                    <p className="text-xs text-slate-400 dark:text-slate-500 mt-0.5">{newsItem.sourceName}</p>
                  )}
                </div>
                <div className="flex items-center gap-2">
                  {analysis.wasTranslatedFromForeign && (
                    <Badge variant="secondary" className="text-[10px]">
                      🌐 Đã dịch từ nguồn quốc tế
                    </Badge>
                  )}
                  {analysis.status === 2 && (
                    <Badge variant="destructive" className="text-[10px]">
                      ⚠️ Phân tích thất bại (Cần thử lại)
                    </Badge>
                  )}
                </div>
              </div>

              <div className="p-4 space-y-5">
                {/* Tầng 1: Macro Event Summary */}
                <div>
                  <div className="flex items-center gap-2 mb-2">
                    <span className="text-base">📊</span>
                    <h4 className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                      Tầng 1: Sự kiện vĩ mô theo hạng mục
                    </h4>
                  </div>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
                    {Array.isArray(analysis.macroEventSummary) && analysis.macroEventSummary.map((evt, i) => (
                      <div key={i} className="flex items-start gap-2 text-xs rounded-lg border border-slate-100 dark:border-slate-800 p-2 bg-slate-50/50 dark:bg-slate-800/30">
                        <span className="font-semibold text-slate-800 dark:text-slate-200 shrink-0 bg-slate-200 dark:bg-slate-700 px-1.5 py-0.5 rounded text-[10px]">
                          {evt.category}
                        </span>
                        <span className="text-slate-600 dark:text-slate-400 leading-relaxed">{evt.content}</span>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Tầng 2 & 4 side by side */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div className="rounded-lg border border-slate-100 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-800/30 p-3">
                    <div className="flex items-center gap-2 mb-2">
                      <span className="text-base">📈</span>
                      <h4 className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                        Tầng 2: Phản ứng thị trường & Ý kiến
                      </h4>
                    </div>
                    <div className="space-y-2">
                      {Array.isArray(analysis.marketReaction) && analysis.marketReaction.map((entry, i) => (
                        <div key={i} className="border-b border-slate-100 dark:border-slate-800/60 pb-1.5 last:border-0 last:pb-0">
                          <span className="text-[10px] font-bold text-slate-500 dark:text-slate-400 uppercase block mb-0.5">
                            {entry.marketOrTopic}
                          </span>
                          <span className="text-xs font-medium text-slate-700 dark:text-slate-300 leading-relaxed">
                            {entry.content}
                          </span>
                        </div>
                      ))}
                    </div>
                  </div>

                  <div className="rounded-lg border border-slate-100 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-800/30 p-3">
                    <div className="flex items-center gap-2 mb-2">
                      <span className="text-base">🧠</span>
                      <h4 className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                        Tầng 4: Đánh giá tâm lý tổng quan
                      </h4>
                    </div>
                    <div className="space-y-2">
                      <div className="flex items-center gap-2">
                        <span className="text-xs text-slate-400">Trạng thái:</span>
                        <span className={`inline-flex items-center rounded-md border px-2 py-0.5 text-xs font-semibold ${sentimentBadgeClass}`}>
                          {sentimentLabel}
                        </span>
                      </div>
                      {analysis.sentimentOverview?.rationale && (
                        <p className="text-xs text-slate-600 dark:text-slate-400 leading-relaxed mt-1">
                          {analysis.sentimentOverview.rationale}
                        </p>
                      )}
                    </div>
                  </div>
                </div>

                {/* Tầng 3: Expectations */}
                <div>
                  <div className="flex items-center gap-2 mb-2">
                    <span className="text-base">🎯</span>
                    <h4 className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                      Tầng 3: Dự đoán kỳ vọng (1-3 tháng & 6-12 tháng)
                    </h4>
                  </div>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    <div className="rounded-lg border-l-4 border-l-blue-400 bg-blue-50/50 dark:bg-blue-950/20 dark:border-l-blue-600 p-3">
                      <p className="text-[10px] font-bold text-blue-600 dark:text-blue-400 uppercase mb-1">Ngắn hạn (1-3 tháng)</p>
                      <p className="text-xs text-slate-700 dark:text-slate-300 leading-relaxed">{analysis.expectationShortTerm}</p>
                    </div>
                    <div className="rounded-lg border-l-4 border-l-violet-400 bg-violet-50/50 dark:bg-violet-950/20 dark:border-l-violet-600 p-3">
                      <p className="text-[10px] font-bold text-violet-600 dark:text-violet-400 uppercase mb-1">Dài hạn (6-12 tháng)</p>
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
                        Tag Cảm xúc khai thác
                      </h4>
                    </div>
                    <div className="flex flex-wrap gap-1.5">
                      {emotions.map((em) => {
                        const emojiMap: Record<string, string> = {
                          'Sợ hãi': '😨', 'Nghi ngờ': '🤔', 'Tò mò': '🤩',
                          'Khẩn cấp': '🚨', 'Áp lực quyết định': '😰', 'Ngạc nhiên': '😲',
                          'Giận dữ': '😡', 'Hy vọng': '🌟', 'FOMO': '📈',
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

                {/* Missing Data Note */}
                {analysis.missingDataNote && (
                  <p className="text-[11px] text-slate-400 dark:text-slate-500 italic">
                    * Ghi chú dữ liệu: {analysis.missingDataNote}
                  </p>
                )}
              </div>
            </div>
          </div>
        )
      })()}

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

