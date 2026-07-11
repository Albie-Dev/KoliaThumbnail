export interface PageInfoDto {
  pageNumber: number
  pageSize: number
  totalRecords: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface BackendPagedResponse<T> {
  items: T[]
  pageInfo: PageInfoDto
}

export interface PagedResult<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}

// ── Sort ──────────────────────────────────────────────────────────────────────

/** Mirrors BE enum CSortDirection (Asc=0, Desc=1) */
export const SortDirection = {
  Asc: 0,
  Desc: 1,
} as const
export type SortDirection = (typeof SortDirection)[keyof typeof SortDirection]

export interface SortRequestDto {
  /** PascalCase field name, e.g. "Name", "CreationTime" */
  field: string
  direction: SortDirection
}

// ── Filter ────────────────────────────────────────────────────────────────────

/** Mirrors BE enum CFilterOperator */
export const FilterOperator = {
  Equal: 0,
  NotEqual: 1,
  GreaterThan: 2,
  GreaterThanOrEqual: 3,
  LessThan: 4,
  LessThanOrEqual: 5,
  Contains: 6,
  StartsWith: 7,
  EndsWith: 8,
  In: 9,
  NotIn: 10,
  Between: 11,
  IsNull: 12,
  IsNotNull: 13,
} as const
export type FilterOperator = (typeof FilterOperator)[keyof typeof FilterOperator]

/** Mirrors BE enum CLogicalOperator */
export const LogicalOperator = {
  And: 0,
  Or: 1,
} as const
export type LogicalOperator = (typeof LogicalOperator)[keyof typeof LogicalOperator]

export interface FilterRequestDto {
  field: string
  operator: FilterOperator
  /** Raw primitive values (string | number | boolean) serialized to query string */
  values: (string | number | boolean)[]
  logicalOperator?: LogicalOperator
}

export interface RangeFilterRequestDto {
  field: string
  /** ISO string or number — null means no lower bound */
  from?: string | number | null
  /** ISO string or number — null means no upper bound */
  to?: string | number | null
  logicalOperator?: LogicalOperator
}

// ── Unified request params ────────────────────────────────────────────────────

export interface PagedRequestParams {
  pageNumber?: number
  pageSize?: number
  includeTotalCount?: boolean
  includeItems?: boolean
  searchText?: string
  sorts?: SortRequestDto[]
  filters?: FilterRequestDto[]
  rangeFilters?: RangeFilterRequestDto[]
  includeDeleted?: boolean
  deletedOnly?: boolean
}
