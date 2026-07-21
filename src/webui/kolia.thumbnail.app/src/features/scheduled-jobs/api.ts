import { httpClient } from '../../lib/api/http-client'
import { buildPagedQuery } from '../../lib/api/build-paged-query'
import type { BackendPagedResponse, PagedResult, PagedRequestParams } from '../../types/paging.types'

export interface ScheduledJobDto {
  id: string
  name: string
  description?: string | null
  sourceType: number
  sourceUrl: string
  googleServiceAccountId: string
  serviceAccountName?: string | null
  serviceAccountEmail?: string | null
  status: number
  errorMessage?: string | null
  createdProjectId?: string | null
  createdBriefId?: string | null
  cronExpression?: string | null
  cronDescription?: string | null
  scheduledAt?: string | null
  startedAt?: string | null
  completedAt?: string | null
  retryCount: number
  maxRetries: number
  creationTime: string
  lastModificationTime?: string | null
}

export interface ScheduledJobSummaryDto {
  id: string
  name: string
  sourceType: number
  sourceUrl: string
  serviceAccountName?: string | null
  status: number
  errorMessage?: string | null
  cronExpression?: string | null
  cronDescription?: string | null
  scheduledAt?: string | null
  createdProjectId?: string | null
  retryCount: number
  creationTime: string
}

export interface LogEntry {
  timestamp: string
  level: string
  message: string
}

export interface CheckAccessResult {
  hasAccess: boolean
  errorMessage?: string | null
  instruction?: string | null
}

function toPagedResult(payload: BackendPagedResponse<ScheduledJobSummaryDto>): PagedResult<ScheduledJobSummaryDto> {
  return {
    items: payload.items,
    pageNumber: payload.pageInfo.pageNumber,
    pageSize: payload.pageInfo.pageSize,
    totalCount: payload.pageInfo.totalRecords,
    totalPages: payload.pageInfo.totalPages,
  }
}

export async function getScheduledJobsWithPaging(params: PagedRequestParams) {
  const query = buildPagedQuery({
    includeTotalCount: true,
    includeItems: true,
    ...params,
  })
  const response = await httpClient.get<BackendPagedResponse<ScheduledJobSummaryDto>>(
    `/api/v1/admin/scheduled-import-jobs/paging?${query.toString()}`
  )
  return toPagedResult(response)
}

export async function getScheduledJob(id: string): Promise<ScheduledJobDto> {
  return httpClient.get<ScheduledJobDto>(`/api/v1/admin/scheduled-import-jobs/${id}`)
}

export interface CreateScheduledJobInput {
  name: string
  description?: string | null
  sourceType?: number
  sourceUrl: string
  googleServiceAccountId: string
  scheduledAt?: string | null
  cronExpression?: string | null
  cronDescription?: string | null
  timeZone?: string | null
  maxRetries?: number
}

export async function createScheduledJob(data: CreateScheduledJobInput): Promise<ScheduledJobDto> {
  return httpClient.post<ScheduledJobDto>('/api/v1/admin/scheduled-import-jobs', data)
}

export interface UpdateScheduledJobInput {
  name: string
  description?: string | null
  sourceUrl: string
  googleServiceAccountId: string
  scheduledAt?: string | null
  cronExpression?: string | null
  cronDescription?: string | null
  timeZone?: string | null
  maxRetries: number
}

export async function updateScheduledJob(id: string, data: UpdateScheduledJobInput): Promise<ScheduledJobDto> {
  return httpClient.put<ScheduledJobDto>(`/api/v1/admin/scheduled-import-jobs/${id}`, data)
}

export async function cancelScheduledJob(id: string): Promise<void> {
  await httpClient.post(`/api/v1/admin/scheduled-import-jobs/${id}/cancel`, {})
}

export async function deleteScheduledJob(id: string): Promise<void> {
  await httpClient.delete(`/api/v1/admin/scheduled-import-jobs/${id}`)
}

export async function checkAccess(data: {
  sourceUrl: string
  sourceType?: number
  googleServiceAccountId: string
}): Promise<CheckAccessResult> {
  return httpClient.post<CheckAccessResult>('/api/v1/admin/scheduled-import-jobs/check-access', data)
}

export async function retryScheduledJob(id: string): Promise<ScheduledJobDto> {
  return httpClient.post<ScheduledJobDto>(`/api/v1/admin/scheduled-import-jobs/${id}/retry`, {})
}

export async function getJobLogs(id: string): Promise<LogEntry[]> {
  return httpClient.get<LogEntry[]>(`/api/v1/admin/scheduled-import-jobs/${id}/logs`)
}
