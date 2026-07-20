import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { Loader2, Download, Copy, Check, Send } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '../../components/ui/button'
import { Input } from '../../components/ui/input'
import { Textarea } from '../../components/ui/textarea'
import { DialogProvider, DialogContent, DialogHeader, DialogTitle } from '../../components/ui/dialog'
import { ImageCompareSlider } from '../../components/ui/image-compare-slider'
import { MultiGenerationResultGrid, type GenerationSet } from '../../components/ui/multi-generation-result-grid'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { useActiveProjectId, EmptyProjectState } from '../../lib/project-context'
import { useStepGuard } from '../../lib/use-step-guard'
import { getThumbnailSets, generateThumbnail, exportThumbnailPrompt, editThumbnail, approveThumbnail, pushThumbnailToTitle, markThumbnailDownloaded } from './api'
import { getDisplayTexts } from '../display-text/api'
import { getThumbnailLibrary } from '../thumbnail-library/api'
import { getCharacters } from '../characters/api'
import { qk } from '../../lib/query-keys'
import { CThumbnailEditTool, THUMBNAIL_EDIT_TOOL_OPTIONS } from '../../types/enums/pipeline.enums'

export function ThumbnailGenerationPage() {
  const [activeProjectId] = useActiveProjectId()
  useStepGuard('/pipeline/thumbnail/generate')
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  // Luôn mang projectId khi navigate sang pipeline khác
  const navigateWithProject = (path: string) => {
    navigate(path + '?projectId=' + encodeURIComponent(activeProjectId!))
  }

  // ── Form state ────────────────────────────────────────────────────────
  const [changesRequestText, setChangesRequestText] = useState('')
  const [ratio, setRatio] = useState('16:9')
  const [resolution, setResolution] = useState('1920x1080')
  const [requestedCount, setRequestedCount] = useState(4)
  const [selectedCharacterId, setSelectedCharacterId] = useState<string | null>(null)

  // ── Display text selection ────────────────────────────────────────────
  const { data: displayTexts } = useQuery({
    queryKey: activeProjectId ? qk.displayTexts(activeProjectId) : ['dt', 'empty'],
    queryFn: () => getDisplayTexts(activeProjectId!),
    enabled: !!activeProjectId,
  })
  const selectedDisplayTextOptions = (displayTexts ?? []).flatMap((r) =>
    r.options.filter((o) => o.isSelected),
  )

  // ── Reference library ────────────────────────────────────────────────
  const { data: libraryItems } = useQuery({
    queryKey: activeProjectId ? qk.thumbnailLibrary.list(activeProjectId) : ['lib', 'empty'],
    queryFn: () => getThumbnailLibrary(activeProjectId!),
    enabled: !!activeProjectId,
  })
  const chosenItems = (libraryItems ?? []).filter((i) => i.hasAnalysis)

  // ── Characters ────────────────────────────────────────────────────────
  const { data: characters } = useQuery({
    queryKey: qk.characters.list(),
    queryFn: () => getCharacters(),
  })

  // ── Generated sets ────────────────────────────────────────────────────
  const { data: thumbnailSets } = useQuery({
    queryKey: activeProjectId ? qk.thumbnailGeneration(activeProjectId) : ['tg', 'empty'],
    queryFn: () => getThumbnailSets(activeProjectId!),
    enabled: !!activeProjectId,
  })

  // ── Before/After state ───────────────────────────────────────────────
  const [selectedVariantId, setSelectedVariantId] = useState<string | null>(null)
  const [afterVariantUrl, setAfterVariantUrl] = useState<string | null>(null)
  const [editTool, setEditTool] = useState<number>(CThumbnailEditTool.Image)
  const [editRequestText, setEditRequestText] = useState('')

  // ── Tick for push-to-title ───────────────────────────────────────────
  const [tickedForTitle, setTickedForTitle] = useState<Set<string>>(new Set())

  // ── Prompt dialog ────────────────────────────────────────────────────
  const [promptDialogOpen, setPromptDialogOpen] = useState(false)
  const [exportedPrompt, setExportedPrompt] = useState('')
  const [copied, setCopied] = useState(false)

  // ── Generate ─────────────────────────────────────────────────────────
  const { mutate: doGenerate, isPending: isGenerating } = useMutation({
    mutationFn: () =>
      generateThumbnail(activeProjectId!, {
        displayTextOptionIds: selectedDisplayTextOptions.map((o) => o.id),
        referenceLibraryItemIds: chosenItems.map((i) => i.id),
        characterId: selectedCharacterId ?? undefined,
        changesRequestText,
        ratio,
        resolution,
        requestedCount,
      }),
    onSuccess: () => {
      toast.success('Đã tạo thumbnail!')
      queryClient.invalidateQueries({ queryKey: qk.thumbnailGeneration(activeProjectId!) })
    },
  })

  // ── Export prompt ────────────────────────────────────────────────────
  const { mutate: doExportPrompt, isPending: isExporting } = useMutation({
    mutationFn: async () => {
      const result = await exportThumbnailPrompt(activeProjectId!, {
        displayTextOptionIds: selectedDisplayTextOptions.map((o) => o.id),
        referenceLibraryItemIds: chosenItems.map((i) => i.id),
        characterId: selectedCharacterId ?? undefined,
        changesRequestText,
        ratio,
        resolution,
      })
      return result
    },
    onSuccess: (prompt) => {
      setExportedPrompt(prompt)
      setPromptDialogOpen(true)
    },
  })

  // ── Edit ─────────────────────────────────────────────────────────────
  const { mutate: doEdit, isPending: isEditing } = useMutation({
    mutationFn: () =>
      editThumbnail(activeProjectId!, selectedVariantId!, {
        editTool: editTool as CThumbnailEditTool,
        editRequestText,
      }),
    onSuccess: (result) => {
      setAfterVariantUrl(result.imageUrl)
      toast.success('Đã áp dụng chỉnh sửa!')
      queryClient.invalidateQueries({ queryKey: qk.thumbnailGeneration(activeProjectId!) })
    },
  })

  // ── Push to title ────────────────────────────────────────────────────
  const { mutate: doPushToTitle, isPending: isPushing } = useMutation({
    mutationFn: async () => {
      for (const id of Array.from(tickedForTitle)) {
        await pushThumbnailToTitle(activeProjectId!, id)
      }
    },
    onSuccess: () => {
      toast.success(`Đã đẩy ${tickedForTitle.size} thumbnail sang Title!`)
      queryClient.invalidateQueries({ queryKey: qk.thumbnailGeneration(activeProjectId!) })
      navigateWithProject('/pipeline/video-title')
    },
  })

  // ── Build generation sets for grid ───────────────────────────────────
  const allVariants = (thumbnailSets ?? []).flatMap((set) => set.variants)
  const generationSets: GenerationSet[] = (thumbnailSets ?? []).map((set) => ({
    id: set.id,
    label: `Bộ ${set.setIndex + 1}`,
    options: set.variants.map((v) => ({
      id: v.id,
      imageUrl: v.imageUrl,
      label: `#${v.variantIndex + 1}${v.versionNumber > 1 ? ` v${v.versionNumber}` : ''}`,
      selected: tickedForTitle.has(v.id),
      onToggle: () => {
        setTickedForTitle((prev) => {
          const next = new Set(prev)
          if (next.has(v.id)) next.delete(v.id)
          else next.add(v.id)
          return next
        })
      },
    })),
  }))

  const selectedVariant = selectedVariantId
    ? allVariants.find((v) => v.id === selectedVariantId)
    : null

  const beforeUrl = selectedVariant?.imageUrl ?? ''

  if (!activeProjectId) return <EmptyProjectState />

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      <h1 className="text-xl font-bold text-slate-900 dark:text-slate-100">4.2 Tạo thumbnail</h1>

      {/* Block 1: Settings */}
      <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4 space-y-4">
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          {/* Display text selection */}
          <div>
            <h3 className="mb-2 text-xs font-semibold text-slate-600 dark:text-slate-300">Display Text đã chọn</h3>
            {selectedDisplayTextOptions.length === 0 ? (
              <p className="text-xs text-slate-400">Chưa có display text nào được chọn</p>
            ) : (
              <div className="space-y-1">
                {selectedDisplayTextOptions.map((opt) => (
                  <label key={opt.id} className="flex items-center gap-2 text-xs text-slate-600 dark:text-slate-300">
                    <input type="checkbox" checked={true} readOnly className="h-3 w-3" />
                    {opt.content}
                  </label>
                ))}
              </div>
            )}
          </div>

          {/* Character selection */}
          <div>
            <h3 className="mb-2 text-xs font-semibold text-slate-600 dark:text-slate-300">Nhân vật</h3>
            <SelectDropdown<{ id: string; name: string }>
              items={(characters ?? []).map((c) => ({ id: c.id, name: c.name }))}
              getOptionId={(o) => o.id}
              getOptionLabel={(o) => o.name}
              value={characters?.find((c) => c.id === selectedCharacterId) ? { id: selectedCharacterId!, name: '' } : null}
              onChange={(o) => setSelectedCharacterId(o?.id ?? null)}
              placeholder="Chọn nhân vật..."
              allowSearch
            />
          </div>
        </div>

        {/* Settings row */}
        <div className="grid grid-cols-1 gap-4 md:grid-cols-4">
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">Tỷ lệ</label>
            <Input value={ratio} onChange={(e) => setRatio(e.target.value)} placeholder="16:9" />
          </div>
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">Độ phân giải</label>
            <Input value={resolution} onChange={(e) => setResolution(e.target.value)} placeholder="1920x1080" />
          </div>
          <div>
            <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">Số ảnh</label>
            <Input type="number" min={1} max={10} value={requestedCount} onChange={(e) => setRequestedCount(Number(e.target.value))} />
          </div>
          <div className="flex items-end gap-2">
            <Button variant="outline" size="sm" onClick={() => doExportPrompt()} disabled={isExporting} className="flex-1">
              {isExporting ? <Loader2 className="h-3.5 w-3.5 animate-spin" /> : null}
              Xuất prompt
            </Button>
            <Button size="sm" onClick={() => doGenerate()} disabled={isGenerating || selectedDisplayTextOptions.length === 0} className="flex-1">
              {isGenerating ? <Loader2 className="h-3.5 w-3.5 animate-spin" /> : null}
              Tạo
            </Button>
          </div>
        </div>

        {/* Changes request */}
        <div>
          <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-300">Muốn thay đổi gì?</label>
          <Textarea value={changesRequestText} onChange={(e) => setChangesRequestText(e.target.value)} placeholder="Mô tả yêu cầu thay đổi..." />
        </div>
      </div>

      {/* Block 2: Results grid */}
      <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4">
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-sm font-semibold text-slate-700 dark:text-slate-200">Kết quả ({allVariants.length})</h2>
          <Button
            size="sm"
            onClick={() => doPushToTitle()}
            disabled={isPushing || tickedForTitle.size === 0}
          >
            {isPushing ? 'Đang đẩy…' : `Đẩy ${tickedForTitle.size} thumbnail sang Title →`}
          </Button>
        </div>
        <MultiGenerationResultGrid sets={generationSets} />
      </div>

      {/* Block 3: Before/After */}
      <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4">
        <h2 className="mb-3 text-sm font-semibold text-slate-700 dark:text-slate-200">
          Chọn thumbnail tốt để tạo Video Title
        </h2>

        {/* Variant selection */}
        <div className="mb-4 flex flex-wrap gap-2">
          {allVariants.filter((v) => tickedForTitle.has(v.id)).map((v) => (
            <button
              key={v.id}
              type="button"
              onClick={() => {
                setSelectedVariantId(v.id)
                setAfterVariantUrl(null)
              }}
              className={`rounded-lg border-2 p-1 transition-all ${
                selectedVariantId === v.id
                  ? 'border-slate-900 dark:border-slate-100'
                  : 'border-transparent hover:border-slate-300 dark:hover:border-slate-600'
              }`}
            >
              <img src={v.imageUrl} alt="" className="h-16 w-28 rounded object-cover" />
            </button>
          ))}
          {allVariants.filter((v) => tickedForTitle.has(v.id)).length === 0 && (
            <p className="text-xs text-slate-400">Tick chọn thumbnail ở khối trên để chỉnh sửa</p>
          )}
        </div>

        {/* Before/After slider */}
        {selectedVariant && (
          <div className="space-y-4">
            <ImageCompareSlider
              beforeSrc={afterVariantUrl || beforeUrl}
              afterSrc={beforeUrl}
              beforeLabel={afterVariantUrl ? 'After' : 'Before'}
              afterLabel="Before"
            />

            {/* Edit tools */}
            <div className="flex gap-2">
              {THUMBNAIL_EDIT_TOOL_OPTIONS.map((tool) => (
                <Button
                  key={tool.id}
                  variant={editTool === tool.id ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => setEditTool(tool.id)}
                >
                  {tool.label}
                </Button>
              ))}
            </div>

            <div className="flex gap-2">
              <Textarea
                value={editRequestText}
                onChange={(e) => setEditRequestText(e.target.value)}
                placeholder="Mô tả yêu cầu chỉnh sửa..."
                className="flex-1"
              />
              <Button onClick={() => doEdit()} disabled={isEditing || !editRequestText.trim()}>
                {isEditing ? <Loader2 className="h-4 w-4 animate-spin" /> : <Send className="h-4 w-4" />}
                Áp dụng
              </Button>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => {
                  window.open(beforeUrl, '_blank')
                  markThumbnailDownloaded(activeProjectId!, selectedVariant.id)
                }}
              >
                <Download className="h-3.5 w-3.5" />
                Download ảnh
              </Button>
              <span className="text-xs text-slate-400">
                <label className="inline-flex items-center gap-1 cursor-pointer">
                  <input type="radio" name="before-after" checked={!afterVariantUrl} onChange={() => setAfterVariantUrl(null)} />
                  Before
                </label>
                <label className="inline-flex items-center gap-1 cursor-pointer ml-3">
                  <input
                    type="radio"
                    name="before-after"
                    checked={!!afterVariantUrl}
                    onChange={() => {
                      if (afterVariantUrl) {
                        approveThumbnail(activeProjectId!, selectedVariant.id)
                      }
                    }}
                    disabled={!afterVariantUrl}
                  />
                  After
                </label>
              </span>
            </div>
          </div>
        )}
      </div>

      {/* Prompt dialog */}
      <DialogProvider open={promptDialogOpen} setOpen={setPromptDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Prompt đã xuất</DialogTitle>
          </DialogHeader>
          <div className="space-y-3">
            <textarea
              readOnly
              value={exportedPrompt}
              className="min-h-[200px] w-full rounded-md border border-slate-300 dark:border-slate-600 bg-slate-900 p-3 text-xs font-mono text-slate-100"
            />
            <Button
              onClick={() => {
                navigator.clipboard.writeText(exportedPrompt)
                setCopied(true)
                setTimeout(() => setCopied(false), 2000)
              }}
              className="w-full"
            >
              {copied ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
              {copied ? 'Đã copy' : 'Copy prompt'}
            </Button>
          </div>
        </DialogContent>
      </DialogProvider>
    </div>
  )
}
