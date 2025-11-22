import React from "react";
import { Skeleton } from "@/components/ui/skeleton";
import { FiAward } from "react-icons/fi";
import { useTranslation } from "react-i18next";
import { useLevelRoles } from "./hooks/useLevelRoles";
import { LevelRoleCard } from "./LevelRoleCard";
import { AddLevelRoleCard } from "./AddLevelRoleCard";
import { Role } from "@/types/Role";
import { SettingsHeader } from "../SettingsHeader";

export function CurrentLevelRoles({
    guildId,
    roles,
    isLoadingRoles,
    searchTerm,
}: {
    guildId: string;
    roles: Role[] | undefined;
    isLoadingRoles: boolean;
    searchTerm: string;
}) {
    const { t } = useTranslation();
    const {
        pendingRoleIds,
        editingRole,
        editTemplateValue,
        editLevelValue,
        isAddingRole,
        newRole,
        isAddingPending,
        setNewRole,
        setEditTemplateValue,
        setEditLevelValue,
        commitEdit,
        cancelEdit,
        startEdit,
        startAddRole,
        cancelAddRole,
        submitAddRole,
        removeLevelRole,
        handleKeyDown,
        handleTemplateBlur,
        handleLevelBlur,
        ignoreBlurRef,
    } = useLevelRoles(roles, guildId);

    const availableRoles = roles?.filter(
        (role) =>
            role.levelToGetRole === null &&
            role.name.toLowerCase().includes(searchTerm.toLowerCase())
    ) || [];

    const filteredRoles = roles
        ?.filter(
            (role) =>
                role.levelToGetRole !== null &&
                role.name.toLowerCase().includes(searchTerm.toLowerCase())
        )
        .sort((a, b) => (a.levelToGetRole || 0) - (b.levelToGetRole || 0)) || [];

    const renderSkeletons = () => (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {[...Array(6)].map((_, i) => (
                <div key={i} className="bg-card rounded-lg border border-border p-4">
                    <Skeleton className="h-4 w-3/4 mb-2" />
                    <Skeleton className="h-4 w-1/2 mb-3" />
                    <Skeleton className="h-10 w-full mb-3" />
                    <div className="flex gap-2">
                        <Skeleton className="h-8 w-8 rounded" />
                        <Skeleton className="h-8 w-8 rounded" />
                    </div>
                </div>
            ))}
        </div>
    );

    const renderRolesGrid = () => (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <AddLevelRoleCard
                availableRoles={availableRoles}
                isAddingRole={isAddingRole}
                newRole={newRole}
                isAddingPending={isAddingPending}
                onStartAddRole={startAddRole}
                onCancelAddRole={cancelAddRole}
                onSubmitAddRole={submitAddRole}
                setNewRole={setNewRole}
            />

            {filteredRoles.map((role) => (
                <LevelRoleCard
                    key={role.id}
                    role={role}
                    editingRole={editingRole}
                    editTemplateValue={editTemplateValue}
                    editLevelValue={editLevelValue}
                    pendingRoleIds={pendingRoleIds}
                    onStartEdit={startEdit}
                    onCancelEdit={cancelEdit}
                    onCommitEdit={commitEdit}
                    onRemoveRole={(roleId) => removeLevelRole.mutate(roleId)}
                    onSetEditTemplateValue={setEditTemplateValue}
                    onSetEditLevelValue={setEditLevelValue}
                    onHandleKeyDown={handleKeyDown}
                    onHandleTemplateBlur={handleTemplateBlur}
                    onHandleLevelBlur={handleLevelBlur}
                    ignoreBlurRef={ignoreBlurRef}
                />
            ))}
        </div>
    );

    return (
        <div className="bg-navbar p-4 rounded-lg border border-border">
            <SettingsHeader
                title={<>{t("leveling.currentLevelRoles")}</>}
                icon={<FiAward className="w-4 h-4" />}
            />
            {isLoadingRoles ? renderSkeletons() : renderRolesGrid()}
        </div>
    );
}