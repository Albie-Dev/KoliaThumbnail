import { z } from 'zod'
import { CSocialMediaProviderType } from './social-media-provider-type'

const SOCIAL_MEDIA_PROVIDER_TYPE_VALUES = Object.values(CSocialMediaProviderType) as [number, ...number[]]

export const createSocialMediaProviderSchema = z.object({
  name: z.string().min(1, 'Tên không được để trống').min(2, 'Tên phải có ít nhất 2 ký tự'),
  shortName: z.string().min(1, 'Mã không được để trống').min(1, 'Mã phải có ít nhất 1 ký tự'),
  providerType: z.number().refine(
    (val) => (SOCIAL_MEDIA_PROVIDER_TYPE_VALUES as readonly number[]).includes(val),
    { message: 'Loại nhà cung cấp AI không hợp lệ' }
  ),
  imageUrl: z.string().optional().nullable(),
  baseUrl: z.string().min(1, 'Base URL không được để trống').url('Base URL không hợp lệ'),
});

export type CreateSocialMediaProviderInput = z.infer<typeof createSocialMediaProviderSchema>

export const updateSocialMediaProviderSchema = createSocialMediaProviderSchema

export type UpdateSocialMediaProviderInput = z.infer<typeof updateSocialMediaProviderSchema>
