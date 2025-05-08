export interface GuildUserStats {
    guildId: string;
    userId: string;
    username: string;
    avatarHash: string;
    duelWin: number;
    duelLose: number;
    likes: number;
    exp: number;
    currentLevelExp: number;
    nextLevelExp: number;
    winRate: number;
    level: number;
  }