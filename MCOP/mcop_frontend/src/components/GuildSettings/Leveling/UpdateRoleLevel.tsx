import { FiRefreshCw , FiChevronDown } from "react-icons/fi";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from "@/components/ui/command";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { config } from "@/config";
import { ScrollArea } from "@/components/ui/scroll-area";
import { useTranslation } from "react-i18next";

export function SetLevelRole({
  guildId,
  roles,
}: {
  guildId: string;
  roles: Role[] | undefined;
}) {
  const { t } = useTranslation();

  const queryClient = useQueryClient();
  const [selectedRole, setSelectedRole] = useState<string | null>(null);
  const [levelInput, setLevelInput] = useState<string>("");
  const [searchTerm, setSearchTerm] = useState("");
  const [open, setOpen] = useState(false);

  const { mutate: updateLevelRole } = useMutation({
    mutationFn: async ({ roleId, level }: { roleId: string; level: number }) => {
      const resp = await fetch(`${config.API_URL}/guilds/${guildId}/level-roles`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("app_session")}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ roleId, level }),
      });
      if (!resp.ok) throw new Error("Failed to update level role");
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["guildRoles", guildId] });
      setSelectedRole(null);
      setLevelInput("");
      setOpen(false);
    },
  });

  const handleAddLevelRole = () => {
    if (!selectedRole || !levelInput) return;
    const level = parseInt(levelInput);
    if (isNaN(level)) return;
    updateLevelRole({ roleId: selectedRole, level });
  };

  const filteredRoles = roles?.filter(role =>
    role.name.toLowerCase().includes(searchTerm.toLowerCase())
  ) || [];

  const selectedRoleName = roles?.find(r => r.id === selectedRole)?.name || t("leveling.selectRole");
  return (
    <div className="bg-navbar p-4 rounded-lg border border-border">
      <h4 className="font-medium mb-4 flex items-center gap-2">
        <FiRefreshCw className="w-4 h-4" /> {t("leveling.updateRoleLevel")}
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
              {selectedRoleName}
              <FiChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
            </Button>
          </PopoverTrigger>
          <PopoverContent className="w-[var(--radix-popover-trigger-width)] p-0">
            <Command>
              <div className="flex items-center px-3 bg-navbar">
                <CommandInput
                  placeholder={t("leveling.searchRoles")}
                  value={searchTerm}
                  onValueChange={setSearchTerm}
                />
              </div>
              <ScrollArea className="bg-navbar h-80 px-4 py-2">
                {filteredRoles.length === 0 ? (
                  <CommandEmpty>{t("leveling.noLevelRoles")}</CommandEmpty>
                ) : (
                  filteredRoles.map(role => (
                    <CommandItem className="cursor-pointer" key={role.id} value={role.name}  onSelect={() => {
                        setSelectedRole(role.id);
                        setOpen(false);
                      }}>
                      <div className="flex items-center">
                        <span className="mr-2 h-4 w-4 rounded-full" style={{ backgroundColor: role.color || 'transparent' }} />
                        {role.name}
                      </div>
                    </CommandItem>
                  ))
                )}
              </ScrollArea>
            </Command>
          </PopoverContent>
        </Popover>
        <Input
          type="number"
          placeholder="Required level"
          value={levelInput}
          onChange={(e) => setLevelInput(e.target.value)}
          min="1"
        />
        <Button
          onClick={handleAddLevelRole}
          disabled={!selectedRole || !levelInput}
          className={`w-full text-primary border-primary border-1 cursor-pointer hover:opacity-75`}
        >
          {t("common.update")}
        </Button>
      </div>
    </div>
  );
}