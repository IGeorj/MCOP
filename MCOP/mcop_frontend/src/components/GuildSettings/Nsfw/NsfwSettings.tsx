import { useQuery } from "@tanstack/react-query";
import { SetNsfwRole } from "./SetNsfwRole";
import { channelQueries } from "@/api/channels";

export function NsfwSettings({ guildId }: { guildId: string }) {
  const { data: channels, isPending: isAllChannelsLoading } =
    useQuery(channelQueries.getAllTextChannels(guildId));

  return (
    <div className="space-y-6 p-6">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <SetNsfwRole
          guildId={guildId}
          channels={channels}
          isLoading={isAllChannelsLoading}
        />
      </div>
    </div>
  );
}