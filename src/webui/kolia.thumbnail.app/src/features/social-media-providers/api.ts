import { httpClient } from '../../lib/api/http-client'
import { buildPagedQuery } from '../../lib/api/build-paged-query'
import type { BackendPagedResponse, PagedResult, PagedRequestParams } from '../../types/paging.types'
import type { CreateSocialMediaProviderInput } from './schema'

export interface SocialMediaProviderEndpointDto {
  type: number
  endpoint: string
  jsonResponse: string
  jsonError: string
  jsonRequest: string
}

export interface SocialMediaProviderBaseDto {
  id: string
  name: string
  shortName: string
  providerType: number
  imageUrl?: string | null
  baseUrl: string
  endpoints: SocialMediaProviderEndpointDto[]
  isDeleted: boolean
  creationTime?: string | null
  lastModificationTime?: string | null
}

export interface SocialMediaProviderDetailDto extends SocialMediaProviderBaseDto {
  isDeleted: boolean
  lastModificationTime?: string | null
  deletionTime?: string | null
}

function toPagedResult(payload: BackendPagedResponse<SocialMediaProviderBaseDto>): PagedResult<SocialMediaProviderBaseDto> {
  return {
    items: payload.items,
    pageNumber: payload.pageInfo.pageNumber,
    pageSize: payload.pageInfo.pageSize,
    totalCount: payload.pageInfo.totalRecords,
    totalPages: payload.pageInfo.totalPages,
  }
}

export async function getSocialMediaProvidersWithPaging(params: PagedRequestParams) {
  const query = buildPagedQuery({
    includeTotalCount: true,
    includeItems: true,
    ...params,
  })

  const response = await httpClient.get<BackendPagedResponse<SocialMediaProviderBaseDto>>(`/api/v1/social-media-providers/paging?${query.toString()}`)
  return toPagedResult(response)
}

export async function createSocialMediaProvider(data: CreateSocialMediaProviderInput): Promise<SocialMediaProviderDetailDto> {
  return httpClient.post<SocialMediaProviderDetailDto>('/api/v1/social-media-providers', {
    name: data.name,
    shortName: data.shortName,
    providerType: data.providerType,
    imageUrl: data.imageUrl || null,
    baseUrl: data.baseUrl,
  })
}

export interface UpdateSocialMediaProviderInput extends CreateSocialMediaProviderInput {
  id: string
}

export async function updateSocialMediaProvider(data: UpdateSocialMediaProviderInput): Promise<SocialMediaProviderDetailDto> {
  return httpClient.put<SocialMediaProviderDetailDto>(`/api/v1/social-media-providers/${data.id}`, {
    name: data.name,
    shortName: data.shortName,
    providerType: data.providerType,
    imageUrl: data.imageUrl || null,
    baseUrl: data.baseUrl,
  })
}

export async function deleteSocialMediaProvider(id: string): Promise<void> {
  await httpClient.delete(`/api/v1/social-media-providers/${id}`)
}

