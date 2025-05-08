import React, { useEffect, useState, MouseEvent } from "react";
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
  const { appSession, handleDiscordLogin } = useAuth();
  const [guilds, setGuilds] = useState<Guild[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    if (!appSession) return;

    setLoading(true);
    setError(null);

    fetch(config.API_URL + "/guilds", {
      headers: {
        Authorization: `Bearer ${appSession}`,
      },
    })
      .then(async (res) => {
        if (!res.ok) throw new Error("Failed to fetch guilds");
        const data = await res.json();
        setGuilds(data);
      })
      .catch((err: any) => {
        setError(err.message || "Unknown error");
      })
      .finally(() => {
        setLoading(false);
      });
  }, [appSession]);

  if (!appSession) {
    return (
      <div className="section flex flex-col items-center justify-center">
        <DiscordLoginButton onLogin={handleDiscordLogin} text={t("welcome.login")} />
      </div>
    );
  }

  const onAddBotClick = (e: MouseEvent, guildId: string) => {
    e.preventDefault();
    window.open(getAddBotUrl(guildId), "_blank", "noopener,noreferrer");
  };

  const onSettingsClick = (e: MouseEvent, guildId: string) => {
    e.preventDefault();
    navigate(`/guilds/${guildId}`);
  };

  return (
    <div className="max-w-2xl mx-auto my-8">
      <h2 className="mb-4 text-2xl font-semibold">{t("guilds.title")}</h2>
      {loading && <h2 className="text-center py-2">{t("loading")}</h2>}
      {error && <h2 className="text-center py-2 text-primary">{error}</h2>}
      <ul className="space-y-4">
        {guilds.map((guild) => (
          <li
            key={guild.id}
            className="card flex items-center bg-secondary"
          >
            {guild.icon ? (
              <img
                src={`https://cdn.discordapp.com/icons/${guild.id}/${guild.icon}.webp`}
                alt={guild.name}
                className="w-10 h-10 rounded-full mr-4 object-cover bg-hover"
              />
            ) : (
              <div className="w-10 h-10 rounded-full mr-4 flex items-center justify-center font-bold text-xl">
                {guild.name[0].toUpperCase()}
              </div>
            )}
            <span className="flex-1 truncate text-base text-text">{guild.name}</span>
            {guild.botPresent ? (
              <button
                className="selected ml-4 font-semibold px-3 py-1 rounded cursor-pointer bg-hover"
                onClick={(e) => onSettingsClick(e, guild.id)}
                aria-label={t("guilds.settings")}
                title={t("guilds.settings") as string}
                type="button"
              >
                {t("guilds.settings")}
              </button>
            ) : (
              <button
                className="ml-4 bg-hover px-3 py-1 cursor-pointer rounded font-medium"
                onClick={(e) => onAddBotClick(e, guild.id)}
                aria-label={t("guilds.addBot")}
                title={t("guilds.addBot") as string}
                type="button"
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
