import { httpClient } from '../../lib/api/http-client'

export interface DisplayTextOptionDto {
  id: string
  displayTextRequestId: string
  sourceNewsItemId: string
  content: string
  isSelected: boolean
}

export interface DisplayTextRequestDto {
  id: string
  projectId: string
  creationTime?: string | null
  selectedNewsItemIds: string[]
  options: DisplayTextOptionDto[]
}

export async function getDisplayTexts(projectId: string): Promise<DisplayTextRequestDto[]> {
  return httpClient.get<DisplayTextRequestDto[]>(`/api/v1/projects/${projectId}/display-texts`)
}

export async function generateDisplayText(
  projectId: string,
  newsItemIds: string[],
): Promise<DisplayTextRequestDto> {
  return httpClient.post<DisplayTextRequestDto>(
    `/api/v1/projects/${projectId}/display-texts/generate`,
    { newsItemIds },
  )
}

export async function selectDisplayTextOption(
  projectId: string,
  optionId: string,
  isSelected: boolean,
): Promise<void> {
  await httpClient.put(
    `/api/v1/projects/${projectId}/display-texts/options/${optionId}/select`,
    { isSelected },
  )
}
