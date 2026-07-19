import { z } from 'zod'

export const generateThumbnailSchema = z.object({
  displayTextOptionIds: z.array(z.string()).min(1, 'Chọn ít nhất một display text'),
  referenceLibraryItemIds: z.array(z.string()).optional().default([]),
  characterId: z.string().optional(),
  changesRequestText: z.string().min(1, 'Mô tả thay đổi không được để trống'),
  ratio: z.string().min(1, 'Tỷ lệ không được để trống'),
  resolution: z.string().min(1, 'Độ phân giải không được để trống'),
  requestedCount: z.number().min(1, 'Số ảnh phải lớn hơn 0'),
  overridePromptText: z.string().optional(),
})

export type GenerateThumbnailInput = z.infer<typeof generateThumbnailSchema>
