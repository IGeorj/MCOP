// queryFactories.ts
import { queryOptions } from '@tanstack/react-query';
import fetchWithAuth from "./fetchWithAuth";
import { config } from "@/config";

// Query factory for guild channels
export const roleQueries = {
    getGuildRoles: (guildId: string) =>
        queryOptions({
            queryKey: ['guildRoles', guildId] as const,
            queryFn: async (): Promise<Role[]> =>
                fetchWithAuth<Role[]>(`${config.API_URL}/guilds/${guildId}/roles`),
            enabled: !!guildId,
        }),
};

export const roleMutations = {
    addOrUpdateLevelRole: (guildId: string) => ({
        mutationFn: async ({ roleId, level }: { roleId: string; level: number }) =>
            fetchWithAuth(`${config.API_URL}/guilds/${guildId}/level-roles`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ roleId, level }),
            }),
    }),
    removeLevelRole: (guildId: string) => ({
        mutationFn: async (roleId: string) =>
            fetchWithAuth(`${config.API_URL}/guilds/${guildId}/level-roles/${roleId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                }
            }),
    }),
    updateRoleTemplate: (guildId: string) => ({
        mutationFn: async ({ roleId, template }: { roleId: string; template: string | null }) =>
            fetchWithAuth(`${config.API_URL}/guilds/${guildId}/level-roles/${roleId}/message-template`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ template }),
            }),
    }),
};