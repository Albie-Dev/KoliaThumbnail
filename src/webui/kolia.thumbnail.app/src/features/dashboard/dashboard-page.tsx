import { useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { Button } from '../../components/ui/button'
import { Badge } from '../../components/ui/badge'
import { useActiveProjectId, EmptyProjectState } from '../../lib/project-context'
import { useSidebarContext } from '../../lib/sidebar-context'
import { getProjectById } from '../projects/api'
import { qk } from '../../lib/query-keys'
import { CProjectStepStatus, STEP_NUMBER_LABELS, STEP_NUMBER_ROUTES, NEXT_STEP_HINTS } from '../../types/enums/pipeline.enums'

export function DashboardPage() {
  const [activeProjectId] = useActiveProjectId()
  const navigate = useNavigate()
  const { open } = useSidebarContext()

  const { data: project, isLoading, error, refetch } = useQuery({
    queryKey: activeProjectId ? qk.projects.detail(activeProjectId) : ['projects', 'empty'],
    queryFn: () => getProjectById(activeProjectId!),
    enabled: !!activeProjectId,
  })

  const progressPercent = useMemo(() => {
    if (!project?.steps) return 0
    const completed = project.steps.filter((s) => s.stepStatus === CProjectStepStatus.Completed).length
    return Math.round((completed / 5) * 100)
  }, [project])

  const currentStep = useMemo(() => {
    if (!project?.steps) return null
    return project.steps.find((s) => s.stepNumber === project.currentStepNumber) ?? null
  }, [project])

  const nextStep = useMemo(() => {
    if (!project?.steps) return null
    const nextNumber = (project.currentStepNumber ?? 0) + 1
    return project.steps.find((s) => s.stepNumber === nextNumber) ?? null
  }, [project])

  const modules = useMemo(() => {
    if (!project?.steps) return []
    const currentNum = project.currentStepNumber

    return Array.from({ length: 5 }, (_, i) => {
      const stepNumber = i + 1
      const step = project.steps.find((s) => s.stepNumber === stepNumber)

      let status: 'complete' | 'active' | 'review' | 'waiting' = 'waiting'
      if (step?.stepStatus === CProjectStepStatus.Completed) {
        status = 'complete'
      } else if (step?.needsApproval && stepNumber === currentNum) {
        status = 'review'
      } else if (step?.stepStatus === CProjectStepStatus.InProgress || stepNumber === currentNum) {
        status = 'active'
      }

      const outputText = step?.outputSummaryText
        ?? (stepNumber === 1 ? 'Chưa tạo Content Brief' : 'Chưa có dữ liệu')

      return { stepNumber, status, name: step?.stepName ?? STEP_NUMBER_LABELS[stepNumber] ?? `Bước ${stepNumber}`, outputText }
    })
  }, [project])

  // ── Empty state ───────────────────────────────────────────────────────
  if (!activeProjectId) {
    return <EmptyProjectState />
  }

  // ── Loading state ─────────────────────────────────────────────────────
  if (isLoading) {
    return (
      <div className="mx-auto max-w-7xl space-y-6">
        <div className="h-8 w-64 animate-pulse rounded bg-slate-200 dark:bg-slate-700" />
        <div className="h-2 w-full animate-pulse rounded-full bg-slate-200 dark:bg-slate-700" />
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-5">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="h-40 animate-pulse rounded-xl bg-slate-200 dark:bg-slate-700" />
          ))}
        </div>
      </div>
    )
  }

  // ── Error state ───────────────────────────────────────────────────────
  if (error || !project) {
    return (
      <div className="mx-auto max-w-7xl">
        <div className="flex flex-col items-center justify-center gap-3 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-12">
          <p className="text-sm text-red-600 dark:text-red-400">
            {error instanceof Error ? error.message : 'Không tìm thấy project'}
          </p>
          <Button variant="outline" onClick={() => void refetch()}>
            Thử lại
          </Button>
        </div>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      {/* Dashboard điều hành */}
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-xs font-bold text-blue-500 dark:text-blue-400 uppercase tracking-widest mb-1">
            Dashboard điều hành
          </p>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">
            {project.name}
          </h1>
        </div>
        <Button onClick={() => open({ type: 'create-project' })} className="shrink-0">
          <Plus className="h-4 w-4" />
          Tạo project mới
        </Button>
      </div>

      {/* Two-column layout */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-[1.8fr_0.72fr]">
        {/* Left — Tiến trình 5 phần */}
        <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-5">
          <div className="flex items-start justify-between gap-4 mb-4">
            <div>
              <h2 className="text-base font-semibold text-slate-900 dark:text-slate-100">Tiến trình 5 phần</h2>
              <p className="text-xs text-slate-500 dark:text-slate-400">Bấm vào từng phần để mở đúng trang đang cần làm.</p>
            </div>
            <span className="inline-flex shrink-0 items-center gap-1.5 rounded-md bg-indigo-50 dark:bg-indigo-950/40 px-2.5 py-1 text-[10px] font-bold text-indigo-700 dark:text-indigo-300 leading-none">
              {currentStep ? `Hiện tại: P${currentStep.stepNumber}` : 'Đã hoàn tất'}
            </span>
          </div>

          {/* Progress */}
          <div className="flex items-center justify-between gap-3 mb-2 text-xs">
            <span className="font-semibold text-slate-700 dark:text-slate-200">
              {modules.filter(m => m.status === 'complete').length}/5 phần hoàn thành
            </span>
            <span className="font-bold text-slate-500 dark:text-slate-400">{progressPercent}%</span>
          </div>
          <div className="h-1.5 rounded-full bg-slate-200 dark:bg-slate-700 overflow-hidden mb-5">
            <div
              className="h-full rounded-full bg-gradient-to-r from-blue-500 to-teal-400 transition-all duration-300"
              style={{ width: `${progressPercent}%` }}
            />
          </div>

          {/* Module grid — 5 cards, responsive */}
          <div className="grid grid-cols-2 gap-2.5 sm:grid-cols-3 md:grid-cols-5">
            {modules.map((mod) => {
              const statusStyles: Record<string, { bg: string; border: string; numBg: string; text: string; statusLabel: string; statusColor: string }> = {
                complete: {
                  bg: 'bg-emerald-50 dark:bg-emerald-950/20',
                  border: 'border-emerald-300 dark:border-emerald-800',
                  numBg: 'bg-emerald-600 text-white',
                  text: 'text-slate-700 dark:text-slate-200',
                  statusLabel: 'Hoàn thành',
                  statusColor: 'text-emerald-600 dark:text-emerald-400',
                },
                active: {
                  bg: 'bg-blue-50 dark:bg-blue-950/20',
                  border: 'border-blue-300 dark:border-blue-800',
                  numBg: 'bg-blue-600 text-white',
                  text: 'text-slate-700 dark:text-slate-200',
                  statusLabel: 'Đang làm',
                  statusColor: 'text-blue-500 dark:text-blue-400',
                },
                review: {
                  bg: 'bg-amber-50 dark:bg-amber-950/20',
                  border: 'border-amber-300 dark:border-amber-800',
                  numBg: 'bg-amber-600 text-white',
                  text: 'text-slate-700 dark:text-slate-200',
                  statusLabel: 'Cần duyệt',
                  statusColor: 'text-amber-600 dark:text-amber-400',
                },
                waiting: {
                  bg: 'bg-slate-50 dark:bg-slate-800/30',
                  border: 'border-slate-200 dark:border-slate-700',
                  numBg: 'bg-slate-300 dark:bg-slate-600 text-white',
                  text: 'text-slate-500 dark:text-slate-400',
                  statusLabel: 'Chờ',
                  statusColor: 'text-slate-400 dark:text-slate-500',
                },
              }
              const s = statusStyles[mod.status] ?? statusStyles.waiting

              return (
                <button
                  key={mod.stepNumber}
                  type="button"
                  onClick={() => {
                    const route = STEP_NUMBER_ROUTES[mod.stepNumber as keyof typeof STEP_NUMBER_ROUTES]
                    if (route) navigate(route + '?projectId=' + encodeURIComponent(activeProjectId!))
                  }}
                  className={`flex flex-col gap-2 rounded-xl border p-2.5 text-left transition-all hover:shadow-md ${s.bg} ${s.border} ${s.text}`}
                >
                  <div className="flex items-center justify-between gap-1">
                    <span className={`inline-flex h-6 w-6 items-center justify-center rounded-md text-xs font-bold ${s.numBg}`}>
                      {mod.stepNumber}
                    </span>
                    <span className={`text-[10px] font-bold ${s.statusColor}`}>{s.statusLabel}</span>
                  </div>
                  <span className="text-xs font-semibold leading-tight">{mod.name}</span>
                  <span className="text-[10px] leading-tight text-slate-500 dark:text-slate-400 line-clamp-2">{mod.outputText}</span>
                </button>
              )
            })}
          </div>
        </div>

        {/* Right — Phần đang xử lý */}
        <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-5">
          <div className="flex items-start justify-between gap-4 mb-1">
            <div>
              <h2 className="text-base font-semibold text-slate-900 dark:text-slate-100">Phần đang xử lý</h2>
              <p className="text-xs text-slate-500 dark:text-slate-400">Output hiện có, trạng thái duyệt và dữ liệu cho bước kế tiếp.</p>
            </div>
            {currentStep && (
              <span className="inline-flex shrink-0 items-center gap-1.5 rounded-md bg-indigo-50 dark:bg-indigo-950/40 px-2.5 py-1 text-[10px] font-bold text-indigo-700 dark:text-indigo-300 leading-none">
                P{currentStep.stepNumber}
              </span>
            )}
          </div>

          {currentStep && (
            <>
              {/* Current card */}
              <div className="mt-4 rounded-lg border border-slate-200 dark:border-slate-700 bg-blue-50/50 dark:bg-blue-950/10 p-4 space-y-3">
                <span className="block text-[10px] font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                  Hiện tại · Phần {currentStep.stepNumber}
                </span>
                <h3 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{currentStep.stepName}</h3>
                <div className="border-t border-slate-200 dark:border-slate-700 pt-3">
                  <span className="block text-[10px] font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-1">
                    {currentStep.stepNumber === 1 ? 'Content Brief' : 'Kết quả'}
                  </span>
                  <p className="text-xs text-slate-700 dark:text-slate-200 leading-relaxed">
                    {currentStep.outputSummaryText || (
                      <span className="text-slate-400 italic">Chưa có dữ liệu</span>
                    )}
                  </p>
                </div>
                <div className="flex items-center justify-between gap-3">
                  <span className="text-[10px] text-slate-500 dark:text-slate-400">Cần người duyệt?</span>
                  {currentStep.stepStatus === CProjectStepStatus.Completed ? (
                    <Badge variant="success" dot className="text-[10px]">Đã duyệt</Badge>
                  ) : currentStep.needsApproval ? (
                    <span className="text-[10px] font-bold text-amber-600 dark:text-amber-400">Chưa · đang xử lý</span>
                  ) : (
                    <span className="text-[10px] text-slate-400">Không</span>
                  )}
                </div>
              </div>

              {/* Next step */}
              {nextStep && (
                <div className="mt-4 rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800/30 p-4 space-y-2">
                  <span className="block text-[10px] font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                    Bước kế tiếp
                  </span>
                  <h4 className="text-sm font-semibold text-slate-900 dark:text-slate-100">
                    Phần {nextStep.stepNumber} · {nextStep.stepName}
                  </h4>
                  <p className="text-xs text-slate-500 dark:text-slate-400">
                    {NEXT_STEP_HINTS[project.currentStepNumber as keyof typeof NEXT_STEP_HINTS] ?? ''}
                  </p>
                </div>
              )}

              {/* Open button */}
              <Button
                variant="default"
                className="w-full mt-4"
                onClick={() => {
                  const route = STEP_NUMBER_ROUTES[project.currentStepNumber as keyof typeof STEP_NUMBER_ROUTES]
                  if (route) navigate(route + '?projectId=' + encodeURIComponent(activeProjectId!))
                }}
              >
                Mở Phần {currentStep.stepNumber} →
              </Button>
            </>
          )}

          {!currentStep && (
            <p className="text-sm text-slate-400 dark:text-slate-500 mt-4">Không có bước nào đang xử lý</p>
          )}
        </div>
      </div>
    </div>
  )
}
