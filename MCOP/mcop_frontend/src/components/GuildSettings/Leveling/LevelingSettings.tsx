import { useState } from "react";
import { Link } from "react-router-dom";
import { FaTrophy } from "react-icons/fa";
import { useQuery } from "@tanstack/react-query";
import { config } from "@/config";
import { SetLevelRole } from "./SetLevelRole";
import { CurrentLevelRoles } from "./CurrentLevelRoles";
import { ExpBlockControl } from "./ExpBlockControl";
import { RoleSearch } from "./RoleSearch";
import { useTranslation } from "react-i18next";

export function LevelingSettings({ guildId }: { guildId: string }) {
  const { t } = useTranslation();

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
      <div className="flex flex-col gap-4 mb-4">
        <Link
          to={`/leaderboard/${guildId}`}
          className="inline-flex items-center gap-2 bg-primary/10 px-4 py-2 rounded-lg font-semibold text-primary shadow-sm hover:bg-primary/20 transition"
        >
          <FaTrophy className="w-5 h-5 text-yellow-500" />
          <span>{t("leaderboard.title")}</span>
        </Link>
        <RoleSearch searchTerm={searchTerm} setSearchTerm={setSearchTerm} />
      </div>
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