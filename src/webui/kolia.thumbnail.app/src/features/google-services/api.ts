import { httpClient } from '../../lib/api/http-client'
import { buildPagedQuery } from '../../lib/api/build-paged-query'
import type { BackendPagedResponse, PagedResult, PagedRequestParams } from '../../types/paging.types'

export interface GoogleServiceAccountDto {
  id: string
  name: string
  description?: string | null
  clientEmail: string
  clientId?: string | null
  projectId?: string | null
  tokenUri?: string | null
  scopes?: string | null
  isEnabled: boolean
  creationTime: string
  lastModificationTime?: string | null
}

export interface GoogleServiceAccountSummaryDto {
  id: string
  name: string
  clientEmail: string
  projectId?: string | null
  isEnabled: boolean
  totalJobs: number
  creationTime: string
}

function toPagedResult(payload: BackendPagedResponse<GoogleServiceAccountSummaryDto>): PagedResult<GoogleServiceAccountSummaryDto> {
  return {
    items: payload.items,
    pageNumber: payload.pageInfo.pageNumber,
    pageSize: payload.pageInfo.pageSize,
    totalCount: payload.pageInfo.totalRecords,
    totalPages: payload.pageInfo.totalPages,
  }
}

export async function getGoogleServiceAccountsWithPaging(params: PagedRequestParams) {
  const query = buildPagedQuery({
    includeTotalCount: true,
    includeItems: true,
    ...params,
  })
  const response = await httpClient.get<BackendPagedResponse<GoogleServiceAccountSummaryDto>>(
    `/api/v1/admin/google-service-accounts/paging?${query.toString()}`
  )
  return toPagedResult(response)
}

export async function getGoogleServiceAccount(id: string): Promise<GoogleServiceAccountDto> {
  return httpClient.get<GoogleServiceAccountDto>(`/api/v1/admin/google-service-accounts/${id}`)
}

export interface CreateGoogleServiceAccountInput {
  name: string
  description?: string | null
  credentialJson: string
  scopes?: string | null
}

export async function createGoogleServiceAccount(data: CreateGoogleServiceAccountInput): Promise<GoogleServiceAccountDto> {
  return httpClient.post<GoogleServiceAccountDto>('/api/v1/admin/google-service-accounts', data)
}

export interface UpdateGoogleServiceAccountInput {
  name: string
  description?: string | null
  credentialJson?: string | null
  scopes?: string | null
  isEnabled: boolean
}

export async function updateGoogleServiceAccount(id: string, data: UpdateGoogleServiceAccountInput): Promise<GoogleServiceAccountDto> {
  return httpClient.put<GoogleServiceAccountDto>(`/api/v1/admin/google-service-accounts/${id}`, data)
}

export async function deleteGoogleServiceAccount(id: string): Promise<void> {
  await httpClient.delete(`/api/v1/admin/google-service-accounts/${id}`)
}

/**
 * Upload file JSON credential để tạo Service Account.
 * Gửi dưới dạng multipart/form-data.
 */
export async function importGoogleServiceAccountFile(
  name: string,
  file: File,
  description?: string | null,
  scopes?: string | null,
): Promise<GoogleServiceAccountDto> {
  const formData = new FormData()
  formData.append('name', name)
  if (description) formData.append('description', description)
  if (scopes) formData.append('scopes', scopes)
  formData.append('file', file)

  const baseUrl = import.meta.env.VITE_API_BASE_URL ?? 'https://holes-interactive-variations-given.trycloudflare.com'
  const res = await fetch(`${baseUrl}/api/v1/admin/google-service-accounts/import-file`, {
    method: 'POST',
    body: formData,
  })

  if (!res.ok) {
    const text = await res.text()
    const body = text ? JSON.parse(text) : null
    throw new Error(body?.message ?? 'Upload file thất bại.')
  }

  if (res.status === 204) return undefined as unknown as GoogleServiceAccountDto
  return (await res.json()) as GoogleServiceAccountDto
}
