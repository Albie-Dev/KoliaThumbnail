// ─────────────────────────────────────────────────────────────────────────────
// LƯU Ý: Các giá trị enum/label dưới đây được suy ra từ pipeline 6 bước khai báo
// trong `src/lib/admin-menu.tsx` (Nội dung video → Tin tức → Thumbnail tham khảo
// → Thumbnail → Tạo video title → Bộ hoàn chỉnh) vì tài liệu API tại
// https://blast-case-skeptic.ngrok-free.dev/scalar không thể truy cập được từ môi
// trường hiện tại (ngrok chặn request tự động bằng trang cảnh báo trình duyệt).
// Nếu API thực tế trả về enum/field khác, chỉ cần sửa trong file này + `api.ts`
// — phần UI (project-card, projects-page) không phụ thuộc trực tiếp vào giá trị số.
// ─────────────────────────────────────────────────────────────────────────────

/** Mirrors BE enum CProjectStatus (giả định — xác nhận lại với API thực tế) */
export const CProjectStatus = {
  Draft: 0,
  Processing: 1,
  Completed: 2,
  Failed: 3,
} as const

export type CProjectStatus = (typeof CProjectStatus)[keyof typeof CProjectStatus]

export interface CProjectStatusOption {
  id: CProjectStatus
  label: string
}

export const PROJECT_STATUS_OPTIONS: CProjectStatusOption[] = [
  { id: CProjectStatus.Draft, label: 'Nháp' },
  { id: CProjectStatus.Processing, label: 'Đang xử lý' },
  { id: CProjectStatus.Completed, label: 'Hoàn thành' },
  { id: CProjectStatus.Failed, label: 'Lỗi' },
]

export function getProjectStatusLabel(status: number): string {
  return PROJECT_STATUS_OPTIONS.find((o) => o.id === status)?.label ?? `#${status}`
}

/** Class màu cho Badge trạng thái — nổi bật, tuân theo bảng ánh xạ RULES.md mục 5 */
export function getProjectStatusBadgeClass(status: number): string {
  switch (status) {
    case CProjectStatus.Completed:
      return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/60 dark:text-emerald-300 ring-1 ring-emerald-200 dark:ring-emerald-800'
    case CProjectStatus.Processing:
      return 'bg-amber-100 text-amber-700 dark:bg-amber-950/60 dark:text-amber-300 ring-1 ring-amber-200 dark:ring-amber-800'
    case CProjectStatus.Failed:
      return 'bg-rose-100 text-rose-700 dark:bg-rose-950/60 dark:text-rose-300 ring-1 ring-rose-200 dark:ring-rose-800'
    case CProjectStatus.Draft:
    default:
      return 'bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-300 ring-1 ring-slate-200 dark:ring-slate-700'
  }
}

// ── Pipeline steps (đồng bộ với src/lib/admin-menu.tsx) ─────────────────────
export interface PipelineStepInfo {
  step: number
  label: string
}

export const PIPELINE_STEPS: PipelineStepInfo[] = [
  { step: 1, label: 'Nội dung video' },
  { step: 2, label: 'Tin tức' },
  { step: 3, label: 'Thumbnail tham khảo' },
  { step: 4, label: 'Thumbnail' },
  { step: 5, label: 'Tạo video title' },
  { step: 6, label: 'Bộ hoàn chỉnh' },
]

export const TOTAL_PIPELINE_STEPS = PIPELINE_STEPS.length

export function getPipelineStepLabel(step: number): string {
  const found = PIPELINE_STEPS.find((s) => s.step === step)
  return found ? `Bước ${found.step}: ${found.label}` : `Bước ${step}`
}
