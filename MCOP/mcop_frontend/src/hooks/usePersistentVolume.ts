// hooks/usePersistentVolume.ts
import { useState, useEffect } from 'react';

export const usePersistentVolume = () => {
  const [volume, setVolume] = useState<number>(() => {
    const saved = localStorage.getItem('video-player-volume');
    return saved !== null ? Number(JSON.parse(saved)) : 1;
  });

  const [muted, setMuted] = useState<boolean>(() => {
    const saved = localStorage.getItem('video-player-muted');
    return saved !== null ? Boolean(JSON.parse(saved)) : false;
  });

  useEffect(() => {
    localStorage.setItem('video-player-volume', JSON.stringify(volume));
  }, [volume]);

  useEffect(() => {
    localStorage.setItem('video-player-muted', JSON.stringify(muted));
  }, [muted]);

  return { volume, setVolume, muted, setMuted };
};