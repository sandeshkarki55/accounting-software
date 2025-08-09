import React, { useEffect, useRef } from 'react';

interface BaseModalProps {
  show: boolean;
  onHide: () => void;
  title?: React.ReactNode;
  children: React.ReactNode;
  footer?: React.ReactNode;
  size?: 'sm' | 'md' | 'lg' | 'xl' | 'fullscreen';
  className?: string;
  ariaLabel?: string;
}

const sizeClassMap = {
  sm: 'modal-sm',
  md: '',
  lg: 'modal-lg',
  xl: 'modal-xl',
  fullscreen: 'modal-fullscreen',
};

const BaseModal: React.FC<BaseModalProps> = ({
  show,
  onHide,
  title,
  children,
  footer,
  size = 'md',
  className = '',
  ariaLabel = 'Modal Dialog',
}) => {
  const modalRef = useRef<HTMLDivElement>(null);

  // Focus management: focus modal on open
  useEffect(() => {
    if (show && modalRef.current) {
      modalRef.current.focus();
    }
  }, [show]);

  // Close on Escape key
  useEffect(() => {
    if (!show) return;
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        onHide();
      }
    };
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [show, onHide]);

  if (!show) return null;

  return (
    <div
      className={`modal show d-block ${className}`}
      tabIndex={-1}
      role="dialog"
      aria-modal="true"
      aria-label={ariaLabel}
      style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}
    >
      <div className={`modal-dialog ${sizeClassMap[size] || ''}`}>
        <div className="modal-content" ref={modalRef} tabIndex={-1}>
          <div className="modal-header">
            {title && <h5 className="modal-title">{title}</h5>}
            <button type="button" className="btn-close" aria-label="Close" onClick={onHide}></button>
          </div>
          <div className="modal-body">{children}</div>
          {footer && <div className="modal-footer">{footer}</div>}
        </div>
      </div>
    </div>
  );
};

export default BaseModal;
