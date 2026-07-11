import { useRef } from 'react';
import { useSidebarContext } from '../lib/sidebar-context';
import { Sidebar, SidebarHeader, SidebarBody, SidebarFooter } from './ui/sidebar';
import { CreateAIProviderForm, type CreateAIProviderFormHandle } from '../features/ai-providers/create-ai-provider-form';
import { EditAIProviderForm, type EditAIProviderFormHandle } from '../features/ai-providers/edit-ai-provider-form';
import { Button } from './ui/button';

export function AppSidebar() {
  const { isOpen, content, close } = useSidebarContext();
  const createFormRef = useRef<CreateAIProviderFormHandle>(null);
  const editFormRef = useRef<EditAIProviderFormHandle>(null);

  const getTitle = () => {
    switch (content) {
      case 'create-ai-provider':
        return 'Tạo nhà cung cấp mới';
      default:
        if (content && typeof content === 'object' && content.type === 'edit-ai-provider') {
          return `Chỉnh sửa: ${content.provider.name}`;
        }
        return '';
    }
  };

  const isEditMode = content !== null && typeof content === 'object' && content.type === 'edit-ai-provider';
  const isCreateMode = content === 'create-ai-provider';

  return (
    <Sidebar isOpen={isOpen} onClose={close}>
      <SidebarHeader title={getTitle()} onClose={close} />
      <SidebarBody>
        <div
          className={`transition-opacity duration-400 ease-in-out ${
            isOpen ? 'opacity-100' : 'opacity-0'
          }`}
        >
          {isCreateMode && (
            <CreateAIProviderForm ref={createFormRef} onClose={close} />
          )}
          {isEditMode && (
            <EditAIProviderForm ref={editFormRef} provider={content.provider} onClose={close} />
          )}
        </div>
      </SidebarBody>
      <SidebarFooter>
        <Button variant="outline" onClick={close}>
          Hủy
        </Button>
        {isCreateMode && (
          <Button
            onClick={() => createFormRef.current?.submit()}
            disabled={createFormRef.current?.isSubmitting}
          >
            {createFormRef.current?.isSubmitting ? 'Đang xử lý…' : 'Tạo'}
          </Button>
        )}
        {isEditMode && (
          <Button
            onClick={() => editFormRef.current?.submit()}
            disabled={editFormRef.current?.isSubmitting}
          >
            {editFormRef.current?.isSubmitting ? 'Đang xử lý…' : 'Lưu'}
          </Button>
        )}
      </SidebarFooter>
    </Sidebar>
  );
}
