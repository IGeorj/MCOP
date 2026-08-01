// VideoGrid.tsx
import React from 'react';
import { ScrollArea } from '@/components/ui/scroll-area';
import VideoCard from './VideoCard'; // new
import { VideoInfo } from './VideoBrowser'; // or export from a types file

interface VideoGridProps {
    videos: VideoInfo[];
    onVideoClick: (video: VideoInfo) => void;
}

const VideoGrid: React.FC<VideoGridProps> = React.memo(function VideoGrid({ videos, onVideoClick }: VideoGridProps) {
    return (
        <ScrollArea className="h-full">
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3 p-3">
                {videos.map((v, idx) => (
                    <VideoCard key={idx} video={v} onClick={onVideoClick} />
                ))}
            </div>
        </ScrollArea>
    );
});

export default VideoGrid;