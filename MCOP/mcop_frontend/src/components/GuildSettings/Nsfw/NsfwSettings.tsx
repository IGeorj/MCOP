import { useQuery } from "@tanstack/react-query";
import { config } from "@/config";
import { SetNsfwRole } from "./SetNsfwRole";
import { Channel } from "@/types/Channel";

export function NsfwSettings({ guildId }: { guildId: string }) {
  const { data: channels, isLoading: isLoadingChannels } = useQuery<Channel[]>({
    queryKey: ["guildChannels", guildId],
    queryFn: async () => {
      const resp = await fetch(`${config.API_URL}/guilds/${guildId}/channels`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("app_session")}`,
        },
      });
      if (!resp.ok) throw new Error("Failed to fetch roles");
      return await resp.json();
    },
    enabled: !!guildId,
  });

  return (
    <div className="space-y-6 p-6">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <SetNsfwRole
          guildId={guildId}
          channels={channels}
        />
      </div>
    </div>
  );
}