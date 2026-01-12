import React from "react";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";
import { Guild } from "@/types/Guild";

export const GuildSelect = ({ guilds, currentGuild, onSelect }: {
  guilds: Guild[];
  currentGuild: Guild;
  onSelect: (value: string) => void;
}) => (
  <Select value={currentGuild.id} onValueChange={onSelect}>
    <SelectTrigger className="text-primary-reversed w-full border-0 bg-navbar cursor-pointer h-full!">
      <SelectValue>
        <GuildItem guild={currentGuild} size="md" />
      </SelectValue>
    </SelectTrigger>
    <SelectContent className="bg-navbar rounded-none">
      {guilds.map((g) => (
        <SelectItem value={g.id} key={g.id} className="text-primary-reversed cursor-pointer rounded-none">
          <GuildItem guild={g} size="sm" />
        </SelectItem>
      ))}
    </SelectContent>
  </Select>
);

const GuildItem = ({ guild, size = "md" }: { guild: Guild; size?: "sm" | "md" }) => {
  const iconSize = size === "sm" ? "w-5 h-5" : "w-6 h-6";
  const textSize = size === "sm" ? "text-sm" : "text-base";
  
  return (
    <div className="flex items-center gap-1.5">
      {guild.icon ? (
        <img
          src={`https://cdn.discordapp.com/icons/${guild.id}/${guild.icon}.webp`}
          alt={guild.name}
          className={`${iconSize} rounded-full mr-2 object-cover bg-hover`}
        />
      ) : (
        <div className={`${iconSize} rounded-full mr-2 flex items-center justify-center font-bold bg-gray-500`}>
          {guild.name[0].toUpperCase()}
        </div>
      )}
      <span className={cn("font-medium", textSize)}>{guild.name}</span>
    </div>
  );
};