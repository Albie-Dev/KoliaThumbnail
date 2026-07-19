import { httpClient } from '../../lib/api/http-client'
import type { CTitleStyle } from '../../types/enums/pipeline.enums'

export interface VideoTitleOptionDto {
  id: string
  videoTitleRequestId: string
  generationRound: number
  content: string
  isSelected: boolean
}

export interface VideoTitleFeedbackDto {
  id: string
  feedbackText: string
  appliedToRound: number
}

export interface VideoTitleRequestDto {
  id: string
  projectId: string
  requestedTitleCount: number
  style: number
  keywordsRaw: string
  builtPromptText?: string | null
  generationRound: number
  creationTime?: string | null
  options: VideoTitleOptionDto[]
  feedbacks: VideoTitleFeedbackDto[]
}

export async function getVideoTitles(projectId: string): Promise<VideoTitleRequestDto[]> {
  return httpClient.get<VideoTitleRequestDto[]>(`/api/v1/projects/${projectId}/video-titles`)
}

export async function generateVideoTitle(
  projectId: string,
  data: {
    selectedThumbnailIds: string[]
    selectedNewsItemIds: string[]
    style: CTitleStyle
    keywordsRaw: string
    requestedCount: number
  },
): Promise<VideoTitleRequestDto> {
  return httpClient.post<VideoTitleRequestDto>(
    `/api/v1/projects/${projectId}/video-titles/generate`,
    data,
  )
}

export async function regenerateVideoTitle(
  projectId: string,
  requestId: string,
): Promise<VideoTitleRequestDto> {
  return httpClient.post<VideoTitleRequestDto>(
    `/api/v1/projects/${projectId}/video-titles/requests/${requestId}/regenerate`,
    {},
  )
}

export async function regenerateWithFeedback(
  projectId: string,
  requestId: string,
  feedbackText: string,
): Promise<VideoTitleRequestDto> {
  return httpClient.post<VideoTitleRequestDto>(
    `/api/v1/projects/${projectId}/video-titles/requests/${requestId}/feedback`,
    { feedbackText },
  )
}

export async function selectVideoTitleOption(
  projectId: string,
  optionId: string,
  isSelected: boolean,
): Promise<void> {
  await httpClient.put(
    `/api/v1/projects/${projectId}/video-titles/options/${optionId}/select`,
    { isSelected },
  )
}
