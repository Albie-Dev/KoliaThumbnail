import { forwardRef, useImperativeHandle } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { createProject } from './api'
import { createProjectSchema, type CreateProjectInput } from './schema'
import { Input } from '../../components/ui/input'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { ApiError } from '../../lib/api/api-error'

interface CreateProjectFormProps {
  onClose?: () => void
}

export interface CreateProjectFormHandle {
  submit: () => Promise<void>
  isSubmitting: boolean
}

export const CreateProjectForm = forwardRef<CreateProjectFormHandle, CreateProjectFormProps>(
  ({ onClose }, ref) => {
    const queryClient = useQueryClient()

    const {
      register,
      handleSubmit,
      formState: { errors },
      setError,
      reset,
    } = useForm<CreateProjectInput>({
      resolver: zodResolver(createProjectSchema),
      defaultValues: { name: '' },
    })

    const { mutate, isPending } = useMutation({
      mutationFn: (data: CreateProjectInput) => createProject(data),
      onError: (error) => {
        if (error instanceof ApiError && error.isValidationError) {
          error.errors?.forEach((validationError) => {
            setError(validationError.property as keyof CreateProjectInput, {
              message: validationError.message,
            })
          })
          // Global handler bỏ qua validation error, tự xử lý local tại đây
        }
        // Các lỗi khác (non-validation) đã được global handler toast rồi
      },
      onSuccess: () => {
        toast.success('Tạo project thành công!')
        onClose?.()
        reset()
        queryClient.invalidateQueries({ queryKey: ['projects'] })
      },
    })

    const onSubmit = async (data: CreateProjectInput) => {
      await new Promise<void>((resolve, reject) => {
        mutate(data, {
          onSuccess: () => resolve(),
          onError: (error) => reject(error),
        })
      })
    }

    useImperativeHandle(ref, () => ({
      submit: handleSubmit(onSubmit),
      isSubmitting: isPending,
    }))

    return (
      <form className="space-y-5">
        <FormGroup>
          <FormLabel htmlFor="name" required>Tên project</FormLabel>
          <Input {...register('name')} id="name" placeholder="VD: Livestream vàng tuần 30" />
          <FormField error={errors.name?.message} />
        </FormGroup>
      </form>
    )
  },
)

CreateProjectForm.displayName = 'CreateProjectForm'
