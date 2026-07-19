import { httpClient } from '../../lib/api/http-client'
import { buildPagedQuery } from '../../lib/api/build-paged-query'
import type { BackendPagedResponse, PagedResult, PagedRequestParams } from '../../types/paging.types'

// ── DTOs ──────────────────────────────────────────────────────────────────

export interface ProjectSummaryDto {
  id: string
  name: string
  status: number
  currentStepNumber: number
  thumbnailCoverUrl?: string | null
  lastActivityTime?: string | null
  creationTime?: string | null
}

export interface ProjectStepDto {
  id: string
  stepNumber: number
  stepName: string
  stepStatus: number
  outputSummaryText?: string | null
  needsApproval: boolean
  approvedAt?: string | null
}

export interface ProjectDetailDto extends ProjectSummaryDto {
  steps: ProjectStepDto[]
}

// ── API functions ─────────────────────────────────────────────────────────

function toPagedResult(payload: BackendPagedResponse<ProjectSummaryDto>): PagedResult<ProjectSummaryDto> {
  return {
    items: payload.items,
    pageNumber: payload.pageInfo.pageNumber,
    pageSize: payload.pageInfo.pageSize,
    totalCount: payload.pageInfo.totalRecords,
    totalPages: payload.pageInfo.totalPages,
  }
}

export async function getProjectsWithPaging(params: PagedRequestParams) {
  const query = buildPagedQuery({
    includeTotalCount: true,
    includeItems: true,
    ...params,
  })

  const response = await httpClient.get<BackendPagedResponse<ProjectSummaryDto>>(
    `/api/v1/projects/paging?${query.toString()}`,
  )
  return toPagedResult(response)
}

export async function getProjectById(id: string): Promise<ProjectDetailDto> {
  return httpClient.get<ProjectDetailDto>(`/api/v1/projects/${id}`)
}

export async function createProject(data: { name: string }): Promise<ProjectDetailDto> {
  return httpClient.post<ProjectDetailDto>('/api/v1/projects', data)
}

export async function renameProject(id: string, newName: string): Promise<void> {
  await httpClient.put(`/api/v1/projects/${id}/rename`, { newName })
}

export async function deleteProject(id: string): Promise<void> {
  await httpClient.delete(`/api/v1/projects/${id}`)
}

export async function advanceProjectStep(id: string): Promise<void> {
  await httpClient.post(`/api/v1/projects/${id}/advance-step`, {})
}
