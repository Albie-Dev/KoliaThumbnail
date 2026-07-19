import { z } from 'zod'

export const createProjectSchema = z.object({
  name: z.string().min(1, 'Tên không được để trống'),
})

export type CreateProjectInput = z.infer<typeof createProjectSchema>

export const renameProjectSchema = z.object({
  name: z.string().min(1, 'Tên không được để trống'),
})

export type RenameProjectInput = z.infer<typeof renameProjectSchema>
