// =============================================================================
// CApiPaginationType — mirror BE enum
// =============================================================================

export const CApiPaginationType = {
  None: 0,
  Offset: 1,
  Page: 2,
  Cursor: 3,
} as const

export type CApiPaginationType = (typeof CApiPaginationType)[keyof typeof CApiPaginationType]

export const API_PAGINATION_TYPE_OPTIONS: { id: CApiPaginationType; label: string }[] = [
  { id: CApiPaginationType.None, label: 'Không phân trang' },
  { id: CApiPaginationType.Offset, label: 'Offset' },
  { id: CApiPaginationType.Page, label: 'Page' },
  { id: CApiPaginationType.Cursor, label: 'Cursor' },
]

export function getApiPaginationTypeLabel(v: number): string | undefined {
  return API_PAGINATION_TYPE_OPTIONS.find((o) => o.id === v)?.label
}
