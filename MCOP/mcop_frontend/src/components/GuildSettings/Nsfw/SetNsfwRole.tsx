import { FiAward } from "react-icons/fi";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Channel } from "@/types/Channel";
import { ChannelSelectPopover } from "@/components/common/ChannelSelectPopover";
import { Spinner } from "@/components/common/Spinner";
import { channelMutations, channelQueries } from "@/api/channels";

export function SetNsfwRole({
  guildId,
  channels,
  isLoading,
}: {
  guildId: string;
  channels: Channel[] | undefined;
  isLoading: boolean | undefined;
}) {
  const { t } = useTranslation();

  const queryClient = useQueryClient();
  const [selectedChannel, setSelectedChannel] = useState<Channel | null>(null);

  useEffect(() => {
    const daily = channels?.find(channel => channel.isDailyNsfw);
    setSelectedChannel(daily ?? null);
  }, [channels, setSelectedChannel]);

  const { mutate: updateNsfwChannel, isPending } = useMutation({
    ...channelMutations.updateDailyNsfwChannel(guildId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: channelQueries.getAllTextChannels(guildId).queryKey
      });
    },
  });

  const handleSwitchNsfwChannel = (channel: Channel | null) => {
    updateNsfwChannel({ channelId: channel?.id ?? null });
  };

  return (
    <div className="bg-navbar p-4 rounded-lg border border-border">
      <h4 className="font-medium mb-4 flex items-center gap-2">
        <FiAward className="w-4 h-4" /> {t("nsfw.updateDailyNsfwChannel")}
        {(isLoading || isPending) && <Spinner size="sm" delay={200} />}
      </h4>
      <ChannelSelectPopover
        selectedChannel={selectedChannel}
        onChannelSelect={handleSwitchNsfwChannel}
        channels={channels?.filter(channel => channel.isNsfw) || []}
      />
    </div>
  );
}