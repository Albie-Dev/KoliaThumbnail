import { httpClient } from '../../lib/api/http-client'
import { buildPagedQuery } from '../../lib/api/build-paged-query'
import type { BackendPagedResponse, PagedRequestParams, PagedResult } from '../../types/paging.types'

// ── DTOs ──────────────────────────────────────────────────────────────────

export interface AIModelInfo {
  providerType: number
  modelId: string
  displayName: string
  inputTokenLimit: number
  outputTokenLimit: number
  supportedGenerationMethods: string[]
}

export interface AIFunctionConfigSummaryDto {
  id: string
  functionType: number
  model?: string | null
  temperature?: number | null
  maxTokens?: number | null
  primaryProviderName?: string | null
  primaryConfigName?: string | null
  fallbackCount: number
  creationTime: string
  lastModificationTime?: string | null
}

export interface AIFunctionConfigItemDetailDto {
  id: string
  priority: number
  aiProviderId: string
  aiProviderName: string
  aiProviderConfigurationId: string
  aiProviderConfigurationName: string
  model?: string | null
  temperature?: number | null
  maxTokens?: number | null
  isEnabled: boolean
}

export interface AIFunctionConfigDetailDto {
  id: string
  functionType: number
  model?: string | null
  temperature?: number | null
  maxTokens?: number | null
  creationTime: string
  lastModificationTime?: string | null
  items: AIFunctionConfigItemDetailDto[]
}

// ── Update DTOs ────────────────────────────────────────────────────────────

export interface UpdateAIFunctionConfigItemInput {
  id?: string | null
  priority: number
  aiProviderId: string
  aiProviderConfigurationId: string
  model?: string | null
  temperature?: number | null
  maxTokens?: number | null
  isEnabled: boolean
}

export interface UpdateAIFunctionConfigInput {
  model?: string | null
  temperature?: number | null
  maxTokens?: number | null
  items: UpdateAIFunctionConfigItemInput[]
}

// ── Paging request params ─────────────────────────────────────────────────

export interface AIFunctionConfigPagedRequest extends PagedRequestParams {
  includeDeleted?: boolean
  deletedOnly?: boolean
}

function toPagedResult(payload: BackendPagedResponse<AIFunctionConfigSummaryDto>): PagedResult<AIFunctionConfigSummaryDto> {
  return {
    items: payload.items,
    pageNumber: payload.pageInfo.pageNumber,
    pageSize: payload.pageInfo.pageSize,
    totalCount: payload.pageInfo.totalRecords,
    totalPages: payload.pageInfo.totalPages,
  }
}

// ── API functions ──────────────────────────────────────────────────────────

export async function getFunctionConfigsPaging(
  params: AIFunctionConfigPagedRequest,
): Promise<PagedResult<AIFunctionConfigSummaryDto>> {
  const { includeDeleted, deletedOnly, ...paging } = params
  const query = buildPagedQuery({ includeItems: true, includeTotalCount: true, ...paging })
  if (includeDeleted != null) query.append('includeDeleted', String(includeDeleted))
  if (deletedOnly != null) query.append('deletedOnly', String(deletedOnly))
  const response = await httpClient.get<BackendPagedResponse<AIFunctionConfigSummaryDto>>(
    `/api/v1/ai-function-configs/paging?${query.toString()}`,
  )
  return toPagedResult(response)
}

export async function getFunctionConfigById(id: string): Promise<AIFunctionConfigDetailDto> {
  return httpClient.get<AIFunctionConfigDetailDto>(`/api/v1/ai-function-configs/${id}`)
}

export async function updateFunctionConfig(
  id: string,
  data: UpdateAIFunctionConfigInput,
): Promise<AIFunctionConfigDetailDto> {
  return httpClient.put<AIFunctionConfigDetailDto>(`/api/v1/ai-function-configs/${id}`, data)
}

export async function getProviderModels(
  providerId: string,
  configurationId: string,
): Promise<AIModelInfo[]> {
  return httpClient.get<AIModelInfo[]>(
    `/api/v1/ai-function-configs/provider-models?providerId=${providerId}&configurationId=${configurationId}`,
  )
}
