import React, { useState, useEffect } from 'react';

interface SpinnerProps {
  show?: boolean;
  delay?: number; // milliseconds before showing
  minDisplayTime?: number; // minimum display time
  size?: 'sm' | 'md' | 'lg' | 'xl';
  color?: string;
  className?: string;
}

export const Spinner: React.FC<SpinnerProps> = ({
  show = true,
  delay = 0,
  minDisplayTime = 500,
  size = 'md',
  color = 'var(--color-primary)',
  className = ''
}) => {
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    let showTimer: NodeJS.Timeout;
    let hideTimer: NodeJS.Timeout;

    if (show) {
      showTimer = setTimeout(() => {
        setIsVisible(true);
      }, delay);
    } else {
      hideTimer = setTimeout(() => {
        setIsVisible(false);
      }, minDisplayTime);
    }

    return () => {
      if (showTimer) clearTimeout(showTimer);
      if (hideTimer) clearTimeout(hideTimer);
    };
  }, [show, delay, minDisplayTime]);

  if (!isVisible) return null;

  const sizeClasses = {
    sm: 'w-2 h-2',
    md: 'w-4 h-4',
    lg: 'w-8 h-8',
    xl: 'w-16 h-16'
  };

  return (
    <div
      className={`rounded-full animate-spin ${sizeClasses[size]} ${className}`}
      style={{
        border: '3px solid',
        borderColor: color,
        borderTopColor: 'transparent'
      }}
    />
  );
};