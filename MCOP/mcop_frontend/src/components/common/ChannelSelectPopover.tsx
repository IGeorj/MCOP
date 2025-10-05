import React, { useState } from 'react';
import { Popover, PopoverContent, PopoverTrigger } from '@radix-ui/react-popover';
import { Channel } from "@/types/Channel";
import { Button } from "../ui/button";
import { FiChevronDown } from "react-icons/fi";
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from "../ui/command";
import { ScrollArea } from "../ui/scroll-area";
import { t } from "i18next";

interface ChannelSelectPopoverProps {
  channels: Channel[];
  selectedChannel: Channel | null;
  isDefaultNone?: boolean;
  onChannelSelect: (channel: Channel | null) => void;
}

export const ChannelSelectPopover: React.FC<ChannelSelectPopoverProps> = ({ channels, selectedChannel, onChannelSelect, isDefaultNone = true }) => {
  const [open, setOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");

  const selectedChannelName =
    selectedChannel === null && isDefaultNone
      ? t("common.none")
      : channels?.find(r => r.id === selectedChannel?.id)?.name || t("common.selectChannel");

  return (
    <Popover open={open} onOpenChange={setOpen} modal={true}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className="w-full justify-between"
        >
          {selectedChannelName}
          <FiChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[var(--radix-popover-trigger-width)] p-0">
        <Command>
          <div className="flex items-center px-3 bg-navbar">
            <CommandInput
              placeholder={t("common.searchRoles")}
              value={searchTerm}
              onValueChange={setSearchTerm}
            />
          </div>
          <ScrollArea className="bg-navbar h-80 px-4 py-2">
            <CommandItem
              className="cursor-pointer"
              key="none"
              value={t("common.none")}
              onSelect={() => {
                onChannelSelect(null);
                setOpen(false);
              }}
            >
              <div className="flex items-center">
                {t("common.none")}
              </div>
            </CommandItem>
            {channels.length === 0 ? (
              <CommandEmpty>{t("common.none")}</CommandEmpty>
            ) : (
              channels.map(channel => (
                <CommandItem className="cursor-pointer" key={channel.id} value={channel.name} onSelect={() => {
                  onChannelSelect(channel);
                  setOpen(false);
                }}>
                  <div className="flex items-center">
                    {channel.name}
                  </div>
                </CommandItem>
              ))
            )}
          </ScrollArea>
        </Command>
      </PopoverContent>
    </Popover>
  );
};