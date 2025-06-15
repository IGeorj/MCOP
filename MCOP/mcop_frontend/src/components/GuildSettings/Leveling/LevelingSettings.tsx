import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { config } from "@/config";
import { SetLevelRole } from "./SetLevelRole";
import { CurrentLevelRoles } from "./CurrentLevelRoles";
import { ExpBlockControl } from "./ExpBlockControl";
import { RoleSearch } from "./RoleSearch";

export function LevelingSettings({ guildId }: { guildId: string }) {
  const [searchTerm, setSearchTerm] = useState("");

  const { data: roles, isLoading: isLoadingRoles } = useQuery<Role[]>({
    queryKey: ["guildRoles", guildId],
    queryFn: async () => {
      const resp = await fetch(`${config.API_URL}/guilds/${guildId}/roles`, {
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
      <RoleSearch searchTerm={searchTerm} setSearchTerm={setSearchTerm} />
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <ExpBlockControl
          guildId={guildId}
          roles={roles}
          isLoadingRoles={isLoadingRoles}
          searchTerm={searchTerm}
        />
        <SetLevelRole
          guildId={guildId}
          roles={roles}
        />
      </div>
      <CurrentLevelRoles
        guildId={guildId}
        roles={roles}
        isLoadingRoles={isLoadingRoles}
        searchTerm={searchTerm}
      />
    </div>
  );
}