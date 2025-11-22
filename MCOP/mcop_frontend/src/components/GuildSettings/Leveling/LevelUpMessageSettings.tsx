import React from "react";
import { useEffect, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { config } from "@/config";
import { useTranslation } from "react-i18next";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { FiMessageSquare } from "react-icons/fi";
import { Spinner } from "@/components/common/Spinner";
import { Skeleton } from "@/components/ui/skeleton";
import { SettingsHeader } from "../SettingsHeader";

export function LevelUpMessageSettings({ guildId }: { guildId: string }) {
    const { t } = useTranslation();
    const queryClient = useQueryClient();

    const { data: messageSettings, isLoading } = useQuery<{ template: string | null; enabled: boolean }>({
        queryKey: ["guildLevelUpMessageSettings", guildId],
        queryFn: async () => {
            const resp = await fetch(`${config.API_URL}/guilds/${guildId}/leveling/message-settings`, {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem("app_session")}`,
                },
            });
            if (!resp.ok) throw new Error("Failed to fetch level-up message settings");
            return await resp.json();
        },
        enabled: !!guildId,
    });

    const [localTemplate, setLocalTemplate] = useState<string>("");

    const normalizedTemplate = localTemplate || null;
    const normalizedSettingsTemplate = messageSettings?.template || null;
    const hasUnsavedChanges = normalizedTemplate !== normalizedSettingsTemplate;

    useEffect(() => {
        if (messageSettings) {
            setLocalTemplate(messageSettings.template ?? "");
        }
    }, [messageSettings]);

    const { mutate: saveSettings, isPending: isSaving } = useMutation({
        mutationFn: async (settings: { enabled?: boolean; template?: string | null }) => {
            const body = {
                enabled: settings.enabled ?? messageSettings?.enabled ?? true,
                template: settings.template?.trim() === "" ? null : settings.template,
                templateProvided: true,
            };

            const resp = await fetch(`${config.API_URL}/guilds/${guildId}/leveling/message-settings`, {
                method: "POST",
                headers: {
                    Authorization: `Bearer ${localStorage.getItem("app_session")}`,
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(body),
            });
            if (!resp.ok) throw new Error("Failed to update level-up message settings");
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["guildLevelUpMessageSettings", guildId] });
        },
    });

    const handleTemplateChange = (newTemplate: string) => {
        setLocalTemplate(newTemplate);
    };

    const handleTemplateBlur = () => {
        if (hasUnsavedChanges && localTemplate !== messageSettings?.template) {
            saveSettings({ template: localTemplate });
        }
    };

    const handleToggle = (checked: boolean) => {
        saveSettings({ enabled: checked });
    };

    if (isLoading) {
        return (
            <div className="bg-navbar p-4 rounded-lg border border-border space-y-4">
                <Skeleton className="h-6 w-[250px] mb-4" />
                <div className="flex items-center gap-2">
                    <Skeleton className="h-6 w-12 rounded-md" />
                    <Skeleton className="h-5 w-[100px]" />
                </div>
                <div className="flex flex-col gap-2">
                    <Skeleton className="h-4 w-[300px]" />
                    <Skeleton className="h-[100px] w-full rounded-md" />
                </div>
            </div>
        );
    }

    return (
        <div className="bg-navbar p-4 rounded-lg border border-border space-y-4">
            <SettingsHeader
                title={<>{t("leveling.levelUpMessageSettings")}</>}
                icon={<FiMessageSquare className="w-4 h-4 shrink-0" />}
            />
            <div className="flex items-center px-1 gap-2">
                <Switch
                    id="lvlup-enabled"
                    checked={messageSettings?.enabled ?? true}
                    onCheckedChange={handleToggle}
                />
                <Label htmlFor="lvlup-enabled">
                    {t("leveling.enableMessages")}
                    {isSaving && <Spinner size="sm" delay={200} />}
                    {hasUnsavedChanges && " • " + t("common.unsavedChanges")}
                </Label>
            </div>

            <div className="flex flex-col gap-2">
                <Label htmlFor="lvlup-template" className="text-sm text-muted-foreground">
                    {t("leveling.templateHelp")}
                </Label>
                <Textarea
                    id="lvlup-template"
                    value={localTemplate}
                    onChange={(e) => handleTemplateChange(e.target.value)}
                    onBlur={handleTemplateBlur}
                    placeholder={t("leveling.templatePlaceholder")}
                    className="min-h-[100px] focus:outline-none focus:border-primary border-border transition-colors"
                />
            </div>
        </div>
    );
}