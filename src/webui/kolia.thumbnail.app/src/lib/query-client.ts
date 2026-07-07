import { MutationCache, QueryCache, QueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { ApiError } from './api/api-error'

const copyTraceId = async (traceId?: string) => {
  if (!traceId) {
    return
  }

  await navigator.clipboard.writeText(traceId)
  toast.success('Đã sao chép mã lỗi')
}

export const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error) => {
      if (!(error instanceof ApiError) || error.isValidationError) {
        return
      }

      toast.error(error.message, {
        description: error.traceId ? `Mã theo dõi: ${error.traceId}` : undefined,
        action: error.traceId
          ? { label: 'Sao chép mã lỗi', onClick: () => void copyTraceId(error.traceId) }
          : undefined,
      })
    },
  }),
  mutationCache: new MutationCache({
    onError: (error) => {
      if (!(error instanceof ApiError) || error.isValidationError) {
        return
      }

      toast.error(error.message)
    },
  }),
  defaultOptions: {
    queries: {
      retry: (count, error) => {
        if (error instanceof ApiError) {
          return error.status >= 500 && count < 2
        }

        return count < 2
      },
      staleTime: 30_000,
    },
  },
})
