import React, { useEffect, useRef, useState } from 'react';
import { config } from '@/config';

interface VideoThumbnailProps {
  path: string;
  aspectRatio?: string; // e.g., '16/9'
  className?: string;
}

const VideoThumbnail: React.FC<VideoThumbnailProps> = ({ path, aspectRatio = '16/9', className }) => {
  const [thumbUrl, setThumbUrl] = useState<string | null>(null);
  const [error, setError] = useState<boolean>(false);
  const videoRef = useRef<HTMLVideoElement | null>(null);
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const containerRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    let cancelled = false;
    setThumbUrl(null);
    setError(false);

    const video = document.createElement('video');
    videoRef.current = video;
    video.crossOrigin = 'use-credentials';
    video.preload = 'auto';
    video.muted = true;
    video.src = `${config.API_URL}/videos/content/${encodeURIComponent(path).replace(/%5C/g, '%5C')}`;

    const handleLoaded = () => {
      try {
        video.currentTime = (video.duration || 1) * 0.5;
      } catch {
        // ignore
      }
    };

    const handleSeeked = () => {
      try {
        const canvas = canvasRef.current || document.createElement('canvas');
        canvasRef.current = canvas;
        const containerW = containerRef.current?.clientWidth || 320;
        const [wRatio, hRatio] = aspectRatio.split('/').map(Number);
        const containerH = Math.round(containerW * (hRatio / wRatio));
        canvas.width = containerW;
        canvas.height = containerH;
        const ctx = canvas.getContext('2d');
        if (!ctx) throw new Error('no ctx');
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
        const url = canvas.toDataURL('image/jpeg');
        if (!cancelled) setThumbUrl(url);
      } catch {
        if (!cancelled) setError(true);
      } finally {
        video.pause();
        video.removeAttribute('src');
        video.load();
      }
    };

    const handleError = () => {
      if (!cancelled) setError(true);
    };

    video.addEventListener('loadedmetadata', handleLoaded);
    video.addEventListener('seeked', handleSeeked);
    video.addEventListener('error', handleError);

    return () => {
      cancelled = true;
      video.removeEventListener('loadedmetadata', handleLoaded);
      video.removeEventListener('seeked', handleSeeked);
      video.removeEventListener('error', handleError);
      try {
        video.pause();
        video.removeAttribute('src');
        video.load();
      } catch {
        // ingnore
      }
    };
  }, [path, aspectRatio]);

  return (
    <div ref={containerRef} className={`w-full ${className || ''}`} style={{ aspectRatio }}>
      {thumbUrl && !error ? (
        <img src={thumbUrl} className="w-full h-full object-cover rounded" alt="thumbnail" />
      ) : (
        <div className="w-full h-full bg-secondary flex items-center justify-center rounded text-xs text-muted-foreground">
          {error ? 'No preview' : 'Loading...'}
        </div>
      )}
    </div>
  );
};

export default VideoThumbnail;
