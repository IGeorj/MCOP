import React, { MouseEvent } from "react";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../hooks/useAuth";
import { useTranslation } from "react-i18next";
import DiscordLoginButton from "./DiscordLoginButton";
import { config } from "../config";
import { useNavigate } from "react-router-dom";
import { getAddBotUrl } from "../utils/discordApi";

type Guild = {
  id: string;
  name: string;
  icon: string | null;
  botPresent: boolean;
  isOwner: boolean;
};

const DiscordGuildList: React.FC = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const {
    isAuthenticated,
    isLoading: isAuthLoading,
    handleDiscordLogin,
  } = useAuth();

  const {
    data: guilds = [],
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
    retry: 1,
  });

  const onAddBotClick = (e: MouseEvent, guildId: string) => {
    e.preventDefault();
    window.open(getAddBotUrl(guildId), "_blank", "noopener,noreferrer");
  };

  const onSettingsClick = (e: MouseEvent, guildId: string) => {
    e.preventDefault();
    navigate(`/guilds/${guildId}`);
  };

  if (isAuthLoading) {
    return <div className="text-center py-4">{t("loading")}</div>;
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
      
      {isGuildsLoading && <div className="text-center py-2">{t("loading")}</div>}
      
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
            className="card flex items-center bg-secondary"
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
              <button
                className="ml-auto selected font-semibold px-3 py-1 rounded cursor-pointer bg-hover"
                onClick={(e) => onSettingsClick(e, guild.id)}
                aria-label={t("guilds.settings")}
              >
                {t("guilds.settings")}
              </button>
            ) : (
              <button
                className="ml-auto bg-hover px-3 py-1 cursor-pointer rounded font-medium"
                onClick={(e) => onAddBotClick(e, guild.id)}
                aria-label={t("guilds.addBot")}
              >
                {t("guilds.addBot")}
              </button>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default DiscordGuildList;