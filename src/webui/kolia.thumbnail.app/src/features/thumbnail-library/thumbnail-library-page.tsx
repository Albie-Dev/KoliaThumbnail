import { useState, useCallback } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ExternalLink, Loader2 } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '../../components/ui/button'
import { Input } from '../../components/ui/input'
import { Badge } from '../../components/ui/badge'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { TagInput } from '../../components/ui/tag-input'

import { useActiveProjectId, EmptyProjectState } from '../../lib/project-context'
import { useStepGuard } from '../../lib/use-step-guard'
import {
  getThumbnailLibrary,
  importThumbnailManual,
  updateThumbnailStatus,
  chooseThumbnailForGeneration,
  deepAnalyzeThumbnail,
} from './api'
import { qk } from '../../lib/query-keys'
import {
  CLibraryUserStatus,
  LIBRARY_USER_STATUS_OPTIONS,
} from '../../types/enums/pipeline.enums'



export function ThumbnailLibraryPage() {
  const [activeProjectId] = useActiveProjectId()
  useStepGuard('/pipeline/reference/library')
  const queryClient = useQueryClient()

  // Tabs
  const [tab, setTab] = useState<'library' | 'analysis'>('library')

  // Import state
  const [videoUrl, setVideoUrl] = useState('')
  const [keywordTags, setKeywordTags] = useState<string[]>([])

  // Selection for actions
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())

  // Analysis preview
  const [selectedForPreview, setSelectedForPreview] = useState<string | null>(null)

  // Fetch library
  const { data: libraryItems, isLoading } = useQuery({
    queryKey: activeProjectId ? qk.thumbnailLibrary.list(activeProjectId) : ['lib', 'empty'],
    queryFn: () => getThumbnailLibrary(activeProjectId!),
    enabled: !!activeProjectId,
  })

  const items = libraryItems ?? []

  // Import mutation
  const { mutate: doImport, isPending: isImporting } = useMutation({
    mutationFn: () => importThumbnailManual(activeProjectId!, videoUrl),
    onSuccess: () => {
      toast.success('Đã thêm link thumbnail!')
      setVideoUrl('')
      queryClient.invalidateQueries({ queryKey: qk.thumbnailLibrary.list(activeProjectId!) })
    },
  })

  // Status update
  const { mutate: doStatusUpdate } = useMutation({
    mutationFn: ({ id, status }: { id: string; status: CLibraryUserStatus }) =>
      updateThumbnailStatus(activeProjectId!, id, status),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: qk.thumbnailLibrary.list(activeProjectId!) }),
  })

  // Choose for generation
  const { mutate: doChoose } = useMutation({
    mutationFn: ({ id, chosen }: { id: string; chosen: boolean }) =>
      chooseThumbnailForGeneration(activeProjectId!, id, chosen),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: qk.thumbnailLibrary.list(activeProjectId!) }),
  })

  // Deep analyze
  const { mutate: doAnalyzeSelected, isPending: isAnalyzing } = useMutation({
    mutationFn: async (ids: string[]) => {
      for (const id of ids) {
        await deepAnalyzeThumbnail(activeProjectId!, id)
      }
    },
    onSuccess: (_, ids) => {
      toast.success(`Đã phân tích ${ids.length} thumbnail`)
      queryClient.invalidateQueries({ queryKey: qk.thumbnailLibrary.list(activeProjectId!) })
      setTab('analysis')
    },
  })

  // Batch actions
  const batchAction = useCallback(
    (action: 'approve' | 'reject' | 'unchoose') => {
      selectedIds.forEach((id) => {
        if (action === 'approve') doStatusUpdate({ id, status: CLibraryUserStatus.Approved })
        else if (action === 'reject') doStatusUpdate({ id, status: CLibraryUserStatus.Rejected })
        else doChoose({ id, chosen: false })
      })
      setSelectedIds(new Set())
    },
    [selectedIds, doStatusUpdate, doChoose],
  )

  if (!activeProjectId) return <EmptyProjectState />

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      <h1 className="text-xl font-bold text-slate-900 dark:text-slate-100">3.1 Thumbnail library</h1>

      {/* Tabs */}
      <div className="flex gap-1 rounded-lg bg-slate-100 dark:bg-slate-800 p-1 w-fit">
        <button
          type="button"
          onClick={() => setTab('library')}
          className={`rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
            tab === 'library'
              ? 'bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 shadow-sm'
              : 'text-slate-500 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-200'
          }`}
        >
          Kho thumbnail
        </button>
        <button
          type="button"
          onClick={() => setTab('analysis')}
          className={`rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
            tab === 'analysis'
              ? 'bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 shadow-sm'
              : 'text-slate-500 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-200'
          }`}
        >
          Phân tích thumbnail, title & chọn mẫu
        </button>
      </div>

      {tab === 'library' && (
        <>
          {/* Import section */}
          <div className="flex items-end gap-3 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4">
            <div className="flex-1">
              <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">Nhập link YouTube</label>
              <Input value={videoUrl} onChange={(e) => setVideoUrl(e.target.value)} placeholder="https://youtube.com/..." />
            </div>
            <div className="flex-1">
              <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">Keyword</label>
              <TagInput tags={keywordTags} onChange={setKeywordTags} placeholder="Thêm tag..." />
            </div>
            <Button onClick={() => doImport()} disabled={isImporting || !videoUrl.trim()}>
              {isImporting ? 'Đang thêm…' : 'Thêm link'}
            </Button>
          </div>

          {/* Batch actions */}
          {selectedIds.size > 0 && (
            <div className="flex items-center gap-2 rounded-lg bg-slate-50 dark:bg-slate-800 px-4 py-2">
              <span className="text-sm text-slate-600 dark:text-slate-300">Đã chọn {selectedIds.size}:</span>
              <Button variant="outline" size="sm" onClick={() => batchAction('approve')}>Đánh dấu phù hợp</Button>
              <Button variant="outline" size="sm" onClick={() => batchAction('reject')}>Đánh dấu không phù hợp</Button>
              <Button variant="outline" size="sm" onClick={() => batchAction('unchoose')}>Bỏ khỏi phân tích</Button>
              <Button variant="outline" size="sm" onClick={() => doAnalyzeSelected(Array.from(selectedIds))} disabled={isAnalyzing}>
                {isAnalyzing ? 'Đang phân tích…' : 'Phân tích đã chọn'}
              </Button>
            </div>
          )}

          {/* Table */}
          <div className="overflow-x-auto rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800">
                  <th className="w-10 px-3 py-2">
                    <input
                      type="checkbox"
                      checked={selectedIds.size === items.length && items.length > 0}
                      onChange={() => {
                        if (selectedIds.size === items.length) setSelectedIds(new Set())
                        else setSelectedIds(new Set(items.map((i) => i.id)))
                      }}
                      className="h-3.5 w-3.5"
                    />
                  </th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500">Thumbnail</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500">Keyword</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500">AI gợi ý</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500">Trạng thái</th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500">Thông tin</th>
                </tr>
              </thead>
              <tbody>
                {items.map((item) => (
                  <tr key={item.id} className="border-b border-slate-100 dark:border-slate-800 hover:bg-slate-50 dark:hover:bg-slate-800/50">
                    <td className="px-3 py-2">
                      <input
                        type="checkbox"
                        checked={selectedIds.has(item.id)}
                        onChange={() => {
                          setSelectedIds((prev) => {
                            const next = new Set(prev)
                            if (next.has(item.id)) next.delete(item.id)
                            else next.add(item.id)
                            return next
                          })
                        }}
                        className="h-3.5 w-3.5"
                      />
                    </td>
                    <td className="px-3 py-2">
                      <div className="flex items-center gap-2">
                        <img src={item.thumbnailImageUrl} alt="" className="h-10 w-16 rounded object-cover" />
                        <div>
                          <a href={item.videoUrl} target="_blank" rel="noopener noreferrer" className="text-xs font-medium text-slate-700 dark:text-slate-200 hover:text-blue-600 inline-flex items-center gap-1">
                            {item.videoTitle} <ExternalLink className="h-3 w-3" />
                          </a>
                          {item.channelName && <p className="text-[10px] text-slate-400">{item.channelName}</p>}
                        </div>
                      </div>
                    </td>
                    <td className="px-3 py-2">
                      {item.keywordBatchTag && (
                        <span className="inline-flex items-center rounded-full bg-indigo-50 dark:bg-indigo-950/40 px-2 py-0.5 text-[10px] text-indigo-700 dark:text-indigo-300">
                          {item.keywordBatchTag}
                        </span>
                      )}
                    </td>
                    <td className="px-3 py-2">
                      {item.hasAnalysis ? (
                        <Badge variant="success" dot>Đã phân tích</Badge>
                      ) : (
                        <Badge variant="secondary">Chưa phân tích</Badge>
                      )}
                    </td>
                    <td className="px-3 py-2">
                      <SelectDropdown<{ id: number; label: string }>
                        items={LIBRARY_USER_STATUS_OPTIONS}
                        getOptionId={(o) => String(o.id)}
                        getOptionLabel={(o) => o.label}
                        value={LIBRARY_USER_STATUS_OPTIONS.find((o) => o.id === item.userStatus) ?? null}
                        onChange={(o) => {
                          if (o) doStatusUpdate({ id: item.id, status: o.id as CLibraryUserStatus })
                        }}
                        placeholder="Trạng thái"
                      />
                    </td>
                    <td className="whitespace-nowrap px-3 py-2 text-xs text-slate-500">
                      {item.viewCount && <span>{Math.round(item.viewCount / 1000)}K lượt xem</span>}
                      {item.publishedTime && <span> · {new Date(item.publishedTime).toLocaleDateString('vi-VN')}</span>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {items.length === 0 && !isLoading && (
              <div className="p-8 text-center text-sm text-slate-400">Chưa có thumbnail nào trong thư viện</div>
            )}
            {isLoading && (
              <div className="flex justify-center p-8"><Loader2 className="h-5 w-5 animate-spin text-slate-400" /></div>
            )}
          </div>
        </>
      )}

      {tab === 'analysis' && (
        <div className="space-y-4">
          {/* Summary */}
          <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4">
            <p className="text-sm text-slate-600 dark:text-slate-300">
              <strong className="text-slate-900 dark:text-slate-100">{items.filter((i) => i.hasAnalysis).length}</strong> thumbnail đã phân tích
            </p>
          </div>

          {/* Two column layout */}
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
            {/* Left: analyzed items */}
            <div className="space-y-2">
              {items.filter((i) => i.hasAnalysis).map((item) => (
                <button
                  key={item.id}
                  type="button"
                  onClick={() => setSelectedForPreview(item.id)}
                  className={`w-full rounded-lg border p-3 text-left transition-colors ${
                    selectedForPreview === item.id
                      ? 'border-slate-900 dark:border-slate-100 bg-slate-50 dark:bg-slate-800'
                      : 'border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 hover:bg-slate-50 dark:hover:bg-slate-800'
                  }`}
                >
                  <div className="flex items-center gap-3">
                    <img src={item.thumbnailImageUrl} alt="" className="h-12 w-20 rounded object-cover" />
                    <div className="flex-1 min-w-0">
                      <p className="text-xs font-medium text-slate-700 dark:text-slate-200 truncate">{item.videoTitle}</p>
                      <input
                        type="checkbox"
                        checked={false}
                        onChange={() => {}}
                        className="mt-1 h-3 w-3"
                        onClick={(e) => e.stopPropagation()}
                      />
                    </div>
                  </div>
                </button>
              ))}
              {items.filter((i) => i.hasAnalysis).length === 0 && (
                <div className="p-8 text-center text-sm text-slate-400">Chưa có thumbnail nào được phân tích</div>
              )}
            </div>

            {/* Right: analysis detail */}
            <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4">
              {selectedForPreview ? (
                <div className="space-y-3">
                  <img
                    src={items.find((i) => i.id === selectedForPreview)?.thumbnailImageUrl ?? ''}
                    alt=""
                    className="w-full rounded-lg object-cover"
                  />
                  <p className="text-sm font-medium text-slate-700 dark:text-slate-200">
                    Đã chọn mẫu để phân tích
                  </p>
                </div>
              ) : (
                <div className="flex items-center justify-center h-48 text-sm text-slate-400">
                  Chọn một thumbnail để xem phân tích
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
