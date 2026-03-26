'use client';

import { motion, AnimatePresence } from 'framer-motion';
import { X } from 'lucide-react';
import { useEffect } from 'react';

interface SlideOverProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  children: React.ReactNode;
  width?: string;
}

export function SlideOver({ isOpen, onClose, title, children, width = 'max-w-2xl' }: SlideOverProps) {
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  return (
    <AnimatePresence>
      {isOpen && (
        <div className="fixed inset-0 z-50 overflow-hidden text-slate-800">
          <div className="absolute inset-0 overflow-hidden">
            {/* Backdrop */}
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={onClose}
              className="absolute inset-0 bg-slate-900/30 backdrop-blur-xs transition-opacity"
            />

            <div className="pointer-events-none fixed inset-y-0 right-0 flex max-w-full pl-10">
              <motion.div
                initial={{ x: '100%' }}
                animate={{ x: 0 }}
                exit={{ x: '100%' }}
                transition={{ type: 'spring', damping: 30, stiffness: 300 }}
                className={`pointer-events-auto w-screen ${width}`}
              >
                <div className="flex h-full flex-col overflow-y-scroll bg-white shadow-2xl border-l border-slate-100">
                  {/* Header */}
                  <div className="px-6 py-5 border-b border-slate-50 flex items-center justify-between bg-slate-50/50">
                    <div>
                      {title && <h2 className="text-xl font-bold text-slate-900">{title}</h2>}
                    </div>
                    <button
                      onClick={onClose}
                      className="rounded-xl p-2 text-slate-400 hover:text-slate-500 hover:bg-slate-100 transition-all focus:outline-none"
                    >
                      <X className="h-6 w-6" aria-hidden="true" />
                    </button>
                  </div>

                  {/* Body */}
                  <div className="relative flex-1 p-6 custom-scrollbar">
                    {children}
                  </div>
                </div>
              </motion.div>
            </div>
          </div>
        </div>
      )}
    </AnimatePresence>
  );
}
