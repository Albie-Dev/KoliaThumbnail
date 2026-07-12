import { z } from 'zod'

export const createAIConfigurationSchema = z.object({
    name: z
        .string()
        .trim()
        .min(1, 'Tên không được để trống')
        .min(2, 'Tên phải có ít nhất 2 ký tự'),

    description: z
        .string()
        .optional()
        .nullable(),

    apiKey: z
        .string()
        .trim()
        .min(1, 'API Key không được để trống'),

    apiVersion: z
        .string()
        .optional()
        .nullable(),

    timeoutSeconds: z
        .number()
        .int('Timeout phải là số nguyên')
        .min(1, 'Timeout phải lớn hơn 0'),

    retryCount: z
        .number()
        .int('Retry Count phải là số nguyên')
        .min(0, 'Retry Count không được nhỏ hơn 0'),

    priority: z
        .number()
        .int('Priority phải là số nguyên')
        .min(0, 'Priority không được nhỏ hơn 0'),

    isEnabled: z.boolean(),

    isDefault: z.boolean(),

    extraSettingsJson: z
        .string()
        .optional()
        .nullable(),

    aiProviderId: z.string().check(
        z.uuid({
            error: 'AI Provider không hợp lệ',
        }),
    ),
})

export type CreateAIConfigurationInput = z.infer<
    typeof createAIConfigurationSchema
>

export const updateAIConfigurationSchema = createAIConfigurationSchema.extend({
    apiKey: z.string().trim().optional().default(''),
})

export type UpdateAIConfigurationInput = z.infer<
    typeof updateAIConfigurationSchema
>