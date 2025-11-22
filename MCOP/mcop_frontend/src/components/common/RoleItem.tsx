import { Role } from "@/types/Role";
import { useTranslation } from 'react-i18next';
import React from "react";

export interface RoleItemProps {
    role: Role;
    onToggle: () => void;
    isToggling: boolean;
}

export function RoleItem({ role, onToggle }: RoleItemProps) {
    const { t } = useTranslation();

    return (
        <div onClick={onToggle} className="group/item flex items-center justify-between p-3 rounded-lg border border-transparent hover:bg-accent dark:hover:bg-accent/50 transition-all duration-200 cursor-pointer transform hover:scale-[1.01] active:opacity-70">
            <div className="flex min-h-6 flex-wrap items-center gap-3 min-w-0 flex-1">
                <div
                    className="h-3.5 w-3.5 rounded-full flex-shrink-0"
                    style={{ backgroundColor: role.color || '#6B7280' }}
                />
                <span className="font-medium break-all text-sm">{role.name}</span>
                {role.isGainExpBlocked && (
                    <div
                        className="bg-primary/10 text-primary text-xs font-medium px-2 py-1 rounded cursor-pointer hover:bg-primary/20 transition-colors group flex items-center gap-1"
                    >
                        {t("common.blocked")}
                    </div>
                )}
            </div>
        </div>
    );
}