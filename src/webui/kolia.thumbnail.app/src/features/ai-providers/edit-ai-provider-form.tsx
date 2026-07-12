import { forwardRef, useImperativeHandle, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm, useFieldArray } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { Plus, Trash2 } from 'lucide-react'
import { updateAIProvider, type UpdateAIProviderInput } from './api'
import type { AIProviderBaseDto } from './api'
import { updateAIProviderSchema } from './schema'
import { ENDPOINT_TYPE_OPTIONS } from './endpoint-type'
import type { EndpointTypeOption } from './endpoint-type'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { Input } from '../../components/ui/input'
import { Textarea } from '../../components/ui/textarea'
import { Button } from '../../components/ui/button'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { FormSection } from '../../components/ui/form-section'
import { ApiError } from '../../lib/api/api-error'
import type { z } from 'zod'

type FormValues = z.infer<typeof updateAIProviderSchema>

interface EditAIProviderFormProps {
  provider: AIProviderBaseDto
  onClose?: () => void
}

export interface EditAIProviderFormHandle {
  submit: () => Promise<void>
  isSubmitting: boolean
}

export const EditAIProviderForm = forwardRef<EditAIProviderFormHandle, EditAIProviderFormProps>(
  ({ provider, onClose }, ref) => {
    const queryClient = useQueryClient()
    const [isSubmitting, setIsSubmitting] = useState(false)

    const {
      register,
      handleSubmit,
      formState: { errors },
      setError,
      reset,
      control,
      watch,
      setValue,
    } = useForm<FormValues>({
      resolver: zodResolver(updateAIProviderSchema),
      defaultValues: {
        name: provider.name,
        shortName: provider.shortName,
        imageUrl: provider.imageUrl ?? '',
        baseUrl: provider.baseUrl,
        endpoints: provider.endpoints.length > 0
          ? provider.endpoints.map(e => ({
              type: e.type,
              route: e.endpoint,
              jsonResponse: e.jsonResponse,
              jsonError: e.jsonError,
              jsonRequest: e.jsonRequest,
            }))
          : [],
      },
    })

    const { fields, append, remove } = useFieldArray({
      control,
      name: 'endpoints',
    })

    const { mutate } = useMutation({
      mutationFn: (data: FormValues) =>
        updateAIProvider({ ...data, id: provider.id } as UpdateAIProviderInput),
      onError: (error) => {
        if (error instanceof ApiError && error.isValidationError) {
          error.errors?.forEach((validationError) => {
            setError(validationError.property as keyof FormValues, {
              message: validationError.message,
            })
          })
          toast.warning('Vui lòng kiểm tra lại thông tin đã nhập.')
        }
      },
      onSuccess: () => {
        toast.success('Cập nhật nhà cung cấp thành công!')
        onClose?.()
        reset()
        queryClient.invalidateQueries({ queryKey: ['ai-providers'] })
      },
    })

    const onSubmit = async (data: FormValues) => {
      setIsSubmitting(true)
      try {
        await toast.promise(
          new Promise((resolve, reject) => {
            mutate(data, {
              onSuccess: () => resolve(null),
              onError: (error) => reject(error),
            })
          }),
          {
            loading: 'Đang cập nhật…',
            success: 'Cập nhật thành công!',
            error: (error: Error) => error.message || 'Có lỗi xảy ra',
          },
        )
      } finally {
        setIsSubmitting(false)
      }
    }

    useImperativeHandle(ref, () => ({
      submit: handleSubmit(onSubmit),
      isSubmitting,
    }))

    return (
      <form className="space-y-5">
        <FormSection title="Thông tin cơ bản" description="Tên, mã và thông tin chung của nhà cung cấp AI">
          <FormGroup>
            <FormLabel htmlFor="edit-name" required>Tên nhà cung cấp</FormLabel>
            <Input {...register('name')} id="edit-name" placeholder="VD: OpenAI, Google, Microsoft" />
            <FormField error={errors.name?.message} />
          </FormGroup>

          <div className="grid grid-cols-2 gap-4">
            <FormGroup>
              <FormLabel htmlFor="edit-shortName" required>Mã nhà cung cấp</FormLabel>
              <Input {...register('shortName')} id="edit-shortName" placeholder="VD: openai" />
              <FormField error={errors.shortName?.message} />
            </FormGroup>

            <FormGroup>
              <FormLabel htmlFor="edit-imageUrl">URL hình ảnh</FormLabel>
              <Input {...register('imageUrl')} id="edit-imageUrl" placeholder="https://example.com/logo.png" />
              <FormField error={errors.imageUrl?.message} />
            </FormGroup>
          </div>

          <FormGroup>
            <FormLabel htmlFor="edit-baseUrl" required>Base URL</FormLabel>
            <Input {...register('baseUrl')} id="edit-baseUrl" placeholder="https://api.openai.com/v1" />
            <FormField error={errors.baseUrl?.message} />
          </FormGroup>
        </FormSection>

        <FormSection
          title="Cấu hình endpoints"
          description="Danh sách các API endpoint mà nhà cung cấp hỗ trợ"
          collapsible
        >
          <div className="space-y-3">
            <div className="flex items-center justify-end">
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() =>
                  append({
                    type: 2,
                    route: '',
                    jsonResponse: '',
                    jsonError: '',
                    jsonRequest: '',
                  })
                }
              >
                <Plus className="mr-1 h-4 w-4" /> Thêm route
              </Button>
            </div>

            {fields.length === 0 && (
              <p className="py-4 text-center text-sm text-slate-400">
                Chưa có route nào. Nhấn "Thêm route" để bắt đầu.
              </p>
            )}

            {fields.map((field, index) => (
              <div key={field.id} className="space-y-3 rounded-lg border border-slate-200 bg-slate-50/50 p-4">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Endpoint #{index + 1}
                  </span>
                  <Button type="button" variant="ghost" size="icon" onClick={() => remove(index)}>
                    <Trash2 className="h-4 w-4 text-red-400" />
                  </Button>
                </div>

                <div className="grid grid-cols-1 gap-3">
                  <FormGroup>
                    <FormLabel required>Loại</FormLabel>
                    <SelectDropdown<EndpointTypeOption>
                      items={ENDPOINT_TYPE_OPTIONS}
                      getOptionId={(opt) => String(opt.id)}
                      getOptionLabel={(opt) => opt.label}
                      value={ENDPOINT_TYPE_OPTIONS.find((opt) => opt.id === watch(`endpoints.${index}.type`)) ?? null}
                      onChange={(opt) => setValue(`endpoints.${index}.type`, opt?.id ?? 2)}
                      allowSearch={false}
                      placeholder="Chọn loại..."
                    />
                    <FormField error={errors.endpoints?.[index]?.type?.message} />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel htmlFor={`endpoints.${index}.route`} required>Route path</FormLabel>
                    <Input
                      {...register(`endpoints.${index}.route`)}
                      id={`endpoints.${index}.route`}
                      placeholder="/v1/chat/completions"
                    />
                    <FormField error={errors.endpoints?.[index]?.route?.message} />
                  </FormGroup>
                </div>

                <FormGroup>
                  <FormLabel htmlFor={`endpoints.${index}.jsonRequest`} required>
                    JSON Request <span className="text-slate-400">(mẫu)</span>
                  </FormLabel>
                  <Textarea
                    {...register(`endpoints.${index}.jsonRequest`)}
                    value={watch(`endpoints.${index}.jsonRequest`)}
                    id={`endpoints.${index}.jsonRequest`}
                    placeholder='{"model":"...","messages":[...]}'
                    enableViewModes
                  />
                  <FormField error={errors.endpoints?.[index]?.jsonRequest?.message} />
                </FormGroup>

                <div className="grid grid-cols-1 gap-3">
                  <FormGroup>
                    <FormLabel htmlFor={`endpoints.${index}.jsonResponse`} required>
                      JSON Response <span className="text-slate-400">(mẫu)</span>
                    </FormLabel>
                    <Textarea
                      {...register(`endpoints.${index}.jsonResponse`)}
                      value={watch(`endpoints.${index}.jsonResponse`)}
                      id={`endpoints.${index}.jsonResponse`}
                      placeholder='{"id":"...","choices":[...]}'
                      enableViewModes
                    />
                    <FormField error={errors.endpoints?.[index]?.jsonResponse?.message} />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel htmlFor={`endpoints.${index}.jsonError`} required>
                      JSON Error <span className="text-slate-400">(mẫu)</span>
                    </FormLabel>
                    <Textarea
                      {...register(`endpoints.${index}.jsonError`)}
                      value={watch(`endpoints.${index}.jsonError`)}
                      id={`endpoints.${index}.jsonError`}
                      placeholder='{"error":{"message":"..."}}'
                      enableViewModes
                    />
                    <FormField error={errors.endpoints?.[index]?.jsonError?.message} />
                  </FormGroup>
                </div>
              </div>
            ))}

            {errors.endpoints?.root?.message && (
              <FormField error={errors.endpoints.root.message} />
            )}
          </div>
        </FormSection>
      </form>
    )
  },
)

EditAIProviderForm.displayName = 'EditAIProviderForm'
