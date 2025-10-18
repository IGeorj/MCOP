import { useState } from "react";
import { Link } from "react-router-dom";
import { FaTrophy } from "react-icons/fa";
import { useQuery } from "@tanstack/react-query";
import { CurrentLevelRoles } from "./CurrentLevelRoles";
import { ExpBlockControl } from "./ExpBlockControl";
import { RoleSearch } from "./RoleSearch";
import { LevelUpMessageSettings } from "./LevelUpMessageSettings";
import { useTranslation } from "react-i18next";
import { roleQueries } from "@/api/roles";

export function LevelingSettings({ guildId }: { guildId: string }) {
  const { t } = useTranslation();

  const [searchTerm, setSearchTerm] = useState("");

  const { data: roles, isPending: isLoadingRoles } =
    useQuery(roleQueries.getGuildRoles(guildId));

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
      </div>
      <RoleSearch searchTerm={searchTerm} setSearchTerm={setSearchTerm} />
      <CurrentLevelRoles
        guildId={guildId}
        roles={roles}
        isLoadingRoles={isLoadingRoles}
        searchTerm={searchTerm}
      />
      <LevelUpMessageSettings guildId={guildId} />
      <ExpBlockControl
        guildId={guildId}
        roles={roles}
        isLoadingRoles={isLoadingRoles}
        searchTerm={searchTerm}
      />
    </div>
  );
}