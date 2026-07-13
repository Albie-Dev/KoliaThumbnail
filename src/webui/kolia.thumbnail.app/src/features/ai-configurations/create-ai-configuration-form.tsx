import { forwardRef, useImperativeHandle, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { createAIProviderConfiguration } from './api'
import { createAIProviderConfigurationSchema, type CreateAIProviderConfigurationInput } from './schema'
import { Input } from '../../components/ui/input'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { ApiError } from '../../lib/api/api-error'
import { getAIProvidersWithPaging, type AIProviderBaseDto } from '../ai-providers/api'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { CheckboxField } from '../../components/ui/checkbox-field'
import type { PagedRequestParams } from '../../types/paging.types';
import type z from 'zod';
import { FormSection } from '../../components/ui/form-section'
import { Eye, EyeOff } from 'lucide-react'
import { Textarea } from '../../components/ui/textarea'

interface CreateAIProviderConfigurationFormProps {
    onClose?: () => void
}

export interface CreateAIProviderConfigurationFormHandle {
    submit: () => Promise<void>
    isSubmitting: boolean
}

export const CreateAIProviderConfigurationForm =
    forwardRef<
        CreateAIProviderConfigurationFormHandle,
        CreateAIProviderConfigurationFormProps
    >(({ onClose }, ref) => {
        const queryClient = useQueryClient()

        const [isSubmitting, setIsSubmitting] = useState(false);

        const [provider, setProvider] = useState<AIProviderBaseDto | null>(null);

        type FormValues = z.input<typeof createAIProviderConfigurationSchema>;

        const [showApiKey, setShowApiKey] = useState(false);

        const {
            control,
            register,
            handleSubmit,
            formState: { errors },
            setError,
            reset,
            watch,
            setValue,
        } = useForm<FormValues>({
            resolver: zodResolver(
                createAIProviderConfigurationSchema,
            ),
            defaultValues: {
                name: '',
                description: '',
                apiKey: '',
                apiVersion: '',
                timeoutSeconds: 120,
                retryCount: 3,
                priority: 0,
                isEnabled: true,
                isDefault: false,
                extraSettingsJson: '',
                aiProviderId: '',
            },
        })

        const { mutateAsync } = useMutation({
            mutationFn: (
                data: CreateAIProviderConfigurationInput,
            ) => createAIProviderConfiguration(data),

            onError: (error) => {
                if (
                    error instanceof ApiError &&
                    error.isValidationError
                ) {
                    error.errors?.forEach(
                        (validationError) => {
                            setError(
                                validationError.property as keyof CreateAIProviderConfigurationInput,
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

        const onSubmit = async (
            data: FormValues,
        ) => {
            setIsSubmitting(true)

            try {
                await mutateAsync(data as CreateAIProviderConfigurationInput)
                toast.success('Tạo cấu hình AI thành công!')
                onClose?.()
                reset()
            } catch {
                // Global mutation cache đã hiển thị toast.error cho business error
            } finally {
                setIsSubmitting(false)
            }
        }

        useImperativeHandle(ref, () => ({
            submit: handleSubmit(onSubmit),
            isSubmitting,
        }))

        return (
            <form className="space-y-8">
                <FormSection
                    title="Thông tin cơ bản"
                    description="Tên và mô tả để nhận diện cấu hình này"
                    collapsible
                    defaultOpen>
                    <FormGroup>
                        <FormLabel htmlFor="name" required>
                            Tên cấu hình
                        </FormLabel>
                        <Input id="name" placeholder="VD: Production, Staging, GPT-4 Backup" {...register('name')} />
                        <FormField error={errors.name?.message} />
                    </FormGroup>

                    <FormGroup>
                        <FormLabel htmlFor="description">Mô tả</FormLabel>
                        <Textarea
                            id="description"
                            rows={3}
                            placeholder="Mô tả ngắn về mục đích sử dụng của cấu hình này..."
                            {...register('description')}
                        />
                        <FormField error={errors.description?.message} />
                    </FormGroup>

                    <FormGroup>
                        <FormLabel required>Nhà cung cấp AI</FormLabel>

                        <Controller
                            control={control}
                            name="aiProviderId"
                            render={({ field }) => (
                                <SelectDropdown<AIProviderBaseDto>
                                    value={provider}
                                    onChange={(item: AIProviderBaseDto | null) => {
                                        setProvider(item)
                                        field.onChange(item?.id ?? '')
                                    }}
                                    fetchData={async (params: PagedRequestParams) => {
                                        const result = await getAIProvidersWithPaging(params)
                                        return {
                                            items: result.items,
                                            pageInfo: {
                                                pageNumber: result.pageNumber,
                                                pageSize: result.pageSize,
                                                totalPages: result.totalPages,
                                                totalRecords: result.totalCount,
                                                hasPreviousPage: result.pageNumber > 1,
                                                hasNextPage: result.pageNumber < result.totalPages,
                                            },
                                        }
                                    }}
                                    getOptionId={(x: AIProviderBaseDto) => x.id}
                                    getOptionLabel={(x: AIProviderBaseDto) => x.name}
                                    placeholder="Chọn AI Provider..."
                                    searchPlaceholder="Tìm AI Provider..."
                                    renderValue={(item: AIProviderBaseDto) => (
                                        <div className="flex items-center gap-2">
                                            {item.imageUrl ? (
                                                <img src={item.imageUrl} className="h-6 w-6 rounded" />
                                            ) : (
                                                <div className="flex h-6 w-6 items-center justify-center rounded bg-slate-100 text-[10px] font-semibold">
                                                    {item.shortName.substring(0, 2).toUpperCase()}
                                                </div>
                                            )}
                                            <span>{item.name}</span>
                                        </div>
                                    )}
                                    renderOption={(item: AIProviderBaseDto) => (
                                        <div className="flex items-center gap-3">
                                            {item.imageUrl ? (
                                                <img src={item.imageUrl} className="h-8 w-8 rounded" />
                                            ) : (
                                                <div className="flex h-8 w-8 items-center justify-center rounded bg-slate-100 text-xs font-semibold">
                                                    {item.shortName.substring(0, 2).toUpperCase()}
                                                </div>
                                            )}
                                            <div>
                                                <div className="font-medium">{item.name}</div>
                                                <div className="text-xs text-muted-foreground">{item.shortName}</div>
                                            </div>
                                        </div>
                                    )}
                                />
                            )}
                        />
                        <FormField error={errors.aiProviderId?.message} />
                    </FormGroup>
                </FormSection>

                <FormSection 
                    title="Kết nối API"
                    description="Thông tin xác thực và endpoint đến nhà cung cấp"
                    collapsible
                    defaultOpen>
                    <FormGroup>
                        <FormLabel htmlFor="apiKey" required>
                            API Key
                        </FormLabel>
                        <div className="relative">
                            <Input
                                id="apiKey"
                                type={showApiKey ? 'text' : 'password'}
                                placeholder="sk-..."
                                className="pr-10 font-mono text-sm"
                                {...register('apiKey')}
                            />
                            <button
                                type="button"
                                onClick={() => setShowApiKey((v) => !v)}
                                className="absolute right-2.5 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
                                tabIndex={-1}
                            >
                                {showApiKey ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                            </button>
                        </div>
                        <FormField error={errors.apiKey?.message} />
                    </FormGroup>

                    <FormGroup>
                        <FormLabel htmlFor="apiVersion">API Version</FormLabel>
                            <Input id="apiVersion" placeholder="VD: 2024-02-01" {...register('apiVersion')} />
                            <FormField error={errors.apiVersion?.message} />
                    </FormGroup>
                </FormSection>

                <FormSection title="Trạng thái"
                    collapsible
                    defaultOpen>
                    <div className="flex flex-col gap-3">
                        <FormGroup>
                            <CheckboxField
                                id="isEnabled"
                                label="Kích hoạt"
                                description="Cấu hình sẽ được sử dụng ngay khi lưu"
                                checked={watch('isEnabled')}
                                onCheckedChange={(checked) => setValue('isEnabled', checked)}
                            />
                            <FormField error={errors.isEnabled?.message} />
                        </FormGroup>

                        <FormGroup>
                            <CheckboxField
                                id="isDefault"
                                label="Đặt làm mặc định"
                                description="Dùng cấu hình này khi không chỉ định cụ thể"
                                checked={watch('isDefault')}
                                onCheckedChange={(checked) => setValue('isDefault', checked)}
                            />
                            <FormField error={errors.isDefault?.message} />
                        </FormGroup>
                    </div>
                </FormSection>

                <FormSection
                    title="Cấu hình nâng cao"
                    description="Timeout, retry và thứ tự ưu tiên khi failover"
                    collapsible
                    defaultOpen={false}
                >
                    <div className="grid grid-cols-2 gap-4">
                        <FormGroup>
                            <FormLabel htmlFor="timeoutSeconds" required>
                                Timeout (giây)
                            </FormLabel>
                            <Input
                                id="timeoutSeconds"
                                type="number"
                                min={1}
                                placeholder="120"
                                {...register('timeoutSeconds', { valueAsNumber: true })}
                            />
                            <FormField error={errors.timeoutSeconds?.message} />
                        </FormGroup>

                        <FormGroup>
                            <FormLabel htmlFor="retryCount" required>
                                Retry Count
                            </FormLabel>
                            <Input
                                id="retryCount"
                                type="number"
                                min={0}
                                placeholder="3"
                                {...register('retryCount', { valueAsNumber: true })}
                            />
                            <FormField error={errors.retryCount?.message} />
                        </FormGroup>

                        <FormGroup>
                            <FormLabel htmlFor="priority" required>
                                Priority
                            </FormLabel>
                            <Input
                                id="priority"
                                type="number"
                                min={0}
                                placeholder="0"
                                {...register('priority', { valueAsNumber: true })}
                            />
                            <FormField error={errors.priority?.message} />
                        </FormGroup>
                    </div>

                    <FormGroup>
                        <FormLabel htmlFor="extraSettingsJson">Extra Settings JSON</FormLabel>
                        <Textarea
                            id="extraSettingsJson"
                            rows={4}
                            placeholder={'{\n  "organizationId": "org_...",\n  "proxy": null\n}'}
                            className="font-mono text-xs"
                            {...register('extraSettingsJson')}
                        />
                        <FormField error={errors.extraSettingsJson?.message} />
                    </FormGroup>
                </FormSection>
            </form>
        )
    })

CreateAIProviderConfigurationForm.displayName =
    'CreateAIProviderConfigurationForm'