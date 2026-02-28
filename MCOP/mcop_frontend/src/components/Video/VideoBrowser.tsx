import React, { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { authFetch } from '@/utils/authFetch';
import VideoPlayer from './VideoPlayer';
import { Spinner } from '@/components/common/Spinner';
import VideoThumbnail from './VideoThumbnail';

interface VideoInfo {
  path: string;
  fullPath: string;
  size: number;
}

const VideoBrowser: React.FC = () => {
  const { data: videos = [], isLoading, isError, isFetching } = useQuery<VideoInfo[]>({
    queryKey: ['randomVideos'],
    queryFn: () => authFetch(`/videos/random?count=${50}`),
    staleTime: 0,
    refetchOnWindowFocus: false,
  });

  const [selected, setSelected] = useState<VideoInfo | null>(null);

  const grid = useMemo(() => (
    <div className="flex flex-col h-full overflow-y-scroll flex-1">
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3 p-3">
        {videos.map((v, idx) => (
          <button key={idx} className="text-left rounded bg-secondary hover:bg-secondary/80 overflow-hidden" onClick={() => setSelected(v)}>
            <VideoThumbnail path={v.path} />
            <div className="p-2">
              <div className="truncate">{v.path.split('\\').pop()}</div>
              <div className="text-xs text-muted-foreground">{(v.size / (1024 * 1024)).toFixed(1)} MB</div>
            </div>
          </button>
        ))}
      </div>
    </div>
  ), [videos, isFetching]);

  if (isLoading) return <div className="flex items-center justify-center h-screen"><Spinner /></div>;
  if (isError || videos.length === 0) return <div className="flex items-center justify-center h-screen">No videos found</div>;

  return (
    <div className="w-full h-screen relative">
      {grid}
      {selected && (
        <div className="fixed inset-0 z-50 bg-black">
          <VideoPlayer path={selected.path} onClose={() => setSelected(null)} />
        </div>
      )}
    </div>
  );
};

export default VideoBrowser;
