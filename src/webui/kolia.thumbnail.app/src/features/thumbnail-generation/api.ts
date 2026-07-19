import { httpClient } from '../../lib/api/http-client'
import type { CThumbnailEditTool } from '../../types/enums/pipeline.enums'

export interface GeneratedThumbnailDto {
  id: string
  generatedThumbnailSetId: string
  variantIndex: number
  parentGeneratedThumbnailId?: string | null
  versionNumber: number
  imageUrl: string
  displayTextSnapshot: string
  characterSnapshotName?: string | null
  lastEditTool?: number | null
  isApproved: boolean
  approvedAt?: string | null
  wasDownloaded: boolean
  isPushedToTitleStep: boolean
}

export interface GeneratedThumbnailSetDto {
  id: string
  thumbnailGenerationRequestId: string
  setIndex: number
  creationTime?: string | null
  variants: GeneratedThumbnailDto[]
}

export async function getThumbnailSets(projectId: string): Promise<GeneratedThumbnailSetDto[]> {
  return httpClient.get<GeneratedThumbnailSetDto[]>(
    `/api/v1/projects/${projectId}/thumbnail-generation`,
  )
}

export async function generateThumbnail(
  projectId: string,
  data: {
    displayTextOptionIds: string[]
    referenceLibraryItemIds: string[]
    characterId?: string
    changesRequestText: string
    ratio: string
    resolution: string
    requestedCount: number
    overridePromptText?: string
  },
): Promise<GeneratedThumbnailSetDto> {
  return httpClient.post<GeneratedThumbnailSetDto>(
    `/api/v1/projects/${projectId}/thumbnail-generation/generate`,
    data,
  )
}

export async function exportThumbnailPrompt(
  projectId: string,
  data: {
    displayTextOptionIds: string[]
    referenceLibraryItemIds: string[]
    characterId?: string
    changesRequestText: string
    ratio: string
    resolution: string
  },
): Promise<string> {
  return httpClient.post<string>(
    `/api/v1/projects/${projectId}/thumbnail-generation/export-prompt`,
    data,
  )
}

export async function editThumbnail(
  projectId: string,
  thumbnailId: string,
  data: {
    editTool: CThumbnailEditTool
    editRequestText: string
    secondaryReferenceLibraryItemId?: string
    secondaryCharacterImageId?: string
  },
): Promise<GeneratedThumbnailDto> {
  return httpClient.post<GeneratedThumbnailDto>(
    `/api/v1/projects/${projectId}/thumbnail-generation/variants/${thumbnailId}/edit`,
    data,
  )
}

export async function approveThumbnail(
  projectId: string,
  thumbnailId: string,
): Promise<void> {
  await httpClient.post(
    `/api/v1/projects/${projectId}/thumbnail-generation/variants/${thumbnailId}/approve`,
    {},
  )
}

export async function pushThumbnailToTitle(
  projectId: string,
  thumbnailId: string,
): Promise<void> {
  await httpClient.post(
    `/api/v1/projects/${projectId}/thumbnail-generation/variants/${thumbnailId}/push-to-title`,
    {},
  )
}

export async function markThumbnailDownloaded(
  projectId: string,
  thumbnailId: string,
): Promise<void> {
  await httpClient.post(
    `/api/v1/projects/${projectId}/thumbnail-generation/variants/${thumbnailId}/mark-downloaded`,
    {},
  )
}
