import React, { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Channel } from '@/types/Channel';
import { ChannelSelectPopover } from "@/components/common/ChannelSelectPopover";
import { FiEye, FiTrash2 } from "react-icons/fi";
import { Item, ItemActions, ItemContent, ItemGroup, ItemSeparator, ItemTitle } from "@/components/ui/item";
import { Spinner } from "@/components/common/Spinner";
import { channelMutations, channelQueries } from "@/api/channels";

export const ImageVerificationSettings: React.FC<{ guildId: string }> = ({ guildId }) => {
  const [selectedChannel, setSelectedChannel] = useState<Channel | null>(null);
  const queryClient = useQueryClient();

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
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-navbar p-4 rounded-lg border border-border">
          <h4 className="font-medium mb-4 flex items-center gap-2">
            <FiEye className="w-4 h-4" /> {"Verify Image Duplication"}
            {(isAllChannelsLoading || isImageVerificationLoading) && <Spinner size="sm" delay={200} />}
          </h4>
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
                    <Button variant="ghost" size="icon" className="rounded-full" onClick={() => onChannelRemoved(channelId)} >
                      <FiTrash2 className="w-4 h-4" />
                    </Button>
                  </ItemActions>
                </Item>
                {index !== imageVerificationChannelIds.length - 1 && <ItemSeparator />}
              </React.Fragment>
            ))}
          </ItemGroup>
        </div>
      </div>
    </div >
  );
};