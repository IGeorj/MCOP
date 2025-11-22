import React from "react";
import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Channel } from '@/types/Channel';
import { ChannelSelectPopover } from "@/components/common/ChannelSelectPopover";
import { FiEye, FiTrash2, FiCheck, FiExternalLink } from "react-icons/fi";
import { Item, ItemActions, ItemContent, ItemGroup, ItemSeparator, ItemTitle } from "@/components/ui/item";
import { Spinner } from "@/components/common/Spinner";
import { channelMutations, channelQueries } from "@/api/channels";
import { useTranslation } from 'react-i18next';
import { SettingsHeader } from "../SettingsHeader";

export const ImageVerificationSettings: React.FC<{ guildId: string }> = ({ guildId }) => {
    const [selectedChannel, setSelectedChannel] = useState<Channel | null>(null);
    const queryClient = useQueryClient();

    const { t } = useTranslation();
    
    const { data: imageVerificationChannelIds, isPending: isImageVerificationLoading } =
        useQuery(channelQueries.getImageVerificationChannels(guildId));

    const { data: channels, isPending: isAllChannelsLoading } =
        useQuery(channelQueries.getAllTextChannels(guildId));

    const addChannelMutation = useMutation({
        ...channelMutations.addImageVerificationChannel(guildId),
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: channelQueries.getImageVerificationChannels(guildId).queryKey
            });
        },
    });

    const removeChannelMutation = useMutation({
        ...channelMutations.removeImageVerificationChannel(guildId),
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: channelQueries.getImageVerificationChannels(guildId).queryKey
            });
        },
    });

    const onChannelSelected = (channel: Channel | null): void => {
        if (channel) {
            setSelectedChannel(null);
            addChannelMutation.mutate(channel.id);
        }
    };

    const onChannelRemoved = (channelId: string): void => {
        removeChannelMutation.mutate(channelId);
    };

    const getChannelName = (channelId: string) => {
        const channel = channels?.find((c: Channel) => c.id === channelId);
        return channel ? channel.name : `Channel ${channelId}`;
    };

    return (
        <div className="space-y-6 p-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 bg-navbar border shadow-sm rounded-lg">
                <div className="bg-navbar p-4 rounded-lg">
                    <SettingsHeader
                        title={
                            <>
                            {t("imageVerification.title")} {(isAllChannelsLoading || isImageVerificationLoading) && <Spinner size="md" delay={200} />}
                            </>
                        }
                        icon={<FiEye className="w-4 h-4" />}
                    />
                    <ChannelSelectPopover
                        selectedChannel={selectedChannel}
                        onChannelSelect={onChannelSelected}
                        channels={channels || []}
                        isDefaultNone={false}
                    />
                    <ItemGroup>
                        {imageVerificationChannelIds?.map((channelId: string, index: number) => (
                            <React.Fragment key={channelId}>
                                <Item size="sm" className={`pl-1 pr-0 ${index == imageVerificationChannelIds.length - 1 ? "pb-0" : ""}`}>
                                    <ItemContent className="gap-1">
                                        <ItemTitle>{getChannelName(channelId)}</ItemTitle>
                                    </ItemContent>
                                    <ItemActions>
                                        <Button
                                            variant="ghost"
                                            size="sm"
                                            className="cursor-pointer"
                                            onClick={() => onChannelRemoved(channelId)}
                                            title={t("common.delete")}
                                        >
                                            <FiTrash2 />
                                        </Button>
                                    </ItemActions>
                                </Item>
                                {index !== imageVerificationChannelIds.length - 1 && <ItemSeparator />}
                            </React.Fragment>
                        ))}
                    </ItemGroup>
                </div>

                <div className="bg-navbar p-4 rounded-lg max-w-1xs">
                    <div className="flex items-start justify-between mb-3">
                        <div className="flex flex-col">
                            <h4 className="font-semibold text-lg">{t("imageVerification.matchFound")}</h4>
                            <div className="mt-2">
                                <div className="flex flex-row space-x-2">
                                    <div className="font-bold">{t("common.new")}:</div>
                                    <div>username</div>
                                </div>
                                <div className="flex flex-row space-x-2">
                                    <div className="font-bold">{t("common.old")}:</div>
                                    <div>username</div>
                                </div>
                            </div>
                            <div className="flex flex-row space-x-2">
                                <div className="font-bold">{t("imageVerification.match")}:</div>
                                <div>99,99%</div>
                            </div>
                        </div>
                        <img
                            src="/coplogo.ico"
                            alt="Thumbnail"
                            className="w-20 h-20 object-cover rounded-md"
                        />
                    </div>

                    <div className="flex space-x-2 pt-3">
                        <Button
                            variant="ghost"
                            size="sm"
                            className="flex-1 align-middle text-default border-1"
                        >
                            {t("common.new")} <FiExternalLink className="h-5 w-5 mb-0.5" />
                        </Button>
                        <Button
                            variant="ghost"
                            size="sm"
                            className="flex-1 text-default border-1"
                        >
                            {t("common.old")} <FiExternalLink className="h-5 w-5 mb-0.5" />
                        </Button>
                        <Button
                            variant="default"
                            size="sm"
                            className="flex-1 cursor-pointer bg-green-700 hover:bg-green-800 text-white truncate"
                        >
                            <FiCheck /><span className="mr-1 truncate">{t("imageVerification.understood")}</span> 
                        </Button>
                    </div>
                </div>
            </div>
        </div >
    );
};