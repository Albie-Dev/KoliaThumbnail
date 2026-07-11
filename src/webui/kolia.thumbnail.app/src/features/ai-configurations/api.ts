import { httpClient } from '../../lib/api/http-client'
import { buildPagedQuery } from '../../lib/api/build-paged-query'
import type {
    BackendPagedResponse,
    PagedRequestParams,
    PagedResult,
} from '../../types/paging.types'
import type { CreateAIConfigurationInput } from './schema'

export interface AIConfigurationBaseDto {
    id: string

    name: string
    description?: string | null

    apiKey: string
    baseUrl: string
    endpoint?: string | null
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

export interface AIConfigurationDetailDto extends AIConfigurationBaseDto {
    aiProviderShortName: string
    aiProviderName: string
    aiProviderLogo?: string | null

    deletionTime?: string | null
}

export interface AIConfigurationPagedRequest
    extends PagedRequestParams {
    includeDeleted?: boolean
    deletedOnly?: boolean
}

function toPagedResult(
    payload: BackendPagedResponse<AIConfigurationDetailDto>,
): PagedResult<AIConfigurationDetailDto> {
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
export async function fetchAIConfigurations(
    params: AIConfigurationPagedRequest,
): Promise<PagedResult<AIConfigurationDetailDto>> {
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
            BackendPagedResponse<AIConfigurationDetailDto>
        >(`/api/v1/ai-configurations/paging?${query.toString()}`)

    return toPagedResult(response)
}

/**
 * Get by id
 */
export async function getAIConfigurationById(
    id: string,
): Promise<AIConfigurationDetailDto> {
    return httpClient.get<AIConfigurationDetailDto>(
        `/api/v1/ai-configurations/${id}`,
    )
}

/**
 * Create
 */
export async function createAIConfiguration(
    data: CreateAIConfigurationInput,
): Promise<AIConfigurationDetailDto> {
    return httpClient.post<AIConfigurationDetailDto>(
        '/api/v1/ai-configurations',
        {
            name: data.name,
            description: data.description || null,

            apiKey: data.apiKey,
            baseUrl: data.baseUrl,
            endpoint: data.endpoint || null,
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

export interface UpdateAIConfigurationInput
    extends CreateAIConfigurationInput {
    id: string
}

/**
 * Update
 */
export async function updateAIConfiguration(
    data: UpdateAIConfigurationInput,
): Promise<AIConfigurationDetailDto> {
    return httpClient.put<AIConfigurationDetailDto>(
        `/api/v1/ai-configurations/${data.id}`,
        {
            name: data.name,
            description: data.description || null,

            apiKey: data.apiKey,
            baseUrl: data.baseUrl,
            endpoint: data.endpoint || null,
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
export async function setDefaultAIConfiguration(
    id: string,
): Promise<AIConfigurationDetailDto> {
    return httpClient.patch<AIConfigurationDetailDto>(
        `/api/v1/ai-configurations/${id}/set-default`,
    )
}

/**
 * Delete
 */
export async function deleteAIConfiguration(
    id: string,
): Promise<void> {
    await httpClient.delete(`/api/v1/ai-configurations/${id}`)
}