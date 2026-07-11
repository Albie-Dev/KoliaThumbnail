import { forwardRef, useEffect, useImperativeHandle } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import type { z } from 'zod'

import {
    updateAIConfiguration,
    type AIConfigurationBaseDto,
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
    configuration: AIConfigurationBaseDto
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
                apiKey: configuration.apiKey,
                baseUrl: configuration.baseUrl,
                endpoint:
                    configuration.endpoint ?? '',
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
                apiKey: configuration.apiKey,
                baseUrl: configuration.baseUrl,
                endpoint: configuration.endpoint ?? '',
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

                    toast.warning(
                        'Vui lòng kiểm tra lại thông tin đã nhập.',
                    )
                }
            },

            onSuccess: () => {
                toast.success(
                    'Cập nhật cấu hình AI thành công!',
                )

                onClose?.()

                reset()

                queryClient.invalidateQueries({
                    queryKey: ['ai-configurations'],
                })
            },
        })

        const onSubmit = async (data: FormValues) => {
            await toast.promise(
                mutateAsync({
                    id: configuration.id,
                    ...data,
                }),
                {
                    loading: 'Đang cập nhật...',
                    success: 'Cập nhật thành công!',
                    error: (error: Error) =>
                        error.message || 'Có lỗi xảy ra',
                },
            )
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

                    <Input
                        id="edit-apiKey"
                        {...register('apiKey')}
                    />

                    <FormField
                        error={errors.apiKey?.message}
                    />
                </FormGroup>

                <FormGroup>
                    <FormLabel htmlFor="edit-baseUrl">
                        Base URL
                    </FormLabel>

                    <Input
                        id="edit-baseUrl"
                        {...register('baseUrl')}
                    />

                    <FormField
                        error={errors.baseUrl?.message}
                    />
                </FormGroup>

                <FormGroup>
                    <FormLabel htmlFor="edit-endpoint">
                        Endpoint
                    </FormLabel>

                    <Input
                        id="edit-endpoint"
                        {...register('endpoint')}
                    />

                    <FormField
                        error={
                            errors.endpoint?.message
                        }
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