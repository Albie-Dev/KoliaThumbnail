import { forwardRef, useImperativeHandle, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { createAIProvider } from './api'
import { createAIProviderSchema, type CreateAIProviderInput } from './schema'
import { Input } from '../../components/ui/input'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
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
    } = useForm<CreateAIProviderInput>({
      resolver: zodResolver(createAIProviderSchema),
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
      <form className="space-y-4">
        <FormGroup>
          <FormLabel htmlFor="name">Tên nhà cung cấp</FormLabel>
          <Input {...register('name')} id="name" placeholder="VD: OpenAI, Google, Microsoft" />
          <FormField error={errors.name?.message} />
        </FormGroup>

        <FormGroup>
          <FormLabel htmlFor="shortName">Mã nhà cung cấp</FormLabel>
          <Input {...register('shortName')} id="shortName" placeholder="VD: openai, google, microsoft" />
          <FormField error={errors.shortName?.message} />
        </FormGroup>

        <FormGroup>
          <FormLabel htmlFor="imageUrl">URL hình ảnh (không bắt buộc)</FormLabel>
          <Input {...register('imageUrl')} id="imageUrl" placeholder="https://example.com/logo.png" />
          <FormField error={errors.imageUrl?.message} />
        </FormGroup>
      </form>
    )
  },
)

CreateAIProviderForm.displayName = 'CreateAIProviderForm'
