import { config } from "../config";

export const getDiscordAuthUrl = (): string => {
  const scope = "identify guilds";
  
  return `https://discord.com/api/oauth2/authorize?client_id=${config.CLIENT_ID}&redirect_uri=${encodeURIComponent(config.DISCORD_REDIRECT_URI)}&response_type=code&scope=${encodeURIComponent(scope)}`;
}

export const getAddBotUrl = (guildId: string) => {
  // Docs: https://discord.com/developers/docs/topics/oauth2#adding-bots-to-guilds
  const clientId = config.CLIENT_ID;
  const redirectUri = encodeURIComponent(config.DISCORD_REDIRECT_URI);
  const permissions = "8";
  const scope = "bot%20applications.commands";
  return `https://discord.com/oauth2/authorize?client_id=${clientId}&permissions=${permissions}&scope=${scope}&guild_id=${guildId}&disable_guild_select=true&response_type=code&redirect_uri=${redirectUri}`;
};