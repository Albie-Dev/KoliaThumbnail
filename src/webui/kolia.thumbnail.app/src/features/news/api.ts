import { httpClient } from '../../lib/api/http-client'
import { buildPagedQuery } from '../../lib/api/build-paged-query'
import type { BackendPagedResponse, PagedResult, PagedRequestParams } from '../../types/paging.types'
import type { CMarketScope, CNewsTimeRange, CNewsCountFilter, CRelevanceLevel } from '../../types/enums/pipeline.enums'

// ── DTOs ──────────────────────────────────────────────────────────────────

export interface NewsItemDto {
  id: string
  projectId: string
  newsSearchRequestId?: string | null
  sourceType: number
  title: string
  sourceName: string
  sourceUrl: string
  marketType: number
  publishedTime?: string | null
  scannedTime?: string | null
  summaryOverview: string
  relevanceToTopicScore: number
  importanceImpactScore: number
  emotionPotentialScore: number
  noveltyDataScore: number
  totalScore: number
  recommendation: number
  relevanceLevel: CRelevanceLevel
  isSelectedByTeam: boolean
  suggestedKeywordsForThumbnail?: string | null
  hasDeepAnalysis: boolean
  keywordBatchGroup?: string | null
  emotionTags: number
}

export interface NewsSearchResultDto {
  searchRequestId: string
  marketScope: number
  timeRange: number
  items: NewsItemDto[]
}

export interface NewsDeepAnalysisDto {
  id: string
  newsItemId: string
  macroEventSummary: string[]
  marketReactionJson: string
  expectationShortTerm: string
  expectationLongTerm: string
  sentimentOverviewJson: string
  emotionTags: number
  emotionReason: string
  wasTranslatedFromForeign: boolean
  missingDataNote?: string | null
}

// ── Paging helpers ────────────────────────────────────────────────────────

function toPagedResult(payload: BackendPagedResponse<NewsItemDto>): PagedResult<NewsItemDto> {
  return {
    items: payload.items,
    pageNumber: payload.pageInfo.pageNumber,
    pageSize: payload.pageInfo.pageSize,
    totalCount: payload.pageInfo.totalRecords,
    totalPages: payload.pageInfo.totalPages,
  }
}

// ── API functions ─────────────────────────────────────────────────────────

export async function getNews(projectId: string): Promise<NewsItemDto[]> {
  return httpClient.get<NewsItemDto[]>(`/api/v1/projects/${projectId}/news`)
}

export async function getNewsPaging(projectId: string, params: PagedRequestParams) {
  const query = buildPagedQuery({ includeTotalCount: true, includeItems: true, ...params })
  const response = await httpClient.get<BackendPagedResponse<NewsItemDto>>(
    `/api/v1/projects/${projectId}/news/paging?${query.toString()}`,
  )
  return toPagedResult(response)
}

export async function searchNews(
  projectId: string,
  data: {
    marketScope: CMarketScope
    timeRange: CNewsTimeRange
    countFilter: CNewsCountFilter
    keywordsRaw: string
    suggestedKeywordsSelected?: string[]
  },
  operationId?: string,
): Promise<NewsSearchResultDto> {
  const query = operationId ? `?operationId=${operationId}` : ''
  return httpClient.post<NewsSearchResultDto>(`/api/v1/projects/${projectId}/news/search${query}`, data)
}

export async function importNewsManual(projectId: string, url: string): Promise<NewsItemDto> {
  return httpClient.post<NewsItemDto>(`/api/v1/projects/${projectId}/news/import`, { url })
}

export async function getSuggestedKeywords(projectId: string): Promise<string[]> {
  return httpClient.get<string[]>(`/api/v1/projects/${projectId}/news/suggested-keywords`)
}

export async function selectNewsItem(
  projectId: string,
  newsItemId: string,
  isSelected: boolean,
): Promise<void> {
  await httpClient.put(`/api/v1/projects/${projectId}/news/items/${newsItemId}/select`, { isSelected })
}

export async function deepAnalyzeNews(
  projectId: string,
  newsItemId: string,
  operationId?: string,
): Promise<NewsDeepAnalysisDto> {
  const query = operationId ? `?operationId=${operationId}` : ''
  return httpClient.post<NewsDeepAnalysisDto>(
    `/api/v1/projects/${projectId}/news/items/${newsItemId}/deep-analyze${query}`,
    {},
  )
}

export async function deleteNewsItem(projectId: string, newsItemId: string): Promise<void> {
  await httpClient.delete(`/api/v1/projects/${projectId}/news/items/${newsItemId}`)
}
