import { useState, useMemo } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm, useWatch } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { Button } from '../../components/ui/button'
import { Textarea } from '../../components/ui/textarea'
import { Badge } from '../../components/ui/badge'
import { DialogProvider, DialogContent, DialogHeader, DialogTitle } from '../../components/ui/dialog'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { useActiveProjectId, EmptyProjectState } from '../../lib/project-context'
import { useStepGuard } from '../../lib/use-step-guard'
import { getBrief, saveManualBrief, importBrief, analyzeBrief, confirmBrief } from './api'
import { saveManualBriefSchema, type SaveManualBriefInput } from './schema'
import { qk } from '../../lib/query-keys'
import { CImportContentSource, IMPORT_CONTENT_SOURCE_OPTIONS } from '../../types/enums/pipeline.enums'

/**
 * Xây dựng prompt tự động từ dữ liệu đầu vào (mirror BE: AiBriefAnalysisEngine.BuildAutoPrompt).
 * Bao gồm system instructions (vai trò AI + format JSON) và dữ liệu người dùng,
 * giúp người dùng thấy được toàn bộ nội dung AI sẽ nhận được trước khi chỉnh sửa.
 * Dùng để hiển thị prompt xem trước cho người dùng chỉnh sửa.
 */
function buildAutoPrompt(overview: string, viewpoint: string, keyData: string): string {
  const systemInstruction = `Bạn là chuyên gia phân tích nội dung livestream/video.
Phân tích các thông tin đầu vào sau và trả về JSON đúng cấu trúc:
{
  "topic": "Chủ đề chính của video",
  "mainMessage": "Thông điệp chính cần truyền tải",
  "highlightData": "Các dữ liệu/số liệu nổi bật cần nhấn mạnh",
  "suggestedKeywords": ["từ khóa 1", "từ khóa 2", ...]
}`

  const dataSections = [
    '## Tổng quan',
    overview,
    '',
    '## Quan điểm muốn nhấn mạnh',
    viewpoint,
    '',
    '## Dữ liệu quan trọng',
    keyData,
  ].join('\n')

  return `${systemInstruction}\n\n${dataSections}`
}

export function ContentBriefPage() {
  const [activeProjectId] = useActiveProjectId()
  useStepGuard('/pipeline/video-content')
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  // ── Import dialog state ──────────────────────────────────────────────
  const [importDialogOpen, setImportDialogOpen] = useState(false)
  const [importSource, setImportSource] = useState<CImportContentSource>(CImportContentSource.PasteText)
  const [importText, setImportText] = useState('')
  const [importUrl, setImportUrl] = useState('')

  // ── Manual prompt state ──────────────────────────────────────────────
  const [showManualPrompt, setShowManualPrompt] = useState(false)
  const [manualPrompt, setManualPrompt] = useState('')

  // ── Fetch brief data ─────────────────────────────────────────────────
  const { data: brief, isLoading, error, refetch } = useQuery({
    queryKey: activeProjectId ? qk.brief(activeProjectId) : ['brief', 'empty'],
    queryFn: () => getBrief(activeProjectId!),
    enabled: !!activeProjectId,
  })

  // ── Form ──────────────────────────────────────────────────────────────
  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<SaveManualBriefInput>({
    resolver: zodResolver(saveManualBriefSchema),
    values: {
      overviewInput: brief?.overviewInput ?? '',
      viewpointInput: brief?.viewpointInput ?? '',
      keyDataInput: brief?.keyDataInput ?? '',
    },
  })

  // Watch form values để tự động xây dựng prompt xem trước
  const watchedValues = useWatch({ control })
  const autoPrompt = useMemo(
    () => buildAutoPrompt(watchedValues.overviewInput ?? '', watchedValues.viewpointInput ?? '', watchedValues.keyDataInput ?? ''),
    [watchedValues.overviewInput, watchedValues.viewpointInput, watchedValues.keyDataInput],
  )

  // ── Mutations ────────────────────────────────────────────────────────
  const { mutate: saveAndAnalyze, isPending: isSaving } = useMutation({
    mutationFn: async (data: SaveManualBriefInput) => {
      await saveManualBrief(activeProjectId!, data)
      const prompt = showManualPrompt && manualPrompt.trim() ? manualPrompt : undefined
      return analyzeBrief(activeProjectId!, prompt)
    },
    onSuccess: () => {
      toast.success('Đã tạo bản tóm tắt thành công!')
      queryClient.invalidateQueries({ queryKey: qk.brief(activeProjectId!) })
      // Reset manual prompt state sau khi tạo thành công
      setShowManualPrompt(false)
      setManualPrompt('')
    },
    onError: () => {
      // Refresh UI để hiển thị 3 field đã lưu (nếu saveManualBrief đã thành công trước đó)
      queryClient.invalidateQueries({ queryKey: qk.brief(activeProjectId!) })
    },
  })

  const { mutate: doImport, isPending: isImporting } = useMutation({
    mutationFn: () =>
      importBrief(activeProjectId!, {
        source: importSource,
        rawText: importSource === CImportContentSource.PasteText ? importText : undefined,
        fileUrl: importSource === CImportContentSource.File ? importUrl : undefined,
        externalLink: importSource === CImportContentSource.ExternalLink ? importUrl : undefined,
      }),
    onSuccess: () => {
      toast.success('Đã nhập thông tin thành công!')
      setImportDialogOpen(false)
      setImportText('')
      setImportUrl('')
      queryClient.invalidateQueries({ queryKey: qk.brief(activeProjectId!) })
    },
    onError: () => {
      queryClient.invalidateQueries({ queryKey: qk.brief(activeProjectId!) })
    },
  })

  const { mutate: doConfirm, isPending: isConfirming } = useMutation({
    mutationFn: () => confirmBrief(activeProjectId!),
    onSuccess: async () => {
      toast.success('Đã xác nhận nội dung!')
      // Đợi refetch xong dữ liệu project (cập nhật step mới) trước khi navigate
      // tránh step guard ở trang đích đọc dữ liệu cũ và redirect về dashboard
      await Promise.all([
        queryClient.refetchQueries({ queryKey: qk.brief(activeProjectId!) }),
        queryClient.refetchQueries({ queryKey: qk.projects.detail(activeProjectId!) }),
      ])
      // Tự động chuyển sang bước 2 (Tin tức)
      navigate('/pipeline/news?projectId=' + encodeURIComponent(activeProjectId!))
    },
    onError: () => {
      queryClient.invalidateQueries({ queryKey: qk.brief(activeProjectId!) })
    },
  })

  const onSubmit = (data: SaveManualBriefInput) => {
    saveAndAnalyze(data)
  }

  // ── Empty state ──────────────────────────────────────────────────────
  if (!activeProjectId) {
    return <EmptyProjectState />
  }

  // ── Loading state ────────────────────────────────────────────────────
  if (isLoading) {
    return (
      <div className="mx-auto max-w-5xl space-y-4">
        <div className="h-8 w-48 animate-pulse rounded bg-slate-200 dark:bg-slate-700" />
        <div className="grid grid-cols-2 gap-6">
          <div className="space-y-4">
            {Array.from({ length: 3 }).map((_, i) => (
              <div key={i} className="h-32 animate-pulse rounded-lg bg-slate-200 dark:bg-slate-700" />
            ))}
          </div>
          <div className="h-64 animate-pulse rounded-lg bg-slate-200 dark:bg-slate-700" />
        </div>
      </div>
    )
  }

  // ── Error state ──────────────────────────────────────────────────────
  if (error) {
    return (
      <div className="mx-auto max-w-5xl">
        <div className="flex flex-col items-center gap-3 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-12">
          <p className="text-sm text-red-600 dark:text-red-400">
            {error instanceof Error ? error.message : 'Có lỗi xảy ra'}
          </p>
          <Button variant="outline" onClick={() => void refetch()}>Thử lại</Button>
        </div>
      </div>
    )
  }

  const isConfirmed = brief?.isConfirmed ?? false
  const hasOutput = !!brief?.topicOutput

  return (
    <div className="mx-auto max-w-5xl space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-slate-900 dark:text-slate-100">Nội dung video</h1>
        <div className="flex items-center gap-2">
          {isConfirmed && <Badge variant="success">Đã xác nhận</Badge>}
          <Button variant="outline" onClick={() => setImportDialogOpen(true)} disabled={isConfirmed}>
            Nhập thông tin
          </Button>
        </div>
      </div>

      {/* Two-column layout */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Left column — input */}
        <div className="space-y-4">
          <FormGroup>
            <FormLabel htmlFor="overviewInput">Tổng quan livestream tuần</FormLabel>
            <Textarea
              {...register('overviewInput')}
              id="overviewInput"
              placeholder="Nhập tổng quan về nội dung livestream..."
              rows={4}
              disabled={isConfirmed}
            />
            <FormField error={errors.overviewInput?.message} />
          </FormGroup>

          <FormGroup>
            <FormLabel htmlFor="viewpointInput">Quan điểm muốn nhấn mạnh</FormLabel>
            <Textarea
              {...register('viewpointInput')}
              id="viewpointInput"
              placeholder="Nhập quan điểm cần nhấn mạnh..."
              rows={4}
              disabled={isConfirmed}
            />
            <FormField error={errors.viewpointInput?.message} />
          </FormGroup>

          <FormGroup>
            <FormLabel htmlFor="keyDataInput">Dữ liệu quan trọng</FormLabel>
            <Textarea
              {...register('keyDataInput')}
              id="keyDataInput"
              placeholder="Nhập dữ liệu quan trọng..."
              rows={4}
              disabled={isConfirmed}
            />
            <FormField error={errors.keyDataInput?.message} />
          </FormGroup>

          {!isConfirmed && (
            <>
              {/* Manual prompt toggle */}
              <label className="flex items-start gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={showManualPrompt}
                  onChange={(e) => {
                    setShowManualPrompt(e.target.checked)
                    if (e.target.checked && !manualPrompt) {
                      setManualPrompt(autoPrompt)
                    }
                  }}
                  className="mt-1 h-4 w-4 rounded border-slate-300 text-slate-900 focus:ring-slate-900/20 dark:border-slate-600 dark:text-slate-100"
                />
                <span className="text-xs text-slate-500 dark:text-slate-400 leading-5">
                  Hiện prompt gốc — bật để xem và chỉnh sửa prompt trước khi gửi
                </span>
              </label>

              {/* Manual prompt textarea */}
              {showManualPrompt && (
                <div className="space-y-1">
                  <label className="text-xs font-medium text-slate-500 dark:text-slate-400">
                    Prompt (chỉnh sửa nếu cần)
                  </label>
                  <textarea
                    value={manualPrompt}
                    onChange={(e) => setManualPrompt(e.target.value)}
                    rows={8}
                    className="w-full rounded-md border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 p-3 text-xs text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-slate-900/10 dark:focus:ring-slate-100/20 font-mono"
                    placeholder="Prompt sẽ được hiển thị ở đây..."
                  />
                </div>
              )}

              <Button onClick={handleSubmit(onSubmit)} disabled={isSaving} className="w-full">
                {isSaving ? 'Đang tạo…' : 'Tạo bản tóm tắt'}
              </Button>
            </>
          )}
        </div>

        {/* Right column — output */}
        <div className="space-y-4">
          <h2 className="text-sm font-semibold text-slate-700 dark:text-slate-200">
            Bản tóm tắt nội dung
          </h2>

          {!hasOutput && (
            <div className="flex items-center justify-center rounded-lg border border-dashed border-slate-300 dark:border-slate-600 p-8 text-sm text-slate-400 dark:text-slate-500">
              Chưa có bản tóm tắt — hãy nhập thông tin và tạo bản tóm tắt
            </div>
          )}

          {hasOutput && (
            <div className="space-y-4">
              <div className="rounded-lg bg-slate-50 dark:bg-slate-800 p-4">
                <h3 className="mb-1 text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide">
                  Chủ đề chính
                </h3>
                <p className="text-sm text-slate-700 dark:text-slate-200 whitespace-pre-wrap">
                  {brief?.topicOutput}
                </p>
              </div>

              <div className="rounded-lg bg-slate-50 dark:bg-slate-800 p-4">
                <h3 className="mb-1 text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide">
                  Thông điệp chính
                </h3>
                <p className="text-sm text-slate-700 dark:text-slate-200 whitespace-pre-wrap">
                  {brief?.mainMessageOutput}
                </p>
              </div>

              <div className="rounded-lg bg-slate-50 dark:bg-slate-800 p-4">
                <h3 className="mb-1 text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wide">
                  Dữ liệu nổi bật
                </h3>
                <p className="text-sm text-slate-700 dark:text-slate-200 whitespace-pre-wrap">
                  {brief?.highlightDataOutput}
                </p>
              </div>
            </div>
          )}

          {/* Confirm button */}
          {!isConfirmed && (
            <Button
              onClick={() => doConfirm()}
              disabled={!hasOutput || isConfirming}
              className="w-full"
              variant="outline"
            >
              {isConfirming ? 'Đang xác nhận…' : 'Xác nhận'}
            </Button>
          )}
        </div>
      </div>

      {/* Import dialog */}
      <DialogProvider open={importDialogOpen} setOpen={setImportDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Nhập thông tin</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            {/* Source type selector */}
            <div className="flex gap-2">
              {IMPORT_CONTENT_SOURCE_OPTIONS.map((option) => (
                <Button
                  key={option.id}
                  variant={importSource === option.id ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => {
                    setImportSource(option.id)
                    setImportText('')
                    setImportUrl('')
                  }}
                >
                  {option.label}
                </Button>
              ))}
            </div>

            {/* Content input */}
            {importSource === CImportContentSource.PasteText && (
              <FormGroup>
                <FormLabel>Nội dung</FormLabel>
                <textarea
                  value={importText}
                  onChange={(e) => setImportText(e.target.value)}
                  className="min-h-[150px] w-full rounded-md border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 p-3 text-sm text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-slate-900/10 dark:focus:ring-slate-100/20"
                  placeholder="Dán nội dung vào đây..."
                />
              </FormGroup>
            )}

            {(importSource === CImportContentSource.File || importSource === CImportContentSource.ExternalLink) && (
              <FormGroup>
                <FormLabel>URL</FormLabel>
                <input
                  type="text"
                  value={importUrl}
                  onChange={(e) => setImportUrl(e.target.value)}
                  className="w-full rounded-md border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900 p-2 text-sm text-slate-900 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-slate-900/10 dark:focus:ring-slate-100/20"
                  placeholder={
                    importSource === CImportContentSource.File
                      ? 'Nhập URL file...'
                      : 'Nhập link ngoài...'
                  }
                />
              </FormGroup>
            )}

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setImportDialogOpen(false)}>
                Hủy
              </Button>
              <Button
                onClick={() => doImport()}
                disabled={isImporting || (importSource === CImportContentSource.PasteText ? !importText.trim() : !importUrl.trim())}
              >
                {isImporting ? 'Đang nhập…' : 'Nhập'}
              </Button>
            </div>
          </div>
        </DialogContent>
      </DialogProvider>
    </div>
  )
}
