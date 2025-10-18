import { config } from "@/config";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { FiLock, FiKey } from "react-icons/fi";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { useTranslation } from "react-i18next";
import { roleQueries } from "@/api/roles";

export function ExpBlockControl({
  guildId,
  roles,
  isLoadingRoles,
  searchTerm
}: {
  guildId: string;
  roles: Role[] | undefined;
  isLoadingRoles: boolean;
  searchTerm: string;
}) {
  const { t } = useTranslation();

  const queryClient = useQueryClient();

  const { mutate: toggleExpBlock } = useMutation({
    mutationFn: async (roleId: string) => {
      const resp = await fetch(`${config.API_URL}/guilds/${guildId}/level-roles/${roleId}/toggle-exp-block`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("app_session")}`,
          "Content-Type": "application/json",
        },
      });
      if (!resp.ok) throw new Error("Failed to toggle exp block");
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: roleQueries.getGuildRoles(guildId).queryKey
      });
    },
  });

  const filteredRoles = roles?.filter(role =>
    role.name.toLowerCase().includes(searchTerm.toLowerCase())
  ) || [];

  return (
    <div className="bg-navbar p-4 rounded-lg border border-border">
      <h4 className="font-medium mb-4 flex items-center gap-2">
        <FiLock className="w-4 h-4" /> {t("leveling.blockExperienceGain")}
      </h4>

      <div className="relative">
        {isLoadingRoles ? (
          <div className="space-y-2">
            {[...Array(3)].map((_, i) => (
              <Skeleton key={i} className="h-10 w-full" />
            ))}
          </div>
        ) : (
          <ScrollArea className="h-[300px] pr-2">
            <div className="space-y-2">
              {filteredRoles.map((role) => (
                <div key={role.id} className="flex items-center justify-between p-2 rounded hover:bg-accent dark:hover:bg-accent/50">
                  <div className="flex items-center gap-1">
                    <span className="mr-2 h-4 w-4 rounded-full" style={{ backgroundColor: role.color || 'transparent' }} />
                    <span className="truncate max-w-[180px] text-base">{role.name}</span>
                  </div>
                  <Button
                    variant={"outline"}
                    size="sm"
                    onClick={() => toggleExpBlock(role.id)}
                    className={`shrink-0 ml-2 cursor-pointer ${role.isGainExpBlocked ? "text-primary border-primary border-1" : ""}`}
                  >
                    {role.isGainExpBlocked ? <FiLock /> : <FiKey />}
                  </Button>
                </div>
              ))}
            </div>
          </ScrollArea>
        )}
      </div>
    </div>
  );
}