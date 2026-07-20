import { z } from 'zod'

export const createGoogleServiceAccountSchema = z.object({
  name: z.string().min(1, 'Tên không được để trống').min(2, 'Tên phải có ít nhất 2 ký tự'),
  description: z.string().optional().nullable(),
  credentialJson: z.string().min(1, 'Vui lòng nhập JSON credential'),
  scopes: z.string().optional().nullable(),
})

export type CreateGoogleServiceAccountInput = z.infer<typeof createGoogleServiceAccountSchema>

export const updateGoogleServiceAccountSchema = z.object({
  name: z.string().min(1, 'Tên không được để trống').min(2, 'Tên phải có ít nhất 2 ký tự'),
  description: z.string().optional().nullable(),
  credentialJson: z.string().optional().nullable(),
  scopes: z.string().optional().nullable(),
  isEnabled: z.boolean(),
})

export type UpdateGoogleServiceAccountInput = z.infer<typeof updateGoogleServiceAccountSchema>
