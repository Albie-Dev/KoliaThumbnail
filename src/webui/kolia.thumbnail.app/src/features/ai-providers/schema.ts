import { z } from 'zod'

export const endpointSchema = z.object({
  type: z.number().min(1, 'Loại endpoint không hợp lệ'),
  route: z.string().min(1, 'Route không được để trống'),
  jsonResponse: z.string().min(1, 'JSON response mẫu không được để trống'),
  jsonError: z.string().min(1, 'JSON error mẫu không được để trống'),
  jsonRequest: z.string().min(1, 'JSON request mẫu không được để trống'),
})

export type EndpointInput = z.infer<typeof endpointSchema>

export const createAIProviderSchema = z.object({
  name: z.string().min(1, 'Tên không được để trống').min(2, 'Tên phải có ít nhất 2 ký tự'),
  shortName: z.string().min(1, 'Mã không được để trống').min(1, 'Mã phải có ít nhất 1 ký tự'),
  imageUrl: z.string().optional().nullable(),
  baseUrl: z.string().min(1, 'Base URL không được để trống').url('Base URL không hợp lệ'),
  endpoints: z.array(endpointSchema).min(1, 'Ít nhất một endpoint'),
})

export type CreateAIProviderInput = z.infer<typeof createAIProviderSchema>

export const updateAIProviderSchema = createAIProviderSchema

export type UpdateAIProviderInput = z.infer<typeof updateAIProviderSchema>
