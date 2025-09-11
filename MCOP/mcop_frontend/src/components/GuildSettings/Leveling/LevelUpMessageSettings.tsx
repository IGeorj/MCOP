import { useEffect, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { config } from "@/config";
import { useTranslation } from "react-i18next";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";

export function LevelUpMessageSettings({ guildId }: { guildId: string }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const { data: messageSettings } = useQuery<{ template: string | null; enabled: boolean }>({
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

  const [enabled, setEnabled] = useState<boolean>(true);
  const [template, setTemplate] = useState<string>("");

  useEffect(() => {
    if (messageSettings) {
      setEnabled(messageSettings.enabled);
      setTemplate(messageSettings.template ?? "");
    }
  }, [messageSettings]);

  const { mutate: saveTemplate, isPending: isSavingTemplate } = useMutation({
    mutationFn: async () => {
      const body = {
        enabled,
        template: template.trim() === "" ? null : template,
        templateProvided: true,
      } as { enabled: boolean; template: string | null; templateProvided: boolean };

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

  const { mutate: updateEnabled, isPending: isUpdatingEnabled } = useMutation({
    mutationFn: async (newEnabled: boolean) => {
      const body = {
        enabled: newEnabled,
        template: template.trim() === "" ? null : template,
        templateProvided: template.trim() !== "",
      } as { enabled: boolean; template: string | null; templateProvided: boolean };

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
  });

  const handleToggle = (checked: boolean) => {
    setEnabled(checked);
    updateEnabled(checked);
  };

  return (
    <div className="bg-navbar p-4 rounded-lg border border-border space-y-4">
      <h4 className="font-medium">{t("leveling.levelUpMessageSettings")}</h4>
      <div className="flex items-center gap-2">
        <Switch id="lvlup-enabled" checked={enabled} onCheckedChange={handleToggle} />
        <Label htmlFor="lvlup-enabled">{t("leveling.enableMessages")}</Label>
      </div>
      <div className="flex flex-col gap-2">
        <Label htmlFor="lvlup-template" className="text-sm text-muted-foreground">
          {t("leveling.templateHelp")}
        </Label>
        <Textarea
          id="lvlup-template"
          value={template}
          onChange={(e) => setTemplate(e.target.value)}
          placeholder={t("leveling.templatePlaceholder") ?? undefined}
          className="min-h-[100px]"
        />
      </div>
      <div>
        <Button
          onClick={() => saveTemplate()}
          disabled={messageSettings?.template == template || (!template && !messageSettings?.template) || isSavingTemplate}
          className={`w-full text-primary border-primary border-1 cursor-pointer hover:opacity-75`}
        >
          {t("common.save")}
        </Button>
      </div>
    </div>
  );
}
