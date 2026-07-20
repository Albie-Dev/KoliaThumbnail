import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Search, Loader2 } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '../../components/ui/button'
import { Input } from '../../components/ui/input'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { CheckableThumbnailCard } from '../../components/ui/checkable-thumbnail-card'
import { useActiveProjectId, EmptyProjectState } from '../../lib/project-context'
import { useStepGuard } from '../../lib/use-step-guard'
import { getNews } from '../news/api'
import { searchThumbnails, updateThumbnailStatus } from './api'
import { qk } from '../../lib/query-keys'
import {
  CThumbnailTimeFilter,
  CThumbnailSortFilter,
  CLibraryUserStatus,
  THUMBNAIL_TIME_FILTER_OPTIONS,
  THUMBNAIL_SORT_FILTER_OPTIONS,
} from '../../types/enums/pipeline.enums'

export function ReferenceSearchPage() {
  const [activeProjectId] = useActiveProjectId()
  useStepGuard('/pipeline/reference')
  const queryClient = useQueryClient()

  const [keyword, setKeyword] = useState('')
  const [timeFilter, setTimeFilter] = useState<number>(CThumbnailTimeFilter.OneMonth)
  const [sortFilter, setSortFilter] = useState<number>(CThumbnailSortFilter.MostRelevant)
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())

  // Get suggested keywords from selected news
  const { data: newsItems } = useQuery({
    queryKey: activeProjectId ? qk.news.list(activeProjectId) : ['news', 'empty'],
    queryFn: () => getNews(activeProjectId!),
    enabled: !!activeProjectId,
  })

  const suggestedKeywords = Array.from(
    new Set(
      (newsItems ?? [])
        .filter((n) => n.isSelectedByTeam && n.suggestedKeywordsForThumbnail)
        .flatMap((n) => (n.suggestedKeywordsForThumbnail ?? '').split(',').map((s) => s.trim()).filter(Boolean)),
    ),
  )

  // Search
  const { data: searchResult, mutate: doSearch, isPending: isSearching } = useMutation({
    mutationFn: () =>
      searchThumbnails(activeProjectId!, {
        keyword,
        timeFilter: timeFilter as CThumbnailTimeFilter,
        sortFilter: sortFilter as CThumbnailSortFilter,
      }),
  })

  // Approve selected
  const { mutate: doApprove } = useMutation({
    mutationFn: async (ids: string[]) => {
      for (const id of ids) {
        await updateThumbnailStatus(activeProjectId!, id, CLibraryUserStatus.Approved)
      }
    },
    onSuccess: () => {
      toast.success('Đã chọn thumbnail!')
      queryClient.invalidateQueries({ queryKey: qk.thumbnailLibrary.list(activeProjectId!) })
      setSelectedIds(new Set())
    },
  })

  const items = searchResult?.items ?? []

  if (!activeProjectId) return <EmptyProjectState />

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      <h1 className="text-xl font-bold text-slate-900 dark:text-slate-100">3. Thumbnail tham khảo</h1>

      {/* Filters */}
      <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4">
        <div className="grid grid-cols-1 gap-4 md:grid-cols-4">
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">Từ khoá</label>
            <Input value={keyword} onChange={(e) => setKeyword(e.target.value)} placeholder="Nhập từ khoá..." />
          </div>
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">Thời gian</label>
            <SelectDropdown<{ id: number; label: string }>
              items={THUMBNAIL_TIME_FILTER_OPTIONS}
              getOptionId={(o) => String(o.id)}
              getOptionLabel={(o) => o.label}
              value={THUMBNAIL_TIME_FILTER_OPTIONS.find((o) => o.id === timeFilter) ?? null}
              onChange={(o) => setTimeFilter(o?.id ?? CThumbnailTimeFilter.OneMonth)}
              placeholder="Chọn thời gian"
            />
          </div>
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">Sắp xếp</label>
            <SelectDropdown<{ id: number; label: string }>
              items={THUMBNAIL_SORT_FILTER_OPTIONS}
              getOptionId={(o) => String(o.id)}
              getOptionLabel={(o) => o.label}
              value={THUMBNAIL_SORT_FILTER_OPTIONS.find((o) => o.id === sortFilter) ?? null}
              onChange={(o) => setSortFilter(o?.id ?? CThumbnailSortFilter.MostRelevant)}
              placeholder="Sắp xếp"
            />
          </div>
          <div className="flex items-end gap-2">
            <Button onClick={() => doSearch()} disabled={isSearching || !keyword.trim()} className="flex-1">
              {isSearching ? <Loader2 className="h-4 w-4 animate-spin" /> : <Search className="h-4 w-4" />}
              Tìm thumbnail
            </Button>
          </div>
        </div>

        {/* Suggested keyword pills */}
        {suggestedKeywords.length > 0 && (
          <div className="mt-3 flex flex-wrap gap-1.5">
            <span className="text-xs text-slate-500 dark:text-slate-400 self-center">Keyword gợi ý:</span>
            {suggestedKeywords.map((kw) => (
              <button
                key={kw}
                type="button"
                onClick={() => setKeyword(kw)}
                className="inline-flex items-center rounded-full border border-slate-200 dark:border-slate-700 px-2 py-0.5 text-xs text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
              >
                + {kw}
              </button>
            ))}
          </div>
        )}
      </div>

      {/* Results grid */}
      {items.length > 0 && (
        <div>
          <div className="mb-3 flex items-center justify-between">
            <p className="text-sm text-slate-500 dark:text-slate-400">
              Tìm thấy {items.length} kết quả
            </p>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setSelectedIds(new Set(items.map((i) => i.id)))}
              >
                Chọn tất cả
              </Button>
              <Button
                size="sm"
                onClick={() => doApprove(Array.from(selectedIds))}
                disabled={selectedIds.size === 0}
              >
                Chọn ({selectedIds.size})
              </Button>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5">
            {items.map((item) => (
              <CheckableThumbnailCard
                key={item.id}
                imageUrl={item.thumbnailImageUrl}
                title={item.videoTitle}
                selected={selectedIds.has(item.id)}
                onToggle={() => {
                  setSelectedIds((prev) => {
                    const next = new Set(prev)
                    if (next.has(item.id)) next.delete(item.id)
                    else next.add(item.id)
                    return next
                  })
                }}
                badge={item.platform === 1 ? 'YouTube' : 'Faceless'}
                meta={`${item.viewCount ? `${(item.viewCount / 1000).toFixed(0)}K lượt xem` : ''}${item.publishedTime ? ` · ${new Date(item.publishedTime).toLocaleDateString('vi-VN')}` : ''}`}
              />
            ))}
          </div>
        </div>
      )}

      {!isSearching && items.length === 0 && (
        <div className="flex items-center justify-center rounded-xl border border-dashed border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 p-12 text-sm text-slate-400 dark:text-slate-500">
          Tìm kiếm thumbnail để bắt đầu
        </div>
      )}
    </div>
  )
}
