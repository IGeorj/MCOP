import React from "react";
import { FiAward } from "react-icons/fi";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Channel } from "@/types/Channel";
import { ChannelSelectPopover } from "@/components/common/ChannelSelectPopover";
import { Spinner } from "@/components/common/Spinner";
import { channelMutations, channelQueries } from "@/api/channels";
import { SettingsHeader } from "../SettingsHeader";

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
            <SettingsHeader
                title={
                    <>
                        {t("nsfw.updateDailyNsfwChannel")} {(isLoading || isPending) && <Spinner size="md" delay={200} />}
                    </>
                }
                icon={<FiAward className="w-4 h-4" />}
                tooltipText={t("imageVerification.title")}
            />
            <ChannelSelectPopover
                selectedChannel={selectedChannel}
                onChannelSelect={handleSwitchNsfwChannel}
                channels={channels?.filter(channel => channel.isNsfw) || []}
            />
        </div>
    );
}