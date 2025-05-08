export const getDiscordAvatarUrl = (
    userId: string,
    avatarHash: string | null,
    size: number = 128
  ): string => {
    if (avatarHash && avatarHash != "default") {
      return `https://cdn.discordapp.com/avatars/${userId}/${avatarHash}.${avatarHash.startsWith('a_') ? 'gif' : 'webp'}?size=${size}`;
    }
    
    const defaultAvatarIndex = parseInt(userId) % 5;
    return `https://cdn.discordapp.com/embed/avatars/${defaultAvatarIndex}.png?size=${size}`;
  };