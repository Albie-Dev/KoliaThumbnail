import type { Ref } from 'react'
import { registerSidebarEntry } from '../../lib/sidebar-registry'
import {
  EditAIFunctionConfigForm,
  type EditAIFunctionConfigFormHandle,
} from './edit-ai-function-config-form'
import type { AIFunctionConfigDetailDto } from './api'

registerSidebarEntry<{ type: 'edit-ai-function-config'; config: AIFunctionConfigDetailDto }>(
  'edit-ai-function-config',
  {
    title: (content) => `Chỉnh sửa: ${content.config.functionType}`,
    submitLabel: 'Lưu',
    render: ({ content, onClose, formRef }) => (
      <EditAIFunctionConfigForm
        ref={formRef as Ref<EditAIFunctionConfigFormHandle>}
        config={content.config}
        onClose={onClose}
      />
    ),
  },
)
