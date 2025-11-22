import { useQuery, useQueryClient } from '@tanstack/react-query';
import React, { useCallback, useEffect, useState } from 'react';
import { FaPlay, FaStop, FaAngleRight, FaAngleLeft, FaTrashAlt } from "react-icons/fa";
import { Button } from "@/components/ui/button";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { authFetch } from "../../utils/authFetch";
import { useIsMobile } from "../../hooks/useIsMobile";
import { FullscreenButton } from "../buttons/FullscreenButton";
import { ImageInfoDisplay } from "./ImageInfoDisplay";
import { IntervalControl } from "./IntervalControl";
import { useSlideshow } from "@/hooks/useSlideshow";
import { ImageInfo } from "@/types/ImageInfo";

export const SlideShow = () => {
    const [intervalSec, setIntervalSec] = useState(5);
    const [isControlsVisible, setIsControlsVisible] = useState(true);
    const isMobile = useIsMobile(640);
    
    const fetchImageContent = useCallback(async (path: string, requestInit?: RequestInit) => {
        return await authFetch<Blob>(`/images/content/${path}`, { 
            responseType: 'blob', 
            requestInit: requestInit 
        });
    }, []);

    const queryClient = useQueryClient();

    const { data: loadedImages = [], isLoading } = useQuery<ImageInfo[]>({
        queryKey: ['randomImages'],
        queryFn: () => authFetch(`/images/random?count=${50}`),
        staleTime: 60000,
        refetchOnWindowFocus: false,
    });

    const fetchMoreImages = async () => {
        const newImages = await authFetch<ImageInfo[]>(`/images/random?count=${50}`);
        queryClient.setQueryData<ImageInfo[]>(['randomImages'], (prev = []) => {
            const combined = [...prev, ...newImages];
            return combined.reduce((unique, item) => 
                unique.some(img => img.path === item.path) ? unique : [...unique, item], 
            [] as ImageInfo[]);
        });
    };

    const {
        currentImageSrc,
        currentIndex,
        isPlaying,
        play,
        pause,
        next,
        prev,
    } = useSlideshow(loadedImages, fetchImageContent, { 
        initialIndex: 0, 
        interval: intervalSec * 1000 
    });

    useEffect(() => {
        if (isPlaying && currentIndex >= loadedImages.length - 5 && loadedImages.length > 0) {
            fetchMoreImages();
        }
    }, [currentIndex, isPlaying, loadedImages.length]);

    if (isLoading) return <div className="flex items-center justify-center h-screen">Loading slideshow...</div>;
    if (!loadedImages.length) return <div className="flex items-center justify-center h-screen">No images found</div>;
    return (
        <div className="relative h-screen touch-none select-none">
            <div
                className={`text-sm absolute cursor-text select-text text-muted-foreground top-0 left-0 right-0 bg-navbar p-4 transition-opacity duration-300 ${isControlsVisible ? 'opacity-100' : 'opacity-0 pointer-events-none'
                    }`}
            >
                {loadedImages[currentIndex]?.path && <span className="ml-2">{loadedImages[currentIndex].path.split('\\').pop()}</span>}
            </div>
            {!isMobile && (
                <div className={`absolute top-18 right-8 transition-opacity duration-300 ${isControlsVisible ? 'opacity-100' : 'opacity-0 pointer-events-none'}`}>
                    <Button
                        variant="ghost"
                        size="sm"
                        className="bg-background/80 hover:bg-background"
                    >
                        <FaTrashAlt />
                    </Button>
                </div>
            )}
            <img
                src={currentImageSrc || ''}
                className={`w-full h-full object-contain transition-opacity duration-300`}
                crossOrigin="anonymous"
                alt={`Slide ${currentIndex + 1}`}
                key={`displayed-${currentIndex}`}
                onClick={() => setIsControlsVisible(!isControlsVisible)}
            />

            <div
                className={`absolute bottom-0 left-0 right-0 bg-navbar p-2 md:p-4 transition-opacity duration-300 ${isControlsVisible ? 'opacity-100' : 'opacity-0 pointer-events-none'
                    }`}
                onClick={(e) => e.stopPropagation()}
            >
                <div className="flex flex-wrap items-center justify-evenly md:justify-between gap-y-2">
                    <ImageInfoDisplay
                        image={loadedImages[currentIndex]}
                        currentIndex={currentIndex}
                        totalImages={loadedImages.length}
                    />
                    <div className="flex-shrink-0 flex items-center gap-2 md:gap-4">
                        <Tooltip>
                            <TooltipTrigger asChild>
                                <Button
                                    className="text-primary"
                                    variant="ghost"
                                    size="sm"
                                    onClick={() => isPlaying ? pause() : play()}
                                >
                                    {isPlaying ? <FaStop /> : <FaPlay />}
                                </Button>
                            </TooltipTrigger>
                            <TooltipContent>{isPlaying ? 'Stop' : 'Play'}</TooltipContent>
                        </Tooltip>

                        <IntervalControl
                            intervalSec={intervalSec}
                            onIntervalChange={(value) => setIntervalSec(value)}
                        />

                        <Tooltip>
                            <TooltipTrigger asChild>
                                <Button
                                    className="text-primary"
                                    variant="ghost"
                                    size="sm"
                                    onClick={prev}
                                >
                                    <FaAngleLeft />
                                </Button>
                            </TooltipTrigger>
                            <TooltipContent>Previous</TooltipContent>
                        </Tooltip>

                        <Tooltip>
                            <TooltipTrigger asChild>
                                <Button
                                    className="text-primary"
                                    variant="ghost"
                                    size="sm"
                                    onClick={next}
                                >
                                    <FaAngleRight />
                                </Button>
                            </TooltipTrigger>
                            <TooltipContent>Next</TooltipContent>
                        </Tooltip>

                        {!isMobile && <FullscreenButton />}
                    </div>
                </div>
            </div>
        </div>
    );
};