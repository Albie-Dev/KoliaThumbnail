import { forwardRef, useImperativeHandle, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm, useFieldArray } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { Plus, Trash2 } from 'lucide-react'
import { createAIProvider } from './api'
import { createAIProviderSchema, type CreateAIProviderInput } from './schema'
import { ENDPOINT_TYPE_OPTIONS } from './endpoint-type'
import type { EndpointTypeOption } from './endpoint-type'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { Input } from '../../components/ui/input'
import { Textarea } from '../../components/ui/textarea'
import { Button } from '../../components/ui/button'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { FormSection } from '../../components/ui/form-section'
import { ApiError } from '../../lib/api/api-error'

interface CreateAIProviderFormProps {
  onClose?: () => void
}

export interface CreateAIProviderFormHandle {
  submit: () => Promise<void>
  isSubmitting: boolean
}

export const CreateAIProviderForm = forwardRef<CreateAIProviderFormHandle, CreateAIProviderFormProps>(
  ({ onClose }, ref) => {
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
    } = useForm<CreateAIProviderInput>({
      resolver: zodResolver(createAIProviderSchema),
      defaultValues: {
        name: '',
        shortName: '',
        imageUrl: '',
        baseUrl: '',
        endpoints: [],
      },
    })

    const { fields, append, remove } = useFieldArray({
      control,
      name: 'endpoints',
    })

    const { mutate } = useMutation({
      mutationFn: (data: CreateAIProviderInput) => createAIProvider(data),
      onError: (error) => {
        if (error instanceof ApiError && error.isValidationError) {
          // Map backend validation errors to form fields
          error.errors?.forEach((validationError) => {
            setError(validationError.property as keyof CreateAIProviderInput, {
              message: validationError.message,
            })
          })
          // Toast thông báo về lỗi validation tổng quát
          toast.warning('Vui lòng kiểm tra lại thông tin đã nhập.')
        }
      },
      onSuccess: () => {
        toast.success('Tạo nhà cung cấp thành công!')
        onClose?.()
        reset()
        // Refetch the list
        queryClient.invalidateQueries({ queryKey: ['ai-providers'] })
      },
    })

    const onSubmit = async (data: CreateAIProviderInput) => {
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
            loading: 'Đang tạo nhà cung cấp…',
            success: 'Tạo thành công!',
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
            <FormLabel htmlFor="name" required>Tên nhà cung cấp</FormLabel>
            <Input {...register('name')} id="name" placeholder="VD: OpenAI, Google, Microsoft" />
            <FormField error={errors.name?.message} />
          </FormGroup>

          <div className="grid grid-cols-2 gap-4">
            <FormGroup>
              <FormLabel htmlFor="shortName" required>Mã nhà cung cấp</FormLabel>
              <Input {...register('shortName')} id="shortName" placeholder="VD: openai" />
              <FormField error={errors.shortName?.message} />
            </FormGroup>

            <FormGroup>
              <FormLabel htmlFor="imageUrl">URL hình ảnh</FormLabel>
              <Input {...register('imageUrl')} id="imageUrl" placeholder="https://example.com/logo.png" />
              <FormField error={errors.imageUrl?.message} />
            </FormGroup>
          </div>

          <FormGroup>
            <FormLabel htmlFor="baseUrl" required>Base URL</FormLabel>
            <Input {...register('baseUrl')} id="baseUrl" placeholder="https://api.openai.com/v1" />
            <FormField error={errors.baseUrl?.message} />
          </FormGroup>
        </FormSection>

        <FormSection
          title="Cấu hình routes"
          description="Danh sách các API route mà nhà cung cấp hỗ trợ"
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

CreateAIProviderForm.displayName = 'CreateAIProviderForm'