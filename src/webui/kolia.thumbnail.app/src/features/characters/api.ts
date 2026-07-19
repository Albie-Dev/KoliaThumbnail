import { httpClient } from '../../lib/api/http-client'

export interface CharacterSummaryDto {
  id: string
  name: string
  description?: string | null
  primaryImageUrl?: string | null
}

export interface CharacterDto extends CharacterSummaryDto {
  images: CharacterImageDto[]
}

export interface CharacterImageDto {
  id: string
  characterId: string
  imageUrl: string
  expressionLabel?: string | null
  angleLabel?: string | null
  isPrimary: boolean
}

export async function getCharacters(): Promise<CharacterSummaryDto[]> {
  return httpClient.get<CharacterSummaryDto[]>('/api/v1/characters')
}

export async function getCharacterById(id: string): Promise<CharacterDto> {
  return httpClient.get<CharacterDto>(`/api/v1/characters/${id}`)
}

export async function createCharacter(data: { name: string; description?: string }): Promise<CharacterDto> {
  return httpClient.post<CharacterDto>('/api/v1/characters', data)
}

export async function addCharacterImage(
  characterId: string,
  data: { imageUrl: string; expressionLabel?: string; angleLabel?: string; isPrimary: boolean },
): Promise<CharacterImageDto> {
  return httpClient.post<CharacterImageDto>(`/api/v1/characters/${characterId}/images`, data)
}

export async function deleteCharacter(id: string): Promise<void> {
  await httpClient.delete(`/api/v1/characters/${id}`)
}
