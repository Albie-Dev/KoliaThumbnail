import React, { createContext, useContext, useState, type ReactNode } from 'react';

export type SidebarContent = { type: string; [key: string]: unknown } | null;

interface SidebarContextType {
  isOpen: boolean;
  content: SidebarContent;
  open: (content: SidebarContent) => void;
  close: () => void;
}

const SidebarContext = createContext<SidebarContextType | undefined>(undefined);

export const SidebarProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [content, setContent] = useState<SidebarContent>(null);

  const open = (newContent: SidebarContent) => {
    setContent(newContent);
    setIsOpen(true);
  };

  const close = () => {
    setIsOpen(false);
    // Delay clearing content to allow animation (400ms to match transition duration)
    setTimeout(() => setContent(null), 400);
  };

  return (
    <SidebarContext.Provider value={{ isOpen, content, open, close }}>
      {children}
    </SidebarContext.Provider>
  );
};

export const useSidebarContext = () => {
  const context = useContext(SidebarContext);
  if (!context) {
    throw new Error('useSidebarContext must be used within SidebarProvider');
  }
  return context;
};
