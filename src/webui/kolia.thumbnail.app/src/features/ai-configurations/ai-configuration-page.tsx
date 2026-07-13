import { useMemo, useState, useEffect, useCallback } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { parseAsString, useQueryState } from 'nuqs'
import { Pencil, Plus, Star, Trash2 } from 'lucide-react'
import { toast } from 'sonner'

import { Button } from '../../components/ui/button'
import { ConfirmDialog } from '../../components/ui/confirm-dialog'
import { DateTimePicker } from '../../components/ui/date-time-picker'
import { DataTable } from '../../components/data-table/data-table'
import { useDataTableState } from '../../components/data-table/use-data-table-state'
import {
    StatusFilterGroup,
    type StatusFilter,
} from '../../components/filters/status-filter'

import { ApiError } from '../../lib/api/api-error'
import { formatDateTime } from '../../lib/date-formatter'
import { useSidebarContext } from '../../lib/sidebar-context'

import {
    deleteAIProviderConfiguration,
    fetchAIProviderConfigurations,
    setDefaultAIProviderConfiguration,
    type AIProviderConfigurationBaseDto,
    type AIProviderConfigurationDetailDto,
} from './api'

import {
    SortDirection,
    type RangeFilterRequestDto,
    type SortRequestDto,
} from '../../types/paging.types'
import { Badge } from '../../components/ui/badge'

export function AiConfigurationsPage() {
    const { open } = useSidebarContext()
    const queryClient = useQueryClient()

    const {
        page,
        setPage,
        pageSize,
        setPageSize,
        search,
        setSearch,
        sortBy,
        sortOrder,
        handleSort,
    } = useDataTableState(1, 10)

    const [statusFilter, setStatusFilter] =
        useState<StatusFilter>('active')

    const [localStatusFilter, setLocalStatusFilter] =
        useState<StatusFilter>('active')

    const [deleteTarget, setDeleteTarget] =
        useState<AIProviderConfigurationBaseDto | null>(null)

    const {
        mutate: remove,
        isPending: isDeleting,
    } = useMutation({
        mutationFn: (id: string) =>
            deleteAIProviderConfiguration(id),

        onSuccess: () => {
            toast.success('Đã xoá cấu hình AI.')

            queryClient.invalidateQueries({
                queryKey: ['ai-configurations'],
            })
        },

        onError: (error) => {
            toast.error(
                error instanceof ApiError
                    ? error.message
                    : 'Có lỗi xảy ra khi xoá.',
            )
        },
    })

    const {
        mutate: setDefault,
        isPending: isSettingDefault,
    } = useMutation({
        mutationFn: (id: string) =>
            setDefaultAIProviderConfiguration(id),

        onSuccess: () => {
            toast.success('Đã đặt cấu hình mặc định.')

            queryClient.invalidateQueries({
                queryKey: ['ai-configurations'],
            })
        },

        onError: (error) => {
            toast.error(
                error instanceof ApiError
                    ? error.message
                    : 'Có lỗi xảy ra.',
            )
        },
    })

    const handleDeleteConfirm = useCallback(() => {
        if (!deleteTarget) return

        remove(deleteTarget.id, {
            onSuccess: () => setDeleteTarget(null),
        })
    }, [deleteTarget, remove])

    const [appliedCreationFrom, setAppliedCreationFrom] =
        useQueryState(
            'creationFrom',
            parseAsString.withDefault(''),
        )

    const [appliedCreationTo, setAppliedCreationTo] =
        useQueryState(
            'creationTo',
            parseAsString.withDefault(''),
        )

    const [filterCreationFrom, setFilterCreationFrom] =
        useState(appliedCreationFrom)

    const [filterCreationTo, setFilterCreationTo] =
        useState(appliedCreationTo)

    useEffect(() => {
        setLocalStatusFilter(statusFilter)
    }, [statusFilter])

    useEffect(() => {
        setFilterCreationFrom(appliedCreationFrom)
    }, [appliedCreationFrom])

    useEffect(() => {
        setFilterCreationTo(appliedCreationTo)
    }, [appliedCreationTo])

    const backendSorts = useMemo<
        SortRequestDto[]
    >(() => {
        if (!sortBy || !sortOrder) {
            return []
        }

        let field = 'Name'

        if (sortBy === 'name') field = 'Name'
        if (sortBy === 'priority') field = 'Priority'
        if (sortBy === 'creationTime')
            field = 'CreationTime'
        if (
            sortBy === 'lastModificationTime'
        ) {
            field = 'LastModificationTime'
        }

        return [
            {
                field,
                direction:
                    sortOrder === 'desc'
                        ? SortDirection.Desc
                        : SortDirection.Asc,
            },
        ]
    }, [sortBy, sortOrder])

    const backendDeletedFilters =
        useMemo(() => {
            if (statusFilter === 'all') {
                return {
                    includeDeleted: true,
                }
            }

            if (statusFilter === 'deleted') {
                return {
                    includeDeleted: true,
                    deletedOnly: true,
                }
            }

            return {}
        }, [statusFilter])

    const backendRangeFilters =
        useMemo<RangeFilterRequestDto[]>(() => {
            const filters: RangeFilterRequestDto[] = []

            if (
                appliedCreationFrom ||
                appliedCreationTo
            ) {
                filters.push({
                    field: 'CreationTime',
                    from: appliedCreationFrom
                        ? new Date(
                            appliedCreationFrom,
                        ).toISOString()
                        : null,
                    to: appliedCreationTo
                        ? new Date(
                            appliedCreationTo,
                        ).toISOString()
                        : null,
                })
            }

            return filters
        }, [
            appliedCreationFrom,
            appliedCreationTo,
        ])

    const {
        data,
        isLoading,
        error,
        refetch,
    } = useQuery({
        queryKey: [
            'ai-configurations',
            page,
            pageSize,
            search,
            statusFilter,
            backendSorts,
            backendRangeFilters,
        ],

        queryFn: () =>
            fetchAIProviderConfigurations({
                pageNumber: page,
                pageSize,
                searchText: search || undefined,
                sorts: backendSorts,
                rangeFilters:
                    backendRangeFilters,
                includeDeleted:
                    backendDeletedFilters.includeDeleted,
                deletedOnly:
                    backendDeletedFilters.deletedOnly,
            }),
    })
    const columns = useMemo(
        () => [
            {
                key: 'provider',
                header: 'Provider',
                render: (item: AIProviderConfigurationDetailDto) => (
                    <div className="flex items-center gap-3">
                        {item.aiProviderLogo ? (
                            <img
                                src={item.aiProviderLogo}
                                alt={item.aiProviderName}
                                className="h-8 w-8 rounded object-cover"
                            />
                        ) : (
                            <div className="flex h-8 w-8 items-center justify-center rounded bg-slate-100 text-xs font-semibold">
                                {item.aiProviderShortName.substring(0, 2).toUpperCase()}
                            </div>
                        )}

                        <div>
                            <div className="font-medium">
                                {item.aiProviderName}
                            </div>

                            <div className="text-xs text-slate-500">
                                {item.aiProviderShortName}
                            </div>
                        </div>
                    </div>
                ),
            },

            {
                key: 'name',
                header: 'Tên',
                sortable: true,
                render: (item: AIProviderConfigurationBaseDto) => (
                    <div>
                        <div className="font-medium">
                            {item.name}
                        </div>

                        {item.description && (
                            <div className="text-xs text-slate-500 line-clamp-2">
                                {item.description}
                            </div>
                        )}
                    </div>
                ),
            },

            // {
            //     key: 'priority',
            //     header: 'Priority',
            //     sortable: true,
            //     render: (item: AIProviderConfigurationBaseDto) => (
            //         <Badge variant="secondary">
            //             {item.priority}
            //         </Badge>
            //     ),
            // },

            {
                key: 'timeoutSeconds',
                header: 'Timeout',
                render: (item: AIProviderConfigurationBaseDto) => (
                    <span>{item.timeoutSeconds}s</span>
                ),
            },

            {
                key: 'retryCount',
                header: 'Retry',
                render: (item: AIProviderConfigurationBaseDto) =>
                    item.retryCount,
            },

            {
                key: 'apiKeyMasked',
                header: 'API Key',
                render: (item: AIProviderConfigurationDetailDto) => (
                    <span className="font-mono text-xs text-slate-500" title={item.apiKey}>
                        {item.apiKeyMasked}
                    </span>
                ),
            },

            {
                key: 'totalTokensUsed',
                header: 'Tokens',
                render: (item: AIProviderConfigurationDetailDto) => (
                    <span className="text-xs text-slate-500">
                        {item.totalTokensUsed.toLocaleString()}
                    </span>
                ),
            },

            {
                key: 'isEnabled',
                header: 'Status',
                render: (item: AIProviderConfigurationBaseDto) =>
                    item.isEnabled ? (
                        <Badge variant="success" dot>Enabled</Badge>
                    ) : (
                        <Badge variant="secondary">Disabled</Badge>
                    ),
            },
            
            {
                key: 'isDefault',
                header: 'Default',
                render: (item: AIProviderConfigurationBaseDto) =>
                    item.isDefault ? (
                        <Badge>Default</Badge>
                    ) : (
                        <Button
                            variant="outline"
                            size="sm"
                            disabled={isSettingDefault}
                            onClick={() => setDefault(item.id)}
                        >
                            <Star className="mr-2 h-4 w-4" />
                            Đặt mặc định
                        </Button>
                    ),
            },

            {
                key: 'creationTime',
                header: 'Tạo lúc',
                sortable: true,
                render: (item: AIProviderConfigurationBaseDto) =>
                    formatDateTime(item.creationTime),
            },

            {
                key: 'lastModificationTime',
                header: 'Cập nhật',
                sortable: true,
                render: (item: AIProviderConfigurationBaseDto) =>
                    formatDateTime(
                        item.lastModificationTime,
                    ),
            },

            {
                key: 'actions',
                header: '',
                render: (item: AIProviderConfigurationBaseDto) =>
                    item.isDeleted ? (
                        <span className="text-xs italic text-slate-400">
                            Đã xoá
                        </span>
                    ) : (
                        <div className="flex items-center gap-0.5">
                            <Button
                                variant="ghost"
                                size="icon"
                                title="Chỉnh sửa"
                                onClick={() =>
                                    open({
                                        type: 'edit-ai-configuration',
                                        configuration: item,
                                    })
                                }
                            >
                                <Pencil className="h-4 w-4" />
                            </Button>

                            <Button
                                variant="ghost"
                                size="icon"
                                title="Xoá"
                                className="hover:bg-red-50 hover:text-red-600"
                                onClick={() =>
                                    setDeleteTarget(item)
                                }
                            >
                                <Trash2 className="h-4 w-4" />
                            </Button>
                        </div>
                    ),
            },
        ],
        [
            open,
            setDefault,
            isSettingDefault,
        ],
    )

    const handleApplyFilter = () => {
        setAppliedCreationFrom(filterCreationFrom)
        setAppliedCreationTo(filterCreationTo)
        setStatusFilter(localStatusFilter)
        setPage(1)
    }

    const handleResetFilter = () => {
        setFilterCreationFrom('')
        setFilterCreationTo('')

        setAppliedCreationFrom('')
        setAppliedCreationTo('')

        setLocalStatusFilter('active')
        setStatusFilter('active')

        setPage(1)
    }

    const filterContent = (
        <div className="space-y-5">
            <StatusFilterGroup
                value={localStatusFilter}
                onChange={setLocalStatusFilter}
            />

            <div className="space-y-3">
                <DateTimePicker
                    label="Từ ngày"
                    value={filterCreationFrom}
                    onChange={(e) =>
                        setFilterCreationFrom(
                            e.target.value,
                        )
                    }
                />

                <DateTimePicker
                    label="Đến ngày"
                    value={filterCreationTo}
                    onChange={(e) =>
                        setFilterCreationTo(
                            e.target.value,
                        )
                    }
                />
            </div>
        </div>
    )
    return (
        <div className="mx-auto max-w-7xl">
            <DataTable
                title="AI Configurations"
                columns={columns}
                data={data?.items ?? []}
                isLoading={isLoading}
                error={error instanceof Error ? error.message : null}
                onRetry={() => void refetch()}
                emptyMessage="Không có cấu hình AI nào phù hợp."
                page={data?.pageNumber ?? 1}
                pageSize={data?.pageSize ?? 10}
                totalPages={data?.totalPages ?? 1}
                totalCount={data?.totalCount ?? 0}
                onPageChange={(nextPage) => {
                    setPage(nextPage)
                }}
                onPageSizeChange={(nextPageSize) => {
                    setPageSize(nextPageSize)
                    setPage(1)
                }}
                sortBy={sortBy}
                sortOrder={sortOrder}
                onSort={handleSort}
                actions={
                    <Button
                        onClick={() =>
                            open({ type: 'create-ai-configuration' })
                        }
                        className="gap-2"
                    >
                        <Plus className="h-4 w-4" />
                        Thêm mới
                    </Button>
                }
                search={search}
                searchPlaceholder="Nhập tên cấu hình"
                onSearchChange={(value) => {
                    setSearch(value)
                    setPage(1)
                }}
                onSearchClear={() => {
                    setSearch('')
                    setPage(1)
                }}
                filterContent={filterContent}
                onApplyFilter={handleApplyFilter}
                onResetFilter={handleResetFilter}
            />

            <ConfirmDialog
                open={deleteTarget !== null}
                onClose={() =>
                    setDeleteTarget(null)
                }
                onConfirm={handleDeleteConfirm}
                title="Xoá cấu hình AI"
                message={
                    <>
                        Bạn có chắc chắn muốn xoá{' '}
                        <strong>
                            {deleteTarget?.name}
                        </strong>
                        ?
                        <br />
                        Thao tác này không thể hoàn tác.
                    </>
                }
                confirmLabel="Xoá"
                cancelLabel="Huỷ"
                variant="danger"
                loading={isDeleting}
            />
        </div>
    )
}