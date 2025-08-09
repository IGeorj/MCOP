import { FiAward, FiChevronDown, FiPlus } from "react-icons/fi";
import { Button } from "@/components/ui/button";
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from "@/components/ui/command";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { config } from "@/config";
import { ScrollArea } from "@/components/ui/scroll-area";
import { useTranslation } from "react-i18next";
import { Channel } from "@/types/Channel";

export function SetNsfwRole({
  guildId,
  channels,
}: {
  guildId: string;
  channels: Channel[] | undefined;
}) {
  const { t } = useTranslation();

  const queryClient = useQueryClient();
  // Preselect the channel where isDailyNsfw is true, or null if none
  const [selectedChannel, setSelectedChannel] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [open, setOpen] = useState(false);
  const [userSelected, setUserSelected] = useState(false);

  useEffect(() => {
    if (channels && selectedChannel === null && !userSelected) {
      const daily = channels.find(channel => channel.isDailyNsfw)?.id ?? null;
      if (daily !== null) {
        setSelectedChannel(daily);
      }
    }
  }, [channels, selectedChannel, userSelected]);

  const { mutate: updateNsfwChannel } = useMutation({
    mutationFn: async ({ channelId }: { channelId: string | null; }) => {
      const resp = await fetch(`${config.API_URL}/guilds/${guildId}/daily-nsfw-channel`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("app_session")}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ channelId: channelId }),
      });
      if (!resp.ok) throw new Error("Failed to update level role");
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["guildChannels", guildId] });
      setOpen(false);
    },
  });

  const handleSwitchNsfwChannel = () => {
    updateNsfwChannel({ channelId: selectedChannel });
  };

  // Only show NSFW channels
  const nsfwChannels = channels?.filter(channel => channel.isNsfw) || [];
  const filteredChannels = nsfwChannels.filter(channel =>
    channel.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const selectedChannelName =
    selectedChannel === null
      ? t("common.none")
      : channels?.find(r => r.id === selectedChannel)?.name || t("common.selectChannel");

  return (
    <div className="bg-navbar p-4 rounded-lg border border-border">
      <h4 className="font-medium mb-4 flex items-center gap-2">
        <FiAward className="w-4 h-4" /> {t("nsfw.updateDailyNsfwChannel")}
      </h4>

      <div className="space-y-4">
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
                    setSelectedChannel(null);
                    setUserSelected(true);
                    setOpen(false);
                  }}
                >
                  <div className="flex items-center">
                    {t("common.none")}
                  </div>
                </CommandItem>
                {filteredChannels.length === 0 ? (
                  <CommandEmpty>{t("common.none")}</CommandEmpty>
                ) : (
                  filteredChannels.map(channel => (
                    <CommandItem className="cursor-pointer" key={channel.id} value={channel.name}  onSelect={() => {
                        setSelectedChannel(channel.id);
                        setUserSelected(true);
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

        <Button
          onClick={handleSwitchNsfwChannel}
          disabled={selectedChannel === (channels?.find(channel => channel.isDailyNsfw)?.id ?? null)}
          className={`w-full text-primary border-primary border-1 cursor-pointer`}
        >
          {t("common.update")}
        </Button>
      </div>
    </div>
  );
}