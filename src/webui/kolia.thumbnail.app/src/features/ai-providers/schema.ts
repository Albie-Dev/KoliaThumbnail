import { z } from 'zod'
import { CAIProviderType } from './ai-provider-type'

const AI_PROVIDER_TYPE_VALUES = Object.values(CAIProviderType) as [number, ...number[]]

export const createAIProviderSchema = z.object({
  name: z.string().min(1, 'Tên không được để trống').min(2, 'Tên phải có ít nhất 2 ký tự'),
  shortName: z.string().min(1, 'Mã không được để trống').min(1, 'Mã phải có ít nhất 1 ký tự'),
  providerType: z.number().refine(
    (val) => (AI_PROVIDER_TYPE_VALUES as readonly number[]).includes(val),
    { message: 'Loại nhà cung cấp AI không hợp lệ' }
  ),
  imageUrl: z.string().optional().nullable(),
  baseUrl: z.string().min(1, 'Base URL không được để trống').url('Base URL không hợp lệ'),
});

export type CreateAIProviderInput = z.infer<typeof createAIProviderSchema>

export const updateAIProviderSchema = createAIProviderSchema

export type UpdateAIProviderInput = z.infer<typeof updateAIProviderSchema>
