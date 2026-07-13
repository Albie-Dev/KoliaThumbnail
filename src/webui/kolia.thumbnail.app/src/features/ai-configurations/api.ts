import { httpClient } from '../../lib/api/http-client'
import { buildPagedQuery } from '../../lib/api/build-paged-query'
import type {
    BackendPagedResponse,
    PagedRequestParams,
    PagedResult,
} from '../../types/paging.types'
import type { CreateAIProviderConfigurationInput } from './schema'

export interface AIProviderConfigurationBaseDto {
    id: string

    name: string
    description?: string | null

    apiKey: string
    apiVersion?: string | null

    timeoutSeconds: number
    retryCount: number
    priority: number

    isEnabled: boolean
    isDefault: boolean

    extraSettingsJson?: string | null

    aiProviderId: string

    isDeleted: boolean
    creationTime?: string | null
    lastModificationTime?: string | null
}

export interface AIProviderConfigurationDetailDto extends AIProviderConfigurationBaseDto {
    aiProviderShortName: string
    aiProviderName: string
    aiProviderLogo?: string | null

    apiKeyMasked: string
    totalTokensUsed: number
    lastTokenResetTime?: string | null

    deletionTime?: string | null
}

export interface AIProviderConfigurationPagedRequest
    extends PagedRequestParams {
    includeDeleted?: boolean
    deletedOnly?: boolean
}

function toPagedResult(
    payload: BackendPagedResponse<AIProviderConfigurationDetailDto>,
): PagedResult<AIProviderConfigurationDetailDto> {
    return {
        items: payload.items,
        pageNumber: payload.pageInfo.pageNumber,
        pageSize: payload.pageInfo.pageSize,
        totalCount: payload.pageInfo.totalRecords,
        totalPages: payload.pageInfo.totalPages,
    }
}

/**
 * Get paging
 */
export async function fetchAIProviderConfigurations(
    params: AIProviderConfigurationPagedRequest,
): Promise<PagedResult<AIProviderConfigurationDetailDto>> {
    const {
        includeDeleted,
        deletedOnly,
        ...paging
    } = params

    const query = buildPagedQuery({
        includeItems: true,
        includeTotalCount: true,
        ...paging,
    })

    if (includeDeleted !== undefined) {
        query.set('includeDeleted', String(includeDeleted))
    }

    if (deletedOnly !== undefined) {
        query.set('deletedOnly', String(deletedOnly))
    }

    const response =
        await httpClient.get<
            BackendPagedResponse<AIProviderConfigurationDetailDto>
        >(`/api/v1/ai-configurations/paging?${query.toString()}`)

    return toPagedResult(response)
}

/**
 * Get by id
 */
export async function getAIProviderConfigurationById(
    id: string,
): Promise<AIProviderConfigurationDetailDto> {
    return httpClient.get<AIProviderConfigurationDetailDto>(
        `/api/v1/ai-configurations/${id}`,
    )
}

/**
 * Create
 */
export async function createAIProviderConfiguration(
    data: CreateAIProviderConfigurationInput,
): Promise<AIProviderConfigurationDetailDto> {
    return httpClient.post<AIProviderConfigurationDetailDto>(
        '/api/v1/ai-configurations',
        {
            name: data.name,
            description: data.description || null,

            apiKey: data.apiKey,
            apiVersion: data.apiVersion || null,

            timeoutSeconds: data.timeoutSeconds,
            retryCount: data.retryCount,
            priority: data.priority,

            isEnabled: data.isEnabled,
            isDefault: data.isDefault,

            extraSettingsJson: data.extraSettingsJson || null,

            aiProviderId: data.aiProviderId,
        },
    )
}

export interface UpdateAIProviderConfigurationInput
    extends CreateAIProviderConfigurationInput {
    id: string
}

/**
 * Update
 */
export async function updateAIProviderConfiguration(
    data: UpdateAIProviderConfigurationInput,
): Promise<AIProviderConfigurationDetailDto> {
    return httpClient.put<AIProviderConfigurationDetailDto>(
        `/api/v1/ai-configurations/${data.id}`,
        {
            name: data.name,
            description: data.description || null,

            apiKey: data.apiKey,
            apiVersion: data.apiVersion || null,

            timeoutSeconds: data.timeoutSeconds,
            retryCount: data.retryCount,
            priority: data.priority,

            isEnabled: data.isEnabled,
            isDefault: data.isDefault,

            extraSettingsJson: data.extraSettingsJson || null,

            aiProviderId: data.aiProviderId,
        },
    )
}

/**
 * Set default configuration
 */
export async function setDefaultAIProviderConfiguration(
    id: string,
): Promise<AIProviderConfigurationDetailDto> {
    return httpClient.patch<AIProviderConfigurationDetailDto>(
        `/api/v1/ai-configurations/${id}/set-default`,
    )
}

/**
 * Delete
 */
export async function deleteAIProviderConfiguration(
    id: string,
): Promise<void> {
    await httpClient.delete(`/api/v1/ai-configurations/${id}`)
}