import React, {useEffect, useRef, useState, useMemo, useCallback} from 'react';
import { Button } from '@/components/ui/button';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';
import { FaPlay, FaPause, FaExpand, FaVolumeUp, FaVolumeMute, FaTimes } from 'react-icons/fa';
import { useFullscreen } from '@/hooks/useFullscreen';
import { config } from '@/config';
import { usePersistentVolume } from "@/hooks/usePersistentVolume";

interface VideoPlayerProps {
  path: string;
  className?: string;
  onClose?: () => void;
}

const formatTime = (sec: number) => {
  if (!isFinite(sec)) return '0:00';
  const s = Math.floor(sec % 60).toString().padStart(2, '0');
  const m = Math.floor(sec / 60);
  return `${m}:${s}`;
};

export const VideoPlayer: React.FC<VideoPlayerProps> = ({ path, className, onClose }) => {
  const videoRef = useRef<HTMLVideoElement | null>(null);
  const containerRef = useRef<HTMLDivElement | null>(null);
  const controlsRef = useRef<HTMLDivElement | null>(null);
  const timelineRef = useRef<HTMLInputElement | null>(null);
  const previewVideoRef = useRef<HTMLVideoElement | null>(null);
  const previewCanvasRef = useRef<HTMLCanvasElement | null>(null);

  const [isPlaying, setIsPlaying] = useState(false);
  const [duration, setDuration] = useState(0);
  const [currentTime, setCurrentTime] = useState(0);
  const [isControlsVisible, setIsControlsVisible] = useState(true);

  const { volume, setVolume, muted, setMuted } = usePersistentVolume();

  const [previewUrl, setPreviewUrl] = useState<string | undefined>(undefined);
  const [previewVisible, setPreviewVisible] = useState(false);
  const [previewLeft, setPreviewLeft] = useState(0);
  const [hoverTime, setHoverTime] = useState(0);

  const { isFullscreen, toggleFullscreen } = useFullscreen();

  const src = useMemo(() => `${config.API_URL}/videos/content/${encodeURIComponent(path).replace(/%5C/g, '%5C')}`, [path]);

  useEffect(() => {
    const v = videoRef.current;
    if (!v) return;
    const onLoaded = () => setDuration(v.duration || 0);
    const onTime = () => setCurrentTime(v.currentTime || 0);
    const onPlay = () => setIsPlaying(true);
    const onPause = () => setIsPlaying(false);

    v.addEventListener('loadedmetadata', onLoaded);
    v.addEventListener('timeupdate', onTime);
    v.addEventListener('play', onPlay);
    v.addEventListener('pause', onPause);

    v.volume = volume;
    v.muted = muted;

    return () => {
      v.removeEventListener('loadedmetadata', onLoaded);
      v.removeEventListener('timeupdate', onTime);
      v.removeEventListener('play', onPlay);
      v.removeEventListener('pause', onPause);
    };
  }, [path]);

  useEffect(() => {
    const v = videoRef.current;
    if (!v) return;
    v.volume = volume;
    v.muted = muted;
  }, [volume, muted]);

  useEffect(() => {
    const pv = previewVideoRef.current;
    if (!pv) return;
    pv.src = src;
    pv.preload = 'auto';
    pv.crossOrigin = 'use-credentials';
    pv.muted = true;
  }, [src]);

  const drawPreview = useCallback(() => {
    const pv = previewVideoRef.current;
    const cnv = previewCanvasRef.current;
    if (!pv || !cnv) return;
    try {
      const ctx = cnv.getContext('2d');
      if (!ctx) return;
      // Fixed preview size
      cnv.width = 160;
      cnv.height = 90;
      ctx.clearRect(0, 0, cnv.width, cnv.height);
      ctx.drawImage(pv, 0, 0, cnv.width, cnv.height);
      const url = cnv.toDataURL('image/jpeg');
      setPreviewUrl(url);
    } catch {
      // Canvas might be tainted if CORS fails; ignore preview
    }
  }, []);

  useEffect(() => {
    const pv = previewVideoRef.current;
    if (!pv) return;
    const onSeeked = () => drawPreview();
    const onLoadedData = () => drawPreview();
    pv.addEventListener('seeked', onSeeked);
    pv.addEventListener('loadeddata', onLoadedData);
    return () => {
      pv.removeEventListener('seeked', onSeeked);
      pv.removeEventListener('loadeddata', onLoadedData);
    };
  }, [drawPreview]);

  const togglePlay = useCallback(() => {
    const v = videoRef.current;
    if (!v) return;
    if (v.paused) v.play(); else v.pause();
  }, []);

  const onSeek = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const v = videoRef.current;
    if (!v) return;
    const value = Number(e.target.value);
    v.currentTime = value;
  }, []);

  const onKeyDown = useCallback((e: React.KeyboardEvent) => {
    if (e.code === 'Space') {
      e.preventDefault();
      togglePlay();
    } else if (e.code === 'ArrowLeft') {
      const v = videoRef.current; if (!v) return; v.currentTime = Math.max(0, v.currentTime - 5);
    } else if (e.code === 'ArrowRight') {
      const v = videoRef.current; if (!v) return; v.currentTime = Math.min(duration, v.currentTime + 5);
    } else if (e.code === 'KeyF') {
      toggleFullscreen();
    } else if (e.code === 'Escape' && !isFullscreen) {
      onClose?.();
    }
  }, [togglePlay, duration, toggleFullscreen]);

  const toggleMute = useCallback(() => {
    setMuted(!muted);
  }, []);

  const onChangeVolume = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const v = Math.max(0, Math.min(1, Number(e.target.value) / 100));
    setVolume(v);
    if (v === 0) setMuted(true);
    else if (muted) setMuted(false);
  }, [muted]);

  const handleTimelineMouseMove = useCallback((e: React.MouseEvent<HTMLInputElement>) => {
    const timeline = timelineRef.current;
    if (!timeline) return;

    const rect = timeline.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const ratio = Math.max(0, Math.min(1, x / rect.width));

    const dur = Number.isFinite(duration) && duration > 0 ? duration : 0;
    const epsilon = 0.25;
    const tRaw = dur * ratio;
    const t = dur > 0 ? Math.max(0, Math.min(dur - epsilon, tRaw)) : 0;
    setHoverTime(t);

    const popupHalf = 80;
    const desiredLeft = x;
    const clampedLeft = Math.max(popupHalf, Math.min(rect.width - popupHalf, desiredLeft));
    setPreviewLeft(clampedLeft);

    setPreviewVisible(true);
    const pv = previewVideoRef.current;
    if (pv && !isNaN(t)) {
      try { pv.currentTime = t; } catch {
        //ignore 
      }
    }
  }, [duration]);

  const handleTimelineMouseLeave = useCallback(() => {
    setPreviewVisible(false);
  }, []);

  return (
    <div ref={containerRef} className={`relative w-full h-full bg-black ${className || ''}`} onKeyDown={onKeyDown} tabIndex={0}>
      {onClose && isControlsVisible && (
        <div className="absolute top-2 right-2 z-20">
          <Button
            variant="ghost"
            size="sm"
            className="bg-background/80 hover:bg-background"
            onClick={onClose}
          >
            <FaTimes />
          </Button>
        </div>
      )}

      <video
        ref={videoRef}
        src={src}
        className="w-full h-full object-contain"
        onClick={() => setIsControlsVisible(!isControlsVisible)}
        controls={false}
        crossOrigin="use-credentials"
      />

      {/* Hidden elements for generating previews */}
      <video ref={previewVideoRef} style={{ display: 'none' }} muted preload="metadata" crossOrigin="use-credentials" />
      <canvas ref={previewCanvasRef} style={{ display: 'none' }} />

      {/* Controls */}
      <div ref={controlsRef} className={`absolute bottom-0 left-0 right-0 bg-navbar px-3 py-2 md:px-4 md:py-3 transition-opacity duration-300 ${isControlsVisible ? 'opacity-100' : 'opacity-0 pointer-events-none'}`}>
        <div className="flex items-center gap-3 w-full">
          {/* Left controls */}
          <div className="flex items-center gap-2 shrink-0">
            <Tooltip>
              <TooltipTrigger asChild>
                <Button className="text-primary" variant="ghost" size="sm" onClick={togglePlay}>
                  {isPlaying ? <FaPause /> : <FaPlay />}
                </Button>
              </TooltipTrigger>
              <TooltipContent>{isPlaying ? 'Pause' : 'Play'}</TooltipContent>
            </Tooltip>

            <Tooltip>
              <TooltipTrigger asChild>
                <Button className="text-primary" variant="ghost" size="sm" onClick={toggleMute}>
                  {(muted || volume === 0) ? <FaVolumeMute /> : <FaVolumeUp />}
                </Button>
              </TooltipTrigger>
              <TooltipContent>{(muted || volume === 0) ? 'Unmute' : 'Mute'}</TooltipContent>
            </Tooltip>

            <div className="h-6 flex items-center">
              <input
                type="range"
                min={0}
                max={100}
                step={1}
                value={(muted ? 0 : Math.round(volume * 100))}
                onChange={onChangeVolume}
                className="w-28 h-1.5 accent-color-primary"
              />
            </div>
          </div>

          {/* Middle timeline */}
          <div className="flex-1 flex items-center gap-2 min-w-0">
            <div className="text-xs text-muted-foreground w-16 text-right tabular-nums">
              {formatTime(currentTime)}
            </div>
            <div className="relative flex-1 h-6">
              <input
                ref={timelineRef}
                type="range"
                min={0}
                max={duration || 0}
                step={0.1}
                value={Math.min(currentTime, duration || 0)}
                onChange={onSeek}
                onMouseMove={handleTimelineMouseMove}
                onMouseLeave={handleTimelineMouseLeave}
                className="absolute inset-x-0 top-1/2 -translate-y-1/2 w-full accent-color-primary"
              />
              {previewVisible && previewUrl && (
                <div
                  className="absolute bottom-7 -translate-x-1/2 pointer-events-none"
                  style={{ left: previewLeft }}
                >
                  <div className="rounded bg-background shadow p-1 text-center">
                    <img src={previewUrl} alt="preview" className="min-w-40 h-24 object-cover rounded" />
                    <div className="text-xs text-muted-foreground tabular-nums">{formatTime(hoverTime)}</div>
                  </div>
                </div>
              )}
            </div>
            <div className="text-xs text-muted-foreground w-16 tabular-nums">
              {formatTime(duration)}
            </div>
          </div>

          {/* Right controls */}
          <div className="flex items-center gap-2 shrink-0">
            <Tooltip>
              <TooltipTrigger asChild>
                <Button className="text-primary" variant="ghost" size="sm" onClick={toggleFullscreen}>
                  <FaExpand />
                </Button>
              </TooltipTrigger>
              <TooltipContent>Fullscreen (F)</TooltipContent>
            </Tooltip>
          </div>
        </div>
      </div>
    </div>
  );
};

export default VideoPlayer;
