import type { ComponentType, ReactNode } from 'react'

/**
 * A single menu item in the enterprise sidebar.
 * Supports N-level nesting via `children`.
 */
export interface AdminMenuItem {
  /** Unique key, used for route path matching and active highlight.
   *  Leaf items: full path e.g. "/thumbnails/providers"
   *  Group items: no slash prefix e.g. "management" */
  key: string

  /** Display label in the sidebar */
  label: string

  /** Optional icon (lucide-react icon component or any ReactNode) */
  icon?: ComponentType<{ className?: string }> | ReactNode

  /** Nested children — supports infinite depth */
  children?: AdminMenuItem[]

  /** React component to render when this leaf item is selected.
   *  Only leaf items (no children) should have a component. */
  component?: ComponentType
}

/**
 * A group in the sidebar with a visual separator / group label.
 */
export interface AdminMenuGroup {
  /** Group label displayed above items */
  label?: string

  /** Items in this group */
  items: AdminMenuItem[]
}
