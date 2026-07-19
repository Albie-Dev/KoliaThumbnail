import { httpClient } from '../../lib/api/http-client'
import type { CMarketScope, CNewsTimeRange, CNewsCountFilter } from '../../types/enums/pipeline.enums'

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
  relevanceLevel: number
  isSelectedByTeam: boolean
  suggestedKeywordsForThumbnail?: string | null
  hasDeepAnalysis: boolean
  keywordBatchGroup?: string | null
}

export interface NewsSearchResultDto {
  items: NewsItemDto[]
  totalCount: number
  selectedCount: number
  scannedSourceCount: number
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

// ── API functions ─────────────────────────────────────────────────────────

export async function getNews(projectId: string): Promise<NewsItemDto[]> {
  return httpClient.get<NewsItemDto[]>(`/api/v1/projects/${projectId}/news`)
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
): Promise<NewsSearchResultDto> {
  return httpClient.post<NewsSearchResultDto>(`/api/v1/projects/${projectId}/news/search`, data)
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
): Promise<NewsDeepAnalysisDto> {
  return httpClient.post<NewsDeepAnalysisDto>(
    `/api/v1/projects/${projectId}/news/items/${newsItemId}/deep-analyze`,
    {},
  )
}

export async function deleteNewsItem(projectId: string, newsItemId: string): Promise<void> {
  await httpClient.delete(`/api/v1/projects/${projectId}/news/items/${newsItemId}`)
}
