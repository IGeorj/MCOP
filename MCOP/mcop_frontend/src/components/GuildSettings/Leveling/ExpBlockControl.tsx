import React, { useMemo } from "react";
import { config } from "@/config";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { FiShield, FiSearch } from "react-icons/fi";
import { Skeleton } from "@/components/ui/skeleton";
import { ScrollArea } from "@/components/ui/scroll-area";
import { useTranslation } from "react-i18next";
import { roleQueries } from "@/api/roles";
import { Role } from "@/types/Role";
import { SettingsHeader } from "../SettingsHeader";
import { RoleItem } from "@/components/common/RoleItem";

interface ExpBlockControlProps {
    guildId: string;
    roles: Role[] | undefined;
    isLoadingRoles: boolean;
    searchTerm: string;
}

export function ExpBlockControl({
    guildId,
    roles,
    isLoadingRoles,
    searchTerm
}: ExpBlockControlProps) {
    const { t } = useTranslation();
    const queryClient = useQueryClient();

    const { mutate: toggleExpBlock, isPending: isToggling } = useMutation({
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
        onMutate: async (roleId: string) => {
            await queryClient.cancelQueries({
                queryKey: roleQueries.getGuildRoles(guildId).queryKey
            });

            const previousRoles = queryClient.getQueryData<Role[]>(
                roleQueries.getGuildRoles(guildId).queryKey
            );

            if (previousRoles) {
                queryClient.setQueryData<Role[]>(
                    roleQueries.getGuildRoles(guildId).queryKey,
                    previousRoles.map(role =>
                        role.id === roleId
                            ? { ...role, isGainExpBlocked: !role.isGainExpBlocked }
                            : role
                    )
                );
            }

            return { previousRoles };
        },
        onError: (err, roleId, context) => {
            if (context?.previousRoles) {
                queryClient.setQueryData<Role[]>(
                    roleQueries.getGuildRoles(guildId).queryKey,
                    context.previousRoles
                );
            }
        }
    });

    const filteredRoles = roles?.filter(role =>
        role.name.toLowerCase().includes(searchTerm.toLowerCase())
    ) || [];

    const blockedRolesCount = useMemo(() =>
        filteredRoles.filter(role => role.isGainExpBlocked).length,
        [filteredRoles]
    );

    if (isLoadingRoles) {
        return (
            <div className="bg-card/50 backdrop-blur-sm p-6 rounded-xl border border-border/50 shadow-sm">
                <div className="flex items-center gap-3 mb-6">
                    <div className="p-2 rounded-lg bg-primary/10">
                        <FiShield className="w-5 h-5 text-primary" />
                    </div>
                    <div className="space-y-2">
                        <Skeleton className="h-5 w-40" />
                        <Skeleton className="h-3 w-24" />
                    </div>
                </div>

                <div className="space-y-3">
                    {[...Array(4)].map((_, i) => (
                        <div key={i} className="flex items-center justify-between p-3">
                            <div className="flex items-center gap-3">
                                <Skeleton className="h-8 w-8 rounded-full" />
                                <Skeleton className="h-4 w-32" />
                            </div>
                            <Skeleton className="h-9 w-9 rounded-lg" />
                        </div>
                    ))}
                </div>
            </div>
        );
    }

    return (
        <div className="group bg-navbar backdrop-blur-sm p-4 rounded-xl border border-border shadow-sm hover:shadow-md transition-all duration-300">
            <SettingsHeader
                title={t("leveling.blockExperienceGain")}
                icon={<FiShield className="w-5 h-5" />}
                tooltipText={t("leveling.blockExperienceTooltip")}
            >
                <div className="flex flex-wrap items-center justify-between">
                    <div className="flex flex-wrap items-center gap-2 text-sm text-muted-foreground">
                        <div className="flex items-center gap-1 pr-2">
                            <div className="w-2 h-2 rounded-full bg-emerald-500" />
                            <span className="lowercase">{filteredRoles.length} {t("common.total")}</span>
                        </div>
                        {blockedRolesCount > 0 && (
                            <div className="flex items-center gap-1">
                                <div className="w-2 h-2 rounded-full bg-red-400" />
                                <span className="lowercase">{blockedRolesCount} {t("common.blocked")}</span>
                            </div>
                        )}
                    </div>
                </div>
            </SettingsHeader>

            {/* Content */}
            <div className="relative">
                {filteredRoles.length === 0 ? (
                    <div className="flex flex-col items-center justify-center py-12 text-center">
                        <FiSearch className="w-12 h-12 text-muted-foreground/50 mb-4" />
                        <p className="text-muted-foreground font-medium">
                            {searchTerm ? t("common.nothingFound") : t("leveling.noRolesAvailable")}
                        </p>
                    </div>
                ) : (
                    <ScrollArea className="h-[350px]">
                        <div className="space-y-2 pr-4 px-2">
                            {filteredRoles.map((role) => (
                                <RoleItem
                                    key={role.id}
                                    role={role}
                                    onToggle={() => toggleExpBlock(role.id)}
                                    isToggling={isToggling}
                                />
                            ))}
                        </div>
                    </ScrollArea>
                )}
            </div>
        </div>
    );
}