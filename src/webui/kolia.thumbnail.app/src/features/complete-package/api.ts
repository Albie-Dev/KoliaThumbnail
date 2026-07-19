import { httpClient } from '../../lib/api/http-client'

export interface CompletePackageTitleDto {
  videoTitleOptionId: string
  content: string
}

export interface CompletePackageDto {
  id: string
  projectId: string
  selectedThumbnailId: string
  thumbnailImageUrl: string
  displayTextSnapshot: string
  confirmedAt?: string | null
  selectedTitles: CompletePackageTitleDto[]
}

export async function getCompletePackages(projectId: string): Promise<CompletePackageDto[]> {
  return httpClient.get<CompletePackageDto[]>(`/api/v1/projects/${projectId}/complete-packages`)
}

export async function confirmCompletePackage(
  projectId: string,
  data: { selectedThumbnailId: string; selectedTitleOptionIds: string[] },
): Promise<CompletePackageDto> {
  return httpClient.post<CompletePackageDto>(
    `/api/v1/projects/${projectId}/complete-packages`,
    data,
  )
}
