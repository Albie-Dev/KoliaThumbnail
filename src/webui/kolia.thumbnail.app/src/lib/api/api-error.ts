import type { ValidationErrorDto } from '../../types/api-error.types'

export class ApiError extends Error {
  code: string
  status: number
  traceId?: string
  errors?: ValidationErrorDto[]

  constructor(code: string, message: string, status: number, traceId?: string, errors?: ValidationErrorDto[]) {
    super(message)
    this.name = 'ApiError'
    this.code = code
    this.status = status
    this.traceId = traceId
    this.errors = errors
  }

  get isValidationError() {
    return this.code === 'VALIDATION_ERROR'
  }
}
