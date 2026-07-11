import { httpClient } from '../../lib/api/http-client'
import { buildPagedQuery } from '../../lib/api/build-paged-query'
import type { BackendPagedResponse, PagedResult, PagedRequestParams } from '../../types/paging.types'
import type { CreateAIProviderInput } from './schema'

export interface ThumbnailItem {
  id: string
  name: string
  shortName: string
  imageUrl?: string | null
  isDeleted: boolean
  creationTime?: string | null
  lastModificationTime?: string | null
}

export interface AIProviderDetailDto extends ThumbnailItem {
  isDeleted: boolean
  lastModificationTime?: string | null
  deletionTime?: string | null
}

function toPagedResult(payload: BackendPagedResponse<ThumbnailItem>): PagedResult<ThumbnailItem> {
  return {
    items: payload.items,
    pageNumber: payload.pageInfo.pageNumber,
    pageSize: payload.pageInfo.pageSize,
    totalCount: payload.pageInfo.totalRecords,
    totalPages: payload.pageInfo.totalPages,
  }
}

export async function fetchThumbnails(params: PagedRequestParams) {
  const query = buildPagedQuery({
    includeTotalCount: true,
    includeItems: true,
    ...params,
  })

  const response = await httpClient.get<BackendPagedResponse<ThumbnailItem>>(`/api/v1/ai-providers/paging?${query.toString()}`)
  return toPagedResult(response)
}

export async function createAIProvider(data: CreateAIProviderInput): Promise<AIProviderDetailDto> {
  return httpClient.post<AIProviderDetailDto>('/api/v1/ai-providers', {
    name: data.name,
    shortName: data.shortName,
    imageUrl: data.imageUrl || null,
  })
}

export interface UpdateAIProviderInput extends CreateAIProviderInput {
  id: string
}

export async function updateAIProvider(data: UpdateAIProviderInput): Promise<AIProviderDetailDto> {
  return httpClient.put<AIProviderDetailDto>(`/api/v1/ai-providers/${data.id}`, {
    name: data.name,
    shortName: data.shortName,
    imageUrl: data.imageUrl || null,
  })
}

export async function deleteAIProvider(id: string): Promise<void> {
  await httpClient.delete(`/api/v1/ai-providers/${id}`)
}

