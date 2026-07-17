import { useEffect, useMemo, useRef, useState, useCallback, type ReactNode, type MouseEvent } from 'react'
import { createPortal } from 'react-dom'
import { cn } from '../../lib/utils'
import { ApiError } from '../../lib/api/api-error'
import type { BackendPagedResponse, PagedRequestParams, PageInfoDto } from '../../types/paging.types'

interface SelectDropdownBaseProps<T> {
    fetchData?: (params: PagedRequestParams) => Promise<BackendPagedResponse<T>>
    /** Danh sách tĩnh, dùng thay cho fetchData — không cần API */
    items?: T[]
    getOptionId: (item: T) => string
    getOptionLabel: (item: T) => string
    renderOption?: (item: T, isSelected: boolean) => ReactNode
    renderValue?: (item: T) => ReactNode
    allowSearch?: boolean
    placeholder?: string
    searchPlaceholder?: string
    pageSize?: number
    disabled?: boolean
    className?: string
    emptyText?: string
    /** Filters/sorts/includeDeleted... cố định cho mọi request */
    extraParams?: Omit<PagedRequestParams, 'pageNumber' | 'pageSize' | 'searchText'>
}

interface SingleSelectProps<T> extends SelectDropdownBaseProps<T> {
    multiple?: false
    value: T | null
    onChange: (value: T | null) => void
}

interface MultiSelectProps<T> extends SelectDropdownBaseProps<T> {
    multiple: true
    value: T[]
    onChange: (value: T[]) => void
}

export type SelectDropdownProps<T> = SingleSelectProps<T> | MultiSelectProps<T>

const DEFAULT_PAGE_SIZE = 10

export function SelectDropdown<T>(props: SelectDropdownProps<T>) {
    const {
        fetchData,
        items: itemsProp,
        getOptionId,
        getOptionLabel,
        renderOption,
        renderValue,
        allowSearch = true,
        placeholder = 'Chọn...',
        searchPlaceholder = 'Tìm kiếm...',
        pageSize = DEFAULT_PAGE_SIZE,
        disabled = false,
        className,
        emptyText = 'Không tìm thấy kết quả',
        extraParams,
        multiple,
        value,
        onChange,
    } = props

    const isStatic = !!itemsProp

    const [isOpen, setIsOpen] = useState(false)
    const [searchText, setSearchText] = useState('')
    const [items, setItems] = useState<T[]>([])
    const [pageInfo, setPageInfo] = useState<PageInfoDto | null>(null)
    const [loading, setLoading] = useState(false)
    const [loadingMore, setLoadingMore] = useState(false)
    const [error, setError] = useState<string | null>(null)
    const [loadMoreError, setLoadMoreError] = useState<string | null>(null)
    const [menuStyle, setMenuStyle] = useState<{ top: number; left: number; width: number } | null>(null)

    const containerRef = useRef<HTMLDivElement>(null)
    const triggerRef = useRef<HTMLButtonElement>(null)
    const listRef = useRef<HTMLUListElement>(null)
    const requestIdRef = useRef(0)
    const menuIdRef = useRef(`select-dropdown-menu-${Math.random().toString(36).slice(2, 9)}`)

    const selectedList = useMemo(
        () => (multiple ? (value as T[]) : value ? [value as T] : []),
        [multiple, value],
    )
    const selectedIds = useMemo(
        () => new Set(selectedList.map(getOptionId)),
        [selectedList, getOptionId],
    )

    const updateMenuPosition = useCallback(() => {
        if (!triggerRef.current) return
        const rect = triggerRef.current.getBoundingClientRect()
        setMenuStyle({ top: rect.bottom + 4, left: rect.left, width: rect.width })
    }, [])

    useEffect(() => {
        if (!isOpen) {
            setMenuStyle(null)
            return
        }

        updateMenuPosition()

        function handleClickOutside(e: globalThis.MouseEvent) {
            const target = e.target as Node
            if (containerRef.current?.contains(target)) return
            // Also ignore clicks inside the portal menu
            const menuEl = document.getElementById(menuIdRef.current)
            if (menuEl?.contains(target)) return
            setIsOpen(false)
        }

        function handleReposition() {
            updateMenuPosition()
        }

        document.addEventListener('mousedown', handleClickOutside)
        window.addEventListener('scroll', handleReposition, true)
        window.addEventListener('resize', handleReposition)

        return () => {
            document.removeEventListener('mousedown', handleClickOutside)
            window.removeEventListener('scroll', handleReposition, true)
            window.removeEventListener('resize', handleReposition)
        }
    }, [isOpen, updateMenuPosition])

    function buildParams(pageNumber: number): PagedRequestParams {
        return {
            pageNumber,
            pageSize,
            searchText: searchText || undefined,
            includeItems: true,
            includeTotalCount: true,
            ...extraParams,
        }
    }

    function toErrorMessage(e: unknown): string {
        return e instanceof ApiError ? e.message : 'Không thể tải dữ liệu, vui lòng thử lại.'
    }

    // Nếu dùng danh sách tĩnh, filter theo searchText
    const filteredStaticItems = useMemo(() => {
        if (!isStatic || !itemsProp) return itemsProp
        if (!searchText) return itemsProp
        const lower = searchText.toLowerCase()
        return itemsProp.filter((item) => getOptionLabel(item).toLowerCase().includes(lower))
    }, [isStatic, itemsProp, searchText, getOptionLabel])

    // Load dữ liệu khi mở dropdown (chỉ cho async mode)
    useEffect(() => {
        if (!isOpen) return

        if (isStatic) {
            // Với static items, set trực tiếp
            setItems(itemsProp ?? [])
            setPageInfo({
                pageNumber: 1,
                pageSize: itemsProp?.length ?? 0,
                totalRecords: itemsProp?.length ?? 0,
                totalPages: 1,
                hasNextPage: false,
                hasPreviousPage: false,
            })
            setLoading(false)
            setError(null)
            return
        }

        const requestId = ++requestIdRef.current
        setLoading(true)
        setError(null)

        const timeout = setTimeout(async () => {
            try {
                const result = await fetchData!(buildParams(1))
                if (requestId !== requestIdRef.current) return
                setItems(result.items)
                setPageInfo(result.pageInfo)
            } catch (e) {
                if (requestId !== requestIdRef.current) return
                setItems([])
                setPageInfo(null)
                setError(toErrorMessage(e))
            } finally {
                if (requestId === requestIdRef.current) setLoading(false)
            }
        }, searchText ? 300 : 0)

        return () => clearTimeout(timeout)
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [isOpen, searchText, isStatic, pageSize])

    async function loadMore() {
        if (isStatic || !pageInfo?.hasNextPage || loadingMore) return
        const requestId = requestIdRef.current
        setLoadingMore(true)
        setLoadMoreError(null)
        try {
            const result = await fetchData!(buildParams(pageInfo.pageNumber + 1))
            if (requestId !== requestIdRef.current) return
            setItems((prev) => [...prev, ...result.items])
            setPageInfo(result.pageInfo)
        } catch (e) {
            if (requestId !== requestIdRef.current) return
            setLoadMoreError(toErrorMessage(e))
        } finally {
            if (requestId === requestIdRef.current) setLoadingMore(false)
        }
    }

    function handleScroll() {
        const el = listRef.current
        if (!el) return
        if (el.scrollHeight - el.scrollTop - el.clientHeight < 48) loadMore()
    }

    function handleSelect(item: T) {
        if (multiple) {
            const id = getOptionId(item)
            const exists = (value as T[]).some((v) => getOptionId(v) === id)
            const next = exists
                ? (value as T[]).filter((v) => getOptionId(v) !== id)
                : [...(value as T[]), item]
                ; (onChange as (v: T[]) => void)(next)
        } else {
            ; (onChange as (v: T | null) => void)(item)
            setIsOpen(false)
            setSearchText('')
        }
    }

    function handleRemove(id: string, e: MouseEvent) {
        e.stopPropagation()
        if (multiple) {
            ; (onChange as (v: T[]) => void)((value as T[]).filter((v) => getOptionId(v) !== id))
        } else {
            ; (onChange as (v: T | null) => void)(null)
        }
    }

    function handleRetry() {
        if (isStatic) return
        // Bump lại requestId để trigger useEffect chạy lại logic load trang 1
        setError(null)
        const requestId = ++requestIdRef.current
        setLoading(true)
        fetchData!(buildParams(1))
            .then((result) => {
                if (requestId !== requestIdRef.current) return
                setItems(result.items)
                setPageInfo(result.pageInfo)
            })
            .catch((e) => {
                if (requestId !== requestIdRef.current) return
                setError(toErrorMessage(e))
            })
            .finally(() => {
                if (requestId === requestIdRef.current) setLoading(false)
            })
    }

    const displayItems = isStatic ? filteredStaticItems : items

    return (
        <div ref={containerRef} className={cn('relative', className)}>
            <button
                ref={triggerRef}
                type="button"
                disabled={disabled}
                onClick={() => setIsOpen((o) => !o)}
                className={cn(
                    'flex min-h-[36px] w-full items-center justify-between gap-2 rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 px-3 py-1.5 text-left text-sm transition-colors',
                    disabled ? 'cursor-not-allowed bg-slate-50 dark:bg-slate-900 text-slate-400 dark:text-slate-500' : 'hover:border-slate-300 hover:dark:border-slate-600',
                    isOpen && 'border-slate-400 dark:border-slate-500 ring-2 ring-slate-100 dark:ring-slate-800',
                )}
            >
                <div className="flex flex-1 flex-wrap items-center gap-1 overflow-hidden">
                    {selectedList.length === 0 && <span className="text-[13px] font-normal text-slate-400 dark:text-slate-500">{placeholder}</span>}

                    {!multiple && selectedList[0] && (
                        renderValue ? renderValue(selectedList[0]) : (
                            <span className="truncate text-slate-900 dark:text-slate-100">{getOptionLabel(selectedList[0])}</span>
                        )
                    )}

                    {multiple && selectedList.slice(0, 3).map((item) => (
                        <span
                            key={getOptionId(item)}
                            className="inline-flex items-center gap-1 rounded-md bg-slate-100 dark:bg-slate-800 px-1.5 py-0.5 text-xs font-medium text-slate-700 dark:text-slate-200"
                        >
                            {getOptionLabel(item)}
                            <span onClick={(e) => handleRemove(getOptionId(item), e)} className="text-slate-400 dark:text-slate-500 hover:text-slate-600 hover:dark:text-slate-300">
                                <XIcon />
                            </span>
                        </span>
                    ))}

                    {multiple && selectedList.length > 3 && (
                        <span className="text-xs font-medium text-slate-500 dark:text-slate-400">+{selectedList.length - 3}</span>
                    )}
                </div>
                <ChevronIcon className={cn('shrink-0 text-slate-400 dark:text-slate-500 transition-transform', isOpen && 'rotate-180')} />
            </button>

            {isOpen && menuStyle && createPortal(
                <div
                    id={menuIdRef.current}
                    className="z-50 overflow-hidden rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 shadow-lg"
                    style={{ position: 'fixed', top: menuStyle.top, left: menuStyle.left, width: menuStyle.width }}
                >
                    {allowSearch && (
                        <div className="border-b border-slate-100 dark:border-slate-800 p-2">
                            <div className="flex items-center gap-2 rounded-md bg-slate-50 dark:bg-slate-900 px-2 py-1.5">
                                <SearchIcon className="shrink-0 text-slate-400 dark:text-slate-500" />
                                <input
                                    autoFocus
                                    value={searchText}
                                    onChange={(e) => setSearchText(e.target.value)}
                                    placeholder={searchPlaceholder}
                                    className="w-full bg-transparent text-sm text-slate-900 dark:text-slate-100 outline-none placeholder:text-[13px] placeholder:font-normal placeholder:text-slate-400 placeholder:dark:text-slate-500"
                                />
                            </div>
                        </div>
                    )}

                    <ul ref={listRef} onScroll={handleScroll} className="max-h-64 overflow-y-auto py-1">
                        {loading && (!displayItems || displayItems.length === 0) && !error && (
                            <li className="flex items-center justify-center gap-2 px-3 py-6 text-sm text-slate-400 dark:text-slate-500">
                                <SpinnerIcon /> Đang tải...
                            </li>
                        )}

                        {!loading && error && (
                            <li className="flex flex-col items-center gap-2 px-3 py-6 text-center">
                                <AlertIcon className="text-rose-400 dark:text-rose-400" />
                                <span className="text-sm text-rose-600 dark:text-rose-400">{error}</span>
                                <button
                                    type="button"
                                    onClick={handleRetry}
                                    className="rounded-md border border-slate-200 dark:border-slate-700 px-2.5 py-1 text-xs font-medium text-slate-600 dark:text-slate-300 hover:bg-slate-50 hover:dark:bg-slate-900"
                                >
                                    Thử lại
                                </button>
                            </li>
                        )}

                        {!loading && !error && (!displayItems || displayItems.length === 0) && (
                            <li className="px-3 py-6 text-center text-sm text-slate-400 dark:text-slate-500">{emptyText}</li>
                        )}

                        {!error && displayItems && displayItems.map((item) => {
                            const id = getOptionId(item)
                            const isSelected = selectedIds.has(id)
                            return (
                                <li key={id}>
                                    <button
                                        type="button"
                                        onClick={() => handleSelect(item)}
                                        className={cn(
                                            'flex w-full items-center gap-2 px-3 py-2 text-left text-sm transition-colors',
                                            isSelected ? 'bg-slate-50 dark:bg-slate-900 text-slate-900 dark:text-slate-100' : 'text-slate-700 dark:text-slate-200 hover:bg-slate-50 hover:dark:bg-slate-900',
                                        )}
                                    >
                                        {multiple && (
                                            <span
                                                className={cn(
                                                    'flex h-4 w-4 shrink-0 items-center justify-center rounded border transition-colors',
                                                    isSelected
                                                        ? 'border-slate-900 bg-slate-900 dark:border-slate-100 dark:bg-slate-100'
                                                        : 'border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-900',
                                                )}
                                            >
                                                {isSelected && <CheckIcon className="h-3 w-3 text-white dark:text-slate-900" />}
                                            </span>
                                        )}

                                        <span className="flex-1 truncate">
                                            {renderOption ? renderOption(item, isSelected) : getOptionLabel(item)}
                                        </span>

                                        {!multiple && isSelected && <CheckIcon className="h-4 w-4 shrink-0 text-slate-900 dark:text-slate-100" />}
                                    </button>
                                </li>
                            )
                        })}

                        {loadingMore && (
                            <li className="flex items-center justify-center gap-2 px-3 py-3 text-xs text-slate-400 dark:text-slate-500">
                                <SpinnerIcon /> Đang tải thêm...
                            </li>
                        )}

                        {loadMoreError && (
                            <li className="flex flex-col items-center gap-1.5 px-3 py-3">
                                <span className="text-xs text-rose-600 dark:text-rose-400">{loadMoreError}</span>
                                <button
                                    type="button"
                                    onClick={loadMore}
                                    className="rounded-md border border-slate-200 dark:border-slate-700 px-2 py-0.5 text-xs font-medium text-slate-600 dark:text-slate-300 hover:bg-slate-50 hover:dark:bg-slate-900"
                                >
                                    Thử lại
                                </button>
                            </li>
                        )}
                    </ul>

                    {pageInfo && !error && !isStatic && (
                        <div className="border-t border-slate-100 dark:border-slate-800 px-3 py-1.5 text-[11px] text-slate-400 dark:text-slate-500">
                            {items.length}/{pageInfo.totalRecords} kết quả
                        </div>
                    )}

                    {isStatic && displayItems && itemsProp && (
                        <div className="border-t border-slate-100 dark:border-slate-800 px-3 py-1.5 text-[11px] text-slate-400 dark:text-slate-500">
                            {displayItems.length}/{itemsProp.length} kết quả
                        </div>
                    )}
                </div>,
                document.body,
            )}
        </div>
    )
}

function ChevronIcon({ className }: { className?: string }) {
    return (
        <svg className={cn('h-4 w-4', className)} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="m6 9 6 6 6-6" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
    )
}
function SearchIcon({ className }: { className?: string }) {
    return (
        <svg className={cn('h-4 w-4', className)} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="11" cy="11" r="7" />
            <path d="m21 21-4.3-4.3" strokeLinecap="round" />
        </svg>
    )
}
function XIcon({ className }: { className?: string }) {
    return (
        <svg className={cn('h-3 w-3', className)} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
            <path d="M18 6 6 18M6 6l12 12" strokeLinecap="round" />
        </svg>
    )
}
function CheckIcon({ className }: { className?: string }) {
    return (
        <svg className={cn('h-4 w-4', className)} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
            <path d="M20 6 9 17l-5-5" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
    )
}
function AlertIcon({ className }: { className?: string }) {
    return (
        <svg className={cn('h-5 w-5', className)} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M12 9v4M12 17h.01" strokeLinecap="round" />
            <path d="M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0Z" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
    )
}
function SpinnerIcon() {
    return (
        <svg className="h-3.5 w-3.5 animate-spin" viewBox="0 0 24 24" fill="none">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 0 1 8-8V0C5.373 0 0 5.373 0 12h4z" />
        </svg>
    )
}