import { z } from 'zod'

export const CGoogleServiceType = {
  GoogleSheets: 1,
  GoogleDocs: 2,
} as const

export const GOOGLE_SERVICE_TYPE_OPTIONS = [
  { id: CGoogleServiceType.GoogleSheets, label: 'Google Sheets' },
  { id: CGoogleServiceType.GoogleDocs, label: 'Google Docs' },
]

export function getGoogleServiceTypeLabel(type: number): string {
  return GOOGLE_SERVICE_TYPE_OPTIONS.find((o) => o.id === type)?.label ?? 'Không xác định'
}

export const CJobScheduleStatus = {
  Pending: 1,
  Running: 2,
  Completed: 3,
  Failed: 4,
  Cancelled: 5,
} as const

export function getJobStatusLabel(status: number): string {
  const map: Record<number, string> = {
    1: 'Đang chờ',
    2: 'Đang chạy',
    3: 'Hoàn thành',
    4: 'Thất bại',
    5: 'Đã huỷ',
  }
  return map[status] ?? 'Không xác định'
}

export function getJobStatusBadgeClass(status: number): string {
  const map: Record<number, string> = {
    1: 'bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300',
    2: 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900 dark:text-yellow-300',
    3: 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300',
    4: 'bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300',
    5: 'bg-slate-100 text-slate-500 dark:bg-slate-800 dark:text-slate-400',
  }
  return map[status] ?? 'bg-slate-100 text-slate-500'
}

export const createScheduledJobSchema = z.object({
  name: z.string().min(1, 'Tên không được để trống'),
  description: z.string().optional().nullable(),
  sourceType: z.number().refine((val) => [1, 2].includes(val), { message: 'Loại nguồn không hợp lệ' }),
  sourceUrl: z.string().min(1, 'URL không được để trống').url('URL không hợp lệ'),
  googleServiceAccountId: z.string().min(1, 'Vui lòng chọn service account'),
  scheduleType: z.enum(['now', 'once', 'cron']),
  scheduledAt: z.string().optional().nullable(),
  cronExpression: z.string().optional().nullable(),
  cronDescription: z.string().optional().nullable(),
  maxRetries: z.number().min(0).max(10),
})

export type CreateScheduledJobInput = z.infer<typeof createScheduledJobSchema>
