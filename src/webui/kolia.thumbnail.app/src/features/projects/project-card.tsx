import { Pencil, Trash2, ImageOff } from 'lucide-react'
import { Badge } from '../../components/ui/badge'
import { formatDateTimeRelative, formatDateTime } from '../../lib/date-formatter'
import { cn } from '../../lib/utils'
import type { ProjectBaseDto } from './api'
import { getProjectStatusLabel, getProjectStatusBadgeClass, getPipelineStepLabel } from './project-type'

interface ProjectCardProps {
  project: ProjectBaseDto
  onEdit: (project: ProjectBaseDto) => void
  onRemove: (project: ProjectBaseDto) => void
}

export function ProjectCard({ project, onEdit, onRemove }: ProjectCardProps) {
  // Nếu chưa có step nào hoàn thành → mặc định hiển thị số 1
  const completedStepsDisplay = Math.max(project.completedSteps ?? 0, 1)
  const bottomText = project.displayText?.trim() ? project.displayText : getPipelineStepLabel(project.currentStep)

  return (
    <div className="group relative flex flex-col overflow-hidden rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 shadow-sm transition-shadow hover:shadow-md">
      {/* ── Phần trên: ảnh + badges + hover actions ─────────────────── */}
      <div className="relative aspect-video w-full overflow-hidden bg-slate-100 dark:bg-slate-800">
        {project.thumbnailUrl ? (
          <img
            src={project.thumbnailUrl}
            alt={project.videoTitle || project.name}
            className="h-full w-full object-cover"
            loading="lazy"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center">
            <ImageOff className="h-8 w-8 text-slate-300 dark:text-slate-600" />
          </div>
        )}

        {/* Top-left: Project status — nổi bật */}
        <div className="absolute left-2 top-2 z-10">
          <span
            className={cn(
              'inline-flex items-center rounded-md px-2 py-1 text-[11px] font-bold uppercase tracking-wide shadow-sm backdrop-blur-sm',
              getProjectStatusBadgeClass(project.status),
            )}
          >
            {getProjectStatusLabel(project.status)}
          </span>
        </div>

        {/* Top-right: số bước đã hoàn thành */}
        <div className="absolute right-2 top-2 z-10">
          <Badge variant="default" className="shadow-sm">
            {completedStepsDisplay}/6
          </Badge>
        </div>

        {/* Gradient nền cho text bottom-left dễ đọc trên mọi ảnh */}
        <div className="pointer-events-none absolute inset-x-0 bottom-0 z-[5] h-14 bg-gradient-to-t from-black/80 to-transparent" />

        {/* Bottom-left: DisplayText, fallback bước hiện tại */}
        <div className="absolute inset-x-2 bottom-1.5 z-10">
          <p className="truncate text-xs font-medium text-white drop-shadow" title={bottomText}>
            {bottomText}
          </p>
        </div>

        {/* Hover overlay: Edit / Remove */}
        <div className="absolute inset-0 z-20 flex items-center justify-center gap-2 bg-slate-900/0 opacity-0 transition-all duration-200 group-hover:bg-slate-900/50 group-hover:opacity-100">
          <button
            type="button"
            title="Chỉnh sửa"
            onClick={(e) => {
              e.stopPropagation()
              onEdit(project)
            }}
            className="rounded-full bg-white/95 dark:bg-slate-900/95 p-2 text-slate-700 dark:text-slate-200 shadow-lg transition-transform hover:scale-105 hover:bg-white hover:dark:bg-slate-900"
          >
            <Pencil className="h-4 w-4" />
          </button>
          <button
            type="button"
            title="Xoá"
            onClick={(e) => {
              e.stopPropagation()
              onRemove(project)
            }}
            className="rounded-full bg-white/95 dark:bg-slate-900/95 p-2 text-rose-600 dark:text-rose-400 shadow-lg transition-transform hover:scale-105 hover:bg-rose-50 hover:dark:bg-rose-950/60"
          >
            <Trash2 className="h-4 w-4" />
          </button>
        </div>
      </div>

      {/* ── Phần dưới: nội dung ──────────────────────────────────────── */}
      <div className="flex flex-1 flex-col gap-0.5 px-3 py-2.5">
        <p
          className="truncate text-sm font-semibold text-slate-900 dark:text-slate-100"
          title={project.videoTitle || '—'}
        >
          {project.videoTitle || '—'}
        </p>
        <p className="truncate text-xs text-slate-500 dark:text-slate-400" title={project.name}>
          {project.name}
        </p>
        <p
          className="mt-0.5 text-[11px] text-slate-400 dark:text-slate-500"
          title={formatDateTime(project.lastModificationTime)}
        >
          Cập nhật {formatDateTimeRelative(project.lastModificationTime)}
        </p>
      </div>
    </div>
  )
}
