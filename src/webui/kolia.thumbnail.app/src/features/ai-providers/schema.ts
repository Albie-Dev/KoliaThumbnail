import { z } from 'zod'

export const createAIProviderSchema = z.object({
  name: z.string().min(1, 'Tên không được để trống').min(2, 'Tên phải có ít nhất 2 ký tự'),
  shortName: z.string().min(1, 'Mã không được để trống').min(1, 'Mã phải có ít nhất 1 ký tự'),
  imageUrl: z.string().optional().nullable(),
})

export type CreateAIProviderInput = z.infer<typeof createAIProviderSchema>

export const updateAIProviderSchema = createAIProviderSchema

export type UpdateAIProviderInput = z.infer<typeof updateAIProviderSchema>
