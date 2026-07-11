import React, { createContext, useContext, useState } from 'react';
import type { ReactNode } from 'react';
import { X } from 'lucide-react';
import { cn } from '../../lib/utils';

interface SidebarContextType {
  isOpen: boolean;
  open: () => void;
  close: () => void;
}

const SidebarContext = createContext<SidebarContextType | undefined>(undefined);

export const SidebarProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <SidebarContext.Provider
      value={{
        isOpen,
        open: () => setIsOpen(true),
        close: () => setIsOpen(false),
      }}
    >
      {children}
    </SidebarContext.Provider>
  );
};

export const useSidebar = () => {
  const context = useContext(SidebarContext);
  if (!context) {
    throw new Error('useSidebar must be used within SidebarProvider');
  }
  return context;
};

interface SidebarTriggerProps {
  asChild?: boolean;
  children: ReactNode;
}

export const SidebarTrigger: React.FC<SidebarTriggerProps> = ({ children, asChild }) => {
  const { open } = useSidebar();

  if (asChild && React.isValidElement(children)) {
    return React.cloneElement(children as React.ReactElement<any>, {
      onClick: open,
    });
  }

  return <button onClick={open}>{children}</button>;
};

import * as Portal from '@radix-ui/react-portal';

interface SidebarProps {
  isOpen: boolean;
  onClose: () => void;
  children: ReactNode;
  side?: 'left' | 'right';
}

export const Sidebar: React.FC<SidebarProps> = ({
  isOpen,
  onClose,
  children,
  side = 'right',
}) => {
  return (
    <Portal.Root>
      {/* Backdrop */}
      <div
        className={cn(
          'fixed inset-0 z-40 bg-black transition-all duration-400 ease-in-out',
          isOpen ? 'opacity-50' : 'pointer-events-none opacity-0'
        )}
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Sidebar */}
      <div
        className={cn(
          'fixed top-0 z-50 h-screen w-96 bg-white shadow-2xl transition-all duration-400 ease-in-out flex flex-col',
          side === 'right' ? 'right-0' : 'left-0',
          side === 'right' ? (isOpen ? 'translate-x-0' : 'translate-x-full') : (isOpen ? 'translate-x-0' : '-translate-x-full')
        )}
      >
        {children}
      </div>
    </Portal.Root>
  );
};

interface SidebarHeaderProps {
  title: string;
  onClose: () => void;
}

export const SidebarHeader: React.FC<SidebarHeaderProps> = ({ title, onClose }) => {
  return (
    <div className="flex items-center justify-between border-b px-6 py-4">
      <h2 className="text-xl font-semibold text-gray-900">{title}</h2>
      <button
        onClick={onClose}
        className="inline-flex items-center justify-center rounded-md p-2 text-gray-500 hover:bg-gray-100 hover:text-gray-900 focus:outline-none"
      >
        <X className="h-5 w-5" />
      </button>
    </div>
  );
};

interface SidebarBodyProps {
  children: ReactNode;
}

export const SidebarBody: React.FC<SidebarBodyProps> = ({ children }) => {
  return <div className="flex-1 overflow-y-auto px-6 py-4">{children}</div>;
};

interface SidebarFooterProps {
  children: ReactNode;
}

export const SidebarFooter: React.FC<SidebarFooterProps> = ({ children }) => {
  return (
    <div className="border-t px-6 py-4 flex items-center justify-end gap-3">
      {children}
    </div>
  );
};
