import React, { MouseEvent, useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../hooks/useAuth";
import { useTranslation } from "react-i18next";
import DiscordLoginButton from "./DiscordLoginButton";
import { config } from "../config";
import { useNavigate } from "react-router-dom";
import { getAddBotUrl } from "../utils/discordApi";
import { Guild } from "@/types/Guild";
import { useGuildList } from "../contexts/GuildListContext";
import { Skeleton } from "@/components/ui/skeleton";
import { FaChevronRight, FaCog } from "react-icons/fa";

const DiscordGuildList: React.FC = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const {
        isAuthenticated,
        isLoading: isAuthLoading,
        handleDiscordLogin,
    } = useAuth();

    const { guilds, setGuilds } = useGuildList();
    const {
        data: fetchedGuilds = [],
        isLoading: isGuildsLoading,
        error: guildsError,
    } = useQuery<Guild[]>({
        queryKey: ["guilds"],
        queryFn: async () => {
            const res = await fetch(`${config.API_URL}/guilds`, {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem("app_session")}`,
                },
            });
            if (!res.ok) throw new Error(t("errors.fetchGuilds"));
            return res.json();
        },
        enabled: isAuthenticated,
        staleTime: 1000 * 60 * 1, // 1 min
        retry: 1,
    });

    useEffect(() => {
        if (
            Array.isArray(fetchedGuilds) &&
            (guilds.length !== fetchedGuilds.length ||
                !guilds.every((g, i) => g.id === fetchedGuilds[i].id))
        ) {
            setGuilds(fetchedGuilds);
        }
    }, [fetchedGuilds, guilds, setGuilds]);

    const onAddBotClick = (e: MouseEvent, guildId: string) => {
        e.preventDefault();
        window.open(getAddBotUrl(guildId), "_blank", "noopener,noreferrer");
    };

    const onSettingsClick = (e: MouseEvent, guildId: string) => {
        e.preventDefault();
        navigate(`/guilds/${guildId}`);
    };

    if (isGuildsLoading || isAuthLoading) {
        return (
            <section className="max-w-2xl mx-auto my-8">
                <div className="flex flex-1 min-h-0 w-full">
                    <div className="flex-1 min-h-0 flex flex-col p-8 space-y-4">
                        <Skeleton className="h-8 w-32" />
                        <Skeleton className="h-22 w-full" />
                        <Skeleton className="h-22 w-full" />
                        <Skeleton className="h-22 w-full" />
                    </div>
                </div>
            </section>
        );
    }

    if (!isAuthenticated) {
        return (
            <div className="section flex flex-col items-center justify-center">
                <DiscordLoginButton
                    onLogin={handleDiscordLogin}
                    text={t("welcome.login")}
                />
            </div>
        );
    }

    return (
        <div className="max-w-2xl mx-auto my-8">
            <h2 className="mb-4 text-2xl font-semibold">{t("guilds.title")}</h2>

            {guildsError && (
                <div className="text-center py-2 text-primary">
                    {guildsError instanceof Error
                        ? guildsError.message
                        : t("errors.generic")}
                </div>
            )}

            <ul className="space-y-4">
                {guilds.map((guild) => (
                    <li
                        key={guild.id}
                        className="card flex items-center bg-secondary justify"
                    >
                        <div className="flex items-center min-w-0">
                            {guild.icon ? (
                                <img
                                    src={`https://cdn.discordapp.com/icons/${guild.id}/${guild.icon}.webp`}
                                    alt={guild.name}
                                    className="w-10 h-10 rounded-full mr-4 object-cover bg-hover"
                                />
                            ) : (
                                <div className="w-10 h-10 rounded-full mr-4 flex items-center justify-center font-bold text-xl bg-gray-500">
                                    {guild.name[0].toUpperCase()}
                                </div>
                            )}

                            <span className="truncate text-base text-text">
                                {guild.name}
                            </span>
                        </div>

                        {guild.botPresent ? (
                            <span className="ml-auto group">
                                <button
                                    className="active:scale-[0.90] flex gap-2 text-primary items-center px-3 py-1 rounded font-medium cursor-pointer transition-all duration-200 ease-in-out"
                                    onClick={(e) => onSettingsClick(e, guild.id)}
                                    aria-label={t("guilds.settings")}
                                >
                                    {t("guilds.settings")}
                                    <FaCog className="h-3 transition-transform duration-200 group-hover:rotate-45" />
                                </button>
                            </span>
                        ) : (
                            <span className="ml-auto group">
                                <button
                                    className="text-primary-reversed active:scale-[0.90] flex gap-2 items-center px-3 py-1 cursor-pointer rounded font-medium"
                                    onClick={(e) => onAddBotClick(e, guild.id)}
                                    aria-label={t("guilds.addBot")}
                                >
                                    {t("guilds.addBot")} <FaChevronRight className="h-3 transition-transform duration-200 group-hover:translate-x-1" />
                                </button>
                            </span>
                        )}
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default DiscordGuildList;