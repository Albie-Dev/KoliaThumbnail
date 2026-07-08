import { httpClient } from '../../lib/api/http-client'
import type { BackendPagedResponse, PagedResult } from '../../types/paging.types'
import type { CreateAIProviderInput } from './schema'

export interface ThumbnailItem {
  id: string
  name: string
  shortName: string
  imageUrl?: string | null
  creationTime?: string | null
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

export async function fetchThumbnails(pageNumber = 1, pageSize = 20) {
  const query = new URLSearchParams({
    PageNumber: String(pageNumber),
    PageSize: String(pageSize),
    IncludeTotalCount: 'true',
    IncludeItems: 'true',
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
