import type { Ref } from 'react'
import { registerSidebarEntry } from '../../lib/sidebar-registry'
import { CreateNewsSourceForm, type CreateNewsSourceFormHandle } from './create-news-source-form'
import { EditNewsSourceForm, type EditNewsSourceFormHandle } from './edit-news-source-form'
import type { NewsSourceListItemDto } from './api'

// ── Tạo mới nguồn tin ─────────────────────────────────────────────────────
registerSidebarEntry<{ type: 'create-news-source' }>('create-news-source', {
  title: () => 'Tạo nguồn tin mới',
  submitLabel: 'Tạo',
  render: ({ onClose, formRef }) => (
    <CreateNewsSourceForm
      ref={formRef as Ref<CreateNewsSourceFormHandle>}
      onClose={onClose}
    />
  ),
})

// ── Chỉnh sửa nguồn tin ───────────────────────────────────────────────────
registerSidebarEntry<{ type: 'edit-news-source'; source: NewsSourceListItemDto }>('edit-news-source', {
  title: (content) => `Chỉnh sửa: ${content.source.name}`,
  submitLabel: 'Lưu',
  render: ({ content, onClose, formRef }) => (
    <EditNewsSourceForm
      ref={formRef as Ref<EditNewsSourceFormHandle>}
      source={content.source}
      onClose={onClose}
    />
  ),
})
