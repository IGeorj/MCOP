import { useState, useEffect, useCallback } from 'react';

export function useSlideshow(
  images: { path: string }[],
  fetchImage: (path: string) => Promise<Blob>,
  options: { initialIndex?: number; interval?: number } = {}
) {
  const {
    initialIndex = 0,
    interval = 3000
  } = options;

  const [currentIndex, setCurrentIndex] = useState(initialIndex);
  const [currentImage, setCurrentImage] = useState<string | null>(null);
  const [isPlaying, setIsPlaying] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const loadImage = useCallback(async (index: number) => {
    if (!images.length || index < 0 || index >= images.length) {
      return;
    }

    setIsLoading(true);
    const image = images[index];
    
    try {
      const blob = await fetchImage(image.path);
      const imageUrl = URL.createObjectURL(blob);

      const img = new Image();
      img.src = imageUrl;
      img.onload = () => {
        setCurrentImage(prev => {
          if (prev) URL.revokeObjectURL(prev);
          return imageUrl;
        });
      };
    } catch (error) {
      console.error('Error loading image:', error);
    }

    setIsLoading(false);
    setCurrentIndex(index);
  }, [images, fetchImage]);

  const next = useCallback(() => {
    if (images.length === 0 || isLoading) return;
    const newIndex = (currentIndex + 1) % images.length;
    loadImage(newIndex);
  }, [currentIndex, images.length, loadImage, isLoading]);

  const prev = useCallback(() => {
    if (images.length === 0 || isLoading) return;
    const newIndex = (currentIndex - 1 + images.length) % images.length;
    loadImage(newIndex);
  }, [currentIndex, images.length, loadImage, isLoading]);

  useEffect(() => {
    if (!isPlaying) return;
    const timer = setInterval(next, interval);
    return () => clearInterval(timer);
  }, [isPlaying, next, interval]);

  // Инициализация первого изображения
  useEffect(() => {
    if (images.length > 0 && currentIndex === initialIndex) {
      loadImage(initialIndex);
    }
  }, [images, initialIndex, loadImage]);

  useEffect(() => {
    return () => {
      if (currentImage) {
        URL.revokeObjectURL(currentImage);
      }
    };
  }, [currentImage]);

  const play = useCallback(() => setIsPlaying(true), []);
  const pause = useCallback(() => setIsPlaying(false), []);

  return {
    currentImageSrc: currentImage,
    currentIndex,
    isPlaying,
    isLoading,
    next,
    prev,
    play,
    pause,
  };
}