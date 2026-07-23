import { httpClient } from '../../lib/api/http-client'
import { buildPagedQuery } from '../../lib/api/build-paged-query'
import type { BackendPagedResponse, PagedResult, PagedRequestParams } from '../../types/paging.types'
import type { CreateNewsSourceInput, UpdateNewsSourceInput } from './schema'

// ── DTOs ──────────────────────────────────────────────────────────────────

export interface NewsSourceListItemDto {
  id: string
  name: string
  rssOrFeedUrl: string
  region: number
  isTrusted: boolean
  priority: number
  sourceGroup: number
  fetchMode: number
  domain: string
  lastFetchedAt: string | null
  lastFailedAt: string | null
  consecutiveFailureCount: number
  operationalStatus: string
  isDeleted?: boolean
  creationTime?: string | null
  lastModificationTime?: string | null
}

export interface NewsSourceDetailDto extends NewsSourceListItemDto {
  lastEtag?: string | null
  lastModifiedHeader?: string | null
  deletionTime?: string | null
}

export interface NewsSourceTestFetchResultDto {
  success: boolean
  tierUsed: string
  itemCount: number
  items: NewsSourcePreviewItemDto[]
  errorMessage?: string | null
}

export interface NewsSourcePreviewItemDto {
  title: string
  sourceUrl: string
  publishedTime: string | null
  summaryRaw: string
}

// ── Paging helpers ────────────────────────────────────────────────────────

function toPagedResult(payload: BackendPagedResponse<NewsSourceListItemDto>): PagedResult<NewsSourceListItemDto> {
  return {
    items: payload.items,
    pageNumber: payload.pageInfo.pageNumber,
    pageSize: payload.pageInfo.pageSize,
    totalCount: payload.pageInfo.totalRecords,
    totalPages: payload.pageInfo.totalPages,
  }
}

export interface NewsSourceListParams extends PagedRequestParams {
  group?: number
  region?: number
  isTrusted?: boolean
  includeDeleted?: boolean
  deletedOnly?: boolean
}

export async function getNewsSourcesWithPaging(params: NewsSourceListParams) {
  const query = buildPagedQuery({ includeTotalCount: true, includeItems: true, ...params })
  if (params.group !== undefined) query.set('group', String(params.group))
  if (params.region !== undefined) query.set('region', String(params.region))
  if (params.isTrusted !== undefined) query.set('isTrusted', String(params.isTrusted))
  if (params.includeDeleted !== undefined) query.set('includeDeleted', String(params.includeDeleted))
  if (params.deletedOnly !== undefined) query.set('deletedOnly', String(params.deletedOnly))

  const response = await httpClient.get<BackendPagedResponse<NewsSourceListItemDto>>(
    `/admin/news-sources/paging?${query.toString()}`,
  )
  return toPagedResult(response)
}

export async function getNewsSourceById(id: string): Promise<NewsSourceDetailDto> {
  return httpClient.get<NewsSourceDetailDto>(`/admin/news-sources/${id}`)
}

export async function createNewsSource(data: CreateNewsSourceInput): Promise<NewsSourceDetailDto> {
  return httpClient.post<NewsSourceDetailDto>('/admin/news-sources', data)
}

export async function updateNewsSource(data: UpdateNewsSourceInput): Promise<NewsSourceDetailDto> {
  const { id, ...body } = data
  return httpClient.put<NewsSourceDetailDto>(`/admin/news-sources/${id}`, body)
}

export async function toggleNewsSource(id: string): Promise<NewsSourceDetailDto> {
  return httpClient.patch<NewsSourceDetailDto>(`/admin/news-sources/${id}/toggle`)
}

export async function testFetchNewsSource(
  id: string,
  keywords: string[],
): Promise<NewsSourceTestFetchResultDto> {
  return httpClient.post<NewsSourceTestFetchResultDto>(`/admin/news-sources/${id}/test`, keywords)
}

export async function deleteNewsSource(id: string): Promise<void> {
  await httpClient.delete(`/admin/news-sources/${id}`)
}
