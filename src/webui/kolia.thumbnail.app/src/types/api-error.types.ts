export interface ValidationErrorDto {
  property: string
  message: string
  errorCode: string
}

export interface ErrorResponseDto {
  code: string
  message: string
  traceId?: string
  errors?: ValidationErrorDto[]
}
