// FolderTags.tsx
import React from 'react';
import { ScrollArea, ScrollBar } from '@/components/ui/scroll-area';
import { Spinner } from '@/components/common/Spinner';

interface FolderTagsProps {
    folders: string[];
    loading: boolean;
    selectedFolder: string | null;
    onFolderClick: (folder: string | null) => void;
}

const FolderTags: React.FC<FolderTagsProps> = React.memo(function FolderTags({
    folders,
    loading,
    selectedFolder,
    onFolderClick,
}: FolderTagsProps) {
    return (
        <div className="shrink-0 p-3 pb-1 border-b border-border bg-background">
            <ScrollArea type="always" className="w-full whitespace-nowrap pb-3">
                <div className="flex gap-2">
                    <button
                        onClick={() => onFolderClick(null)}
                        className={`px-3 py-1 items-center rounded whitespace-nowrap text-primary cursor-pointer bg-secondary bg-hover active:opacity-90 ${
                            selectedFolder === null ? 'border-primary border' : ''
                        }`}
                        style={{ display: 'flex', alignItems: 'center' }}
                        aria-pressed={selectedFolder === null}
                    >
                        All
                    </button>
                    {loading ? (
                        <Spinner size="sm" />
                    ) : (
                        folders.map((folder) => (
                            <button
                                key={folder}
                                onClick={() => onFolderClick(folder)}
                                className={`px-3 py-1 items-center rounded whitespace-nowrap text-primary cursor-pointer bg-secondary bg-hover active:opacity-90 ${
                                    selectedFolder === folder ? 'border-primary border' : ''
                                }`}
                                style={{ display: 'flex', alignItems: 'center' }}
                                aria-pressed={selectedFolder === folder}
                            >
                                {folder}
                            </button>
                        ))
                    )}
                </div>
                <ScrollBar orientation="horizontal" />
            </ScrollArea>
        </div>
    );
});

export default FolderTags;