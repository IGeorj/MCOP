// queryFactories.ts
import { queryOptions } from '@tanstack/react-query';
import { Channel } from '@/types/Channel';
import fetchWithAuth from "./fetchWithAuth";
import { config } from "@/config";

// Query factory for guild channels
export const channelQueries = {
    getAllTextChannels: (guildId: string) =>
        queryOptions({
            queryKey: ['guilds', guildId, 'channels'] as const,
            queryFn: async (): Promise<Channel[]> =>
                fetchWithAuth<Channel[]>(`${config.API_URL}/guilds/${guildId}/channels`),
            enabled: !!guildId,
            staleTime: 1000 * 60 * 1, // 1 min
        }),

    getImageVerificationChannels: (guildId: string) =>
        queryOptions({
            queryKey: ['guilds', guildId, 'image-verification-channels'] as const,
            queryFn: async (): Promise<string[]> =>
                fetchWithAuth<string[]>(`${config.API_URL}/guilds/${guildId}/image-verification-channels`),
            enabled: !!guildId,
        }),
};

export const channelMutations = {
    addImageVerificationChannel: (guildId: string) => ({
        mutationFn: async (channelId: string) =>
            fetchWithAuth(`${config.API_URL}/guilds/${guildId}/image-verification-channels`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ channelId }),
            }),
    }),

    removeImageVerificationChannel: (guildId: string) => ({
        mutationFn: async (channelId: string) =>
            fetchWithAuth(`${config.API_URL}/guilds/${guildId}/image-verification-channels/${channelId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                },
            }),
    }),

    updateDailyNsfwChannel: (guildId: string) => ({
        mutationFn: async ({ channelId }: { channelId: string | null }) =>
            fetchWithAuth(`${config.API_URL}/guilds/${guildId}/daily-nsfw-channel`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ channelId }),
            }),
    }),
};