import { httpClient } from '../../lib/api/http-client'
import type { CThumbnailTimeFilter, CThumbnailSortFilter, CLibraryUserStatus } from '../../types/enums/pipeline.enums'

// ── DTOs ──────────────────────────────────────────────────────────────────

export interface ThumbnailLibraryItemDto {
  id: string
  projectId: string
  thumbnailSearchRequestId?: string | null
  sourceType: number
  platform: number
  videoTitle: string
  videoUrl: string
  channelName?: string | null
  thumbnailImageUrl: string
  marketType?: number | null
  publishedTime?: string | null
  viewCount?: number | null
  keywordBatchTag?: string | null
  userStatus: number
  isFilteredIrrelevant: boolean
  hasAnalysis: boolean
}

export interface ThumbnailSearchResultDto {
  items: ThumbnailLibraryItemDto[]
}

export interface ThumbnailAnalysisDto {
  id: string
  thumbnailLibraryItemId: string
  thumbnailFactorsJson: string
  titleTextAnalysis: string
  videoTitleAnalysis: string
  displayTextStyleNote: string
  isChosenForGeneration: boolean
}

// ── API functions ─────────────────────────────────────────────────────────

export async function getThumbnailLibrary(
  projectId: string,
  excludeIrrelevant = true,
): Promise<ThumbnailLibraryItemDto[]> {
  return httpClient.get<ThumbnailLibraryItemDto[]>(
    `/api/v1/projects/${projectId}/thumbnail-library?excludeIrrelevant=${excludeIrrelevant}`,
  )
}

export async function searchThumbnails(
  projectId: string,
  data: {
    keyword: string
    timeFilter: CThumbnailTimeFilter
    sortFilter: CThumbnailSortFilter
    wasSuggestedFromNews?: boolean
  },
): Promise<ThumbnailSearchResultDto> {
  return httpClient.post<ThumbnailSearchResultDto>(
    `/api/v1/projects/${projectId}/thumbnail-library/search`,
    data,
  )
}

export async function importThumbnailManual(
  projectId: string,
  videoUrl: string,
): Promise<ThumbnailLibraryItemDto> {
  return httpClient.post<ThumbnailLibraryItemDto>(
    `/api/v1/projects/${projectId}/thumbnail-library/import`,
    { videoUrl },
  )
}

export async function updateThumbnailStatus(
  projectId: string,
  itemId: string,
  status: CLibraryUserStatus,
): Promise<void> {
  await httpClient.put(
    `/api/v1/projects/${projectId}/thumbnail-library/items/${itemId}/status`,
    { status },
  )
}

export async function chooseThumbnailForGeneration(
  projectId: string,
  itemId: string,
  isChosen: boolean,
): Promise<void> {
  await httpClient.put(
    `/api/v1/projects/${projectId}/thumbnail-library/items/${itemId}/choose-for-generation`,
    { isChosen },
  )
}

export async function deepAnalyzeThumbnail(
  projectId: string,
  itemId: string,
): Promise<ThumbnailAnalysisDto> {
  return httpClient.post<ThumbnailAnalysisDto>(
    `/api/v1/projects/${projectId}/thumbnail-library/items/${itemId}/deep-analyze`,
    {},
  )
}
