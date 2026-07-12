import { forwardRef, useEffect, useImperativeHandle } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import type { z } from 'zod'

import {
    updateAIConfiguration,
    type AIConfigurationDetailDto,
    type UpdateAIConfigurationInput,
} from './api'
import { updateAIConfigurationSchema } from './schema'

import { Input } from '../../components/ui/input'
import {
    FormField,
    FormGroup,
    FormLabel,
} from '../../components/ui/form'

import { ApiError } from '../../lib/api/api-error'
import { Checkbox } from '../../components/ui/checkbox';

type FormValues = z.input<typeof updateAIConfigurationSchema>;

interface EditAIConfigurationFormProps {
    configuration: AIConfigurationDetailDto
    onClose?: () => void
}

export interface EditAIConfigurationFormHandle {
    submit: () => Promise<void>
    isSubmitting: boolean
}

export const EditAIConfigurationForm =
    forwardRef<
        EditAIConfigurationFormHandle,
        EditAIConfigurationFormProps
    >(({ configuration, onClose }, ref) => {
        const queryClient = useQueryClient();

        const {
            control,
            register,
            handleSubmit,
            formState: { errors, isSubmitting },
            setError,
            reset,
        } = useForm<FormValues>({
            resolver: zodResolver(
                updateAIConfigurationSchema,
            ),
            defaultValues: {
                name: configuration.name,
                description:
                    configuration.description ?? '',
                apiKey: '',
                apiVersion:
                    configuration.apiVersion ?? '',
                timeoutSeconds:
                    configuration.timeoutSeconds,
                retryCount:
                    configuration.retryCount,
                priority: configuration.priority,
                isEnabled:
                    configuration.isEnabled,
                isDefault:
                    configuration.isDefault,
                extraSettingsJson:
                    configuration.extraSettingsJson ??
                    '',
                aiProviderId:
                    configuration.aiProviderId,
            },
        });

        useEffect(() => {
            reset({
                name: configuration.name,
                description: configuration.description ?? '',
                apiKey: '',
                apiVersion: configuration.apiVersion ?? '',
                timeoutSeconds: configuration.timeoutSeconds,
                retryCount: configuration.retryCount,
                priority: configuration.priority,
                isEnabled: configuration.isEnabled,
                isDefault: configuration.isDefault,
                extraSettingsJson:
                    configuration.extraSettingsJson ?? '',
                aiProviderId:
                    configuration.aiProviderId,
            })
        }, [configuration, reset]);

        const { mutateAsync } = useMutation({
            mutationFn: updateAIConfiguration,

            onError: (error) => {
                if (
                    error instanceof ApiError &&
                    error.isValidationError
                ) {
                    error.errors?.forEach(
                        (validationError) => {
                            setError(
                                validationError.property as keyof FormValues,
                                {
                                    message:
                                        validationError.message,
                                },
                            )
                        },
                    )
                }
            },

            onSuccess: () => {
                queryClient.invalidateQueries({
                    queryKey: ['ai-configurations'],
                })
            },

        })

        const onSubmit = async (data: FormValues) => {
            try {
                await mutateAsync({
                    id: configuration.id,
                    ...data,
                    apiKey: data.apiKey ?? '',
                } as UpdateAIConfigurationInput)
                toast.success('Cập nhật cấu hình AI thành công!')
                onClose?.()
                reset()
            } catch {
                // Global mutation cache đã hiển thị toast.error cho business error
            }
        };

        useImperativeHandle(ref, () => ({
            submit: handleSubmit(onSubmit),
            isSubmitting,
        }))

        return (
            <form className="space-y-4">
                <FormGroup>
                    <FormLabel htmlFor="edit-name">
                        Tên cấu hình
                    </FormLabel>

                    <Input
                        id="edit-name"
                        {...register('name')}
                    />

                    <FormField
                        error={errors.name?.message}
                    />
                </FormGroup>

                <FormGroup>
                    <FormLabel htmlFor="edit-description">
                        Mô tả
                    </FormLabel>

                    <Input
                        id="edit-description"
                        {...register('description')}
                    />

                    <FormField
                        error={
                            errors.description?.message
                        }
                    />
                </FormGroup>

                <FormGroup>
                    <FormLabel htmlFor="edit-apiKey">
                        API Key
                    </FormLabel>

                    <div className="space-y-1.5">
                        <p className="text-xs text-slate-400 font-mono">{configuration.apiKeyMasked}</p>
                        <Input
                            id="edit-apiKey"
                            placeholder="Để trống nếu không đổi key"
                            {...register('apiKey')}
                        />
                    </div>

                    <FormField
                        error={errors.apiKey?.message}
                    />
                </FormGroup>

                <FormGroup>
                    <FormLabel htmlFor="edit-apiVersion">
                        API Version
                    </FormLabel>

                    <Input
                        id="edit-apiVersion"
                        {...register('apiVersion')}
                    />

                    <FormField
                        error={
                            errors.apiVersion?.message
                        }
                    />
                </FormGroup>

                <FormGroup>
                    <FormLabel htmlFor="edit-timeout">
                        Timeout (giây)
                    </FormLabel>

                    <Input
                        id="edit-timeout"
                        type="number"
                        {...register('timeoutSeconds', {
                            valueAsNumber: true,
                        })}
                    />

                    <FormField
                        error={
                            errors.timeoutSeconds
                                ?.message
                        }
                    />
                </FormGroup>

                <FormGroup>
                    <FormLabel htmlFor="edit-retry">
                        Retry Count
                    </FormLabel>

                    <Input
                        id="edit-retry"
                        type="number"
                        {...register('retryCount', {
                            valueAsNumber: true,
                        })}
                    />

                    <FormField
                        error={
                            errors.retryCount?.message
                        }
                    />
                </FormGroup>

                <FormGroup>
                    <FormLabel htmlFor="edit-priority">
                        Priority
                    </FormLabel>

                    <Input
                        id="edit-priority"
                        type="number"
                        {...register('priority', {
                            valueAsNumber: true,
                        })}
                    />

                    <FormField
                        error={
                            errors.priority?.message
                        }
                    />
                </FormGroup>

                <FormGroup>
                    <FormLabel htmlFor="edit-extra">
                        Extra Settings JSON
                    </FormLabel>

                    <Input
                        id="edit-extra"
                        {...register(
                            'extraSettingsJson',
                        )}
                    />

                    <FormField
                        error={
                            errors.extraSettingsJson
                                ?.message
                        }
                    />
                </FormGroup>

                <FormGroup>
                    <div className="flex items-center gap-2">
                        <Controller
                            control={control}
                            name="isEnabled"
                            render={({ field }) => (
                                <Checkbox
                                    checked={field.value}
                                    onCheckedChange={(checked) =>
                                        field.onChange(!!checked)
                                    }
                                />
                            )}
                        />

                        <FormLabel>
                            Kích hoạt
                        </FormLabel>
                    </div>

                    <FormField
                        error={
                            errors.isEnabled?.message
                        }
                    />
                </FormGroup>

                <FormGroup>
                    <div className="flex items-center gap-2">
                        <Controller
                            control={control}
                            name="isDefault"
                            render={({ field }) => (
                                <Checkbox
                                    checked={field.value}
                                    onCheckedChange={(checked) =>
                                        field.onChange(!!checked)
                                    }
                                />
                            )}
                        />
                        <FormLabel>Mặc định</FormLabel>
                    </div>

                    <FormField error={errors.isDefault?.message} />
                </FormGroup>
            </form>
        )
    })

EditAIConfigurationForm.displayName =
    'EditAIConfigurationForm'