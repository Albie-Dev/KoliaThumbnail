import { useRef } from 'react';
import { useSidebarContext } from '../lib/sidebar-context';
import { Sidebar, SidebarHeader, SidebarBody, SidebarFooter } from './ui/sidebar';
import { CreateAIProviderForm, type CreateAIProviderFormHandle } from '../features/thumbnails/create-ai-provider-form';
import { Button } from './ui/button';

export function AppSidebar() {
  const { isOpen, content, close } = useSidebarContext();
  const formRef = useRef<CreateAIProviderFormHandle>(null);

  const getTitle = () => {
    switch (content) {
      case 'create-ai-provider':
        return 'Tạo nhà cung cấp mới';
      default:
        return '';
    }
  };

  return (
    <Sidebar isOpen={isOpen} onClose={close}>
      <SidebarHeader title={getTitle()} onClose={close} />
      <SidebarBody>
        <div
          className={`transition-opacity duration-400 ease-in-out ${
            isOpen ? 'opacity-100' : 'opacity-0'
          }`}
        >
          {content === 'create-ai-provider' && (
            <CreateAIProviderForm ref={formRef} onClose={close} />
          )}
        </div>
      </SidebarBody>
      <SidebarFooter>
        <Button variant="outline" onClick={close}>
          Hủy
        </Button>
        {content === 'create-ai-provider' && (
          <Button
            onClick={() => formRef.current?.submit()}
            disabled={formRef.current?.isSubmitting}
          >
            {formRef.current?.isSubmitting ? 'Đang xử lý…' : 'Tạo'}
          </Button>
        )}
      </SidebarFooter>
    </Sidebar>
  );
}
