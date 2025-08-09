import { Guild } from "@/types/Guild";
import React, { createContext, useContext, useState, ReactNode } from "react";

type GuildListContextType = {
  guilds: Guild[];
  setGuilds: React.Dispatch<React.SetStateAction<Guild[]>>;
};

const GuildListContext = createContext<GuildListContextType | undefined>(undefined);

export const useGuildList = () => {
  const context = useContext(GuildListContext);
  if (!context) {
    throw new Error("useGuildList must be used within a GuildListProvider");
  }
  return context;
};

export const GuildListProvider = ({ children }: { children: ReactNode }) => {
  const [guilds, setGuilds] = useState<Guild[]>([]);
  return (
    <GuildListContext.Provider value={{ guilds, setGuilds }}>
      {children}
    </GuildListContext.Provider>
  );
};
