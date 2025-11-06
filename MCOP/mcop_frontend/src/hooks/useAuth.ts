import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { config } from "../config";
import { getDiscordAuthUrl } from "../utils/discordApi";

export interface IUser {
    id: string;
    username: string;
    avatarUrl: string;
}

type AuthResult = {
    session: string;
    id: string;
    username: string;
    avatarUrl: string;
};

export function useAuth() {
    const queryClient = useQueryClient();

    const {
        data: user,
        isLoading,
        error,
        isError,
    } = useQuery<IUser | null>({
        queryKey: ["auth", "current_user"],
        queryFn: async () => {
            const session = localStorage.getItem("app_session");
            if (!session) return null;

            const response = await fetch(`${config.API_URL}/auth/me`, {
                headers: { Authorization: `Bearer ${session}` },
            });

            if (!response.ok) {
                throw new Error("Session invalid");
            }

            return response.json();
        },
        retry: false
    });

    const { mutate: handleDiscordLogin } = useMutation({
        mutationFn: async () => {
            window.location.assign(getDiscordAuthUrl());
        },
    });

    const { mutate: handleLogout } = useMutation({
        mutationFn: async () => {
            localStorage.removeItem("app_session");
            queryClient.setQueryData(["auth", "current_user"], null);
        },
    });

    const { mutate: handleAuthResult } = useMutation({
        mutationFn: async (data: AuthResult | null) => {
            if (!data?.session) {
                handleLogout();
                return;
            }

            localStorage.setItem("app_session", data.session);
            queryClient.setQueryData(["auth", "current_user"], {
                id: data.id,
                username: data.username,
                avatarUrl: data.avatarUrl,
            });
        },
    });

    return {
        user,
        isLoading,
        error,
        isAuthenticated: !!user && !isError,
        handleDiscordLogin,
        handleLogout,
        handleAuthResult,
    };
}