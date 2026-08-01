// VideoCard.tsx
import React from 'react';
import VideoThumbnail from './VideoThumbnail';
import { VideoInfo } from './VideoBrowser';

interface VideoCardProps {
    video: VideoInfo;
    onClick: (video: VideoInfo) => void;
}

const VideoCard: React.FC<VideoCardProps> = React.memo(function VideoCard({ video, onClick }: VideoCardProps) {
    const fileName = video.path.split('\\').pop() || video.path;
    const sizeMB = (video.size / (1024 * 1024)).toFixed(1);

    return (
        <button
            className="text-left rounded bg-secondary hover:bg-secondary/80 overflow-hidden cursor-pointer transition-all duration-200 hover:scale-105 hover:ring-2 hover:ring-primary hover:shadow-lg"
            onClick={() => onClick(video)}
        >
            <VideoThumbnail path={video.path} />
            <div className="p-2">
                <div className="truncate">{fileName}</div>
                <div className="text-xs text-muted-foreground">{sizeMB} MB</div>
            </div>
        </button>
    );
});

export default VideoCard;