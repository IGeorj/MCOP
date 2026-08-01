import React, { useMemo, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { authFetch } from '@/utils/authFetch';
import VideoPlayer from './VideoPlayer';
import { Spinner } from '@/components/common/Spinner';
import VideoGrid from './VideoGrid'; 
import FolderTags from './FolderTags';

export interface VideoInfo {
    path: string;
    fullPath: string;
    size: number;
}

const VideoBrowser: React.FC = () => {
    const [searchParams, setSearchParams] = useSearchParams();

    const folderParam = searchParams.get('folder');
    const videoParam = searchParams.get('video');
    const selectedFolder = folderParam || null;

    const { data: folders = [], isLoading: foldersLoading } = useQuery<string[]>({
        queryKey: ['videoFolders'],
        queryFn: () => authFetch('/videos/folders'),
        staleTime: 5 * 60 * 1000,
    });

    const {
        data: videos = [],
        isLoading,
        isError,
    } = useQuery<VideoInfo[]>({
        queryKey: ['randomVideos', selectedFolder],
        queryFn: () => {
            const base = '/videos/random?count=50';
            const url = selectedFolder
                ? `${base}&folder=${encodeURIComponent(selectedFolder)}`
                : base;
            return authFetch(url);
        },
        staleTime: 0,
        refetchOnWindowFocus: false,
        placeholderData: (prev) => prev,
    });

    const selected = useMemo(() => {
        if (!videoParam) return null;
        return videos.find((v) => v.path === decodeURIComponent(videoParam)) || null;
    }, [videos, videoParam]);

    useEffect(() => {
        if (videoParam && !selected) {
            setSearchParams((prev) => {
                prev.delete('video');
                return prev;
            });
        }
    }, [videoParam, selected, setSearchParams]);

    const onVideoClick = (video: VideoInfo) => {
        setSearchParams((prev) => {
            prev.set('video', encodeURIComponent(video.path));
            return prev;
        });
    };

    const onClosePlayer = () => {
        setSearchParams((prev) => {
            prev.delete('video');
            return prev;
        });
    };

    const onFolderClick = (folder: string | null) => {
        setSearchParams((prev) => {
            if (folder === null) prev.delete('folder');
            else prev.set('folder', folder);
            prev.delete('video');
            return prev;
        });
    };

    if (isLoading && videos.length === 0) {
        return (
            <div className="flex items-center justify-center h-screen">
                <Spinner />
            </div>
        );
    }
    if (isError || videos.length === 0) {
        return (
            <div className="flex items-center justify-center h-screen">
                No videos found
            </div>
        );
    }

    return (
        <div className="w-full h-screen relative flex flex-col">
            <FolderTags
                folders={folders}
                loading={foldersLoading}
                selectedFolder={selectedFolder}
                onFolderClick={onFolderClick}
            />
            <div className="flex-1 min-h-0">
                <VideoGrid videos={videos} onVideoClick={onVideoClick} />
            </div>
            {selected && (
                <div className="fixed inset-0 z-50 bg-black">
                    <VideoPlayer path={selected.path} onClose={onClosePlayer} />
                </div>
            )}
        </div>
    );
};

export default VideoBrowser;