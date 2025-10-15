import { config } from "@/config";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { FiTrash2, FiEdit, FiCheck, FiX, FiAward } from "react-icons/fi";
import { useTranslation } from "react-i18next";
import { useRef, useState } from "react";

export function CurrentLevelRoles({
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
  const [editingRoleId, setEditingRoleId] = useState<string | null>(null);
  const [editValue, setEditValue] = useState<string>("");
  const ignoreBlurRef = useRef(false);

  const { mutate: removeLevelRole } = useMutation({
    mutationFn: async (roleId: string) => {
      const resp = await fetch(`${config.API_URL}/guilds/${guildId}/level-roles/${roleId}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("app_session")}`,
          "Content-Type": "application/json",
        },
      });
      if (!resp.ok) throw new Error("Failed to remove level role");
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["guildRoles", guildId] });
    },
  });

  const { mutate: updateRoleTemplate } = useMutation({
    mutationFn: async ({ roleId, template }: { roleId: string; template: string | null }) => {
      const resp = await fetch(`${config.API_URL}/guilds/${guildId}/level-roles/${roleId}/message-template`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("app_session")}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ template }),
      });
      if (!resp.ok) throw new Error("Failed to update role template");
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["guildRoles", guildId] });
    },
  });

  const startEdit = (role: Role) => {
    setEditingRoleId(role.id);
    setEditValue(role.levelUpMessageTemplate ?? "");
  };

  const cancelEdit = () => {
    setEditingRoleId(null);
    setEditValue("");
    ignoreBlurRef.current = false;
  };

  const commitEdit = () => {
    if (!editingRoleId) return;
    const value = editValue.trim() === "" ? null : editValue;
    updateRoleTemplate({ roleId: editingRoleId, template: value });
    setEditingRoleId(null);
  };

  const handleBlur = () => {
    if (ignoreBlurRef.current) {
      ignoreBlurRef.current = false;
      return;
    }
    commitEdit();
  };

  const filteredRoles = roles
    ?.filter(role =>
      role.levelToGetRole !== null &&
      role.name.toLowerCase().includes(searchTerm.toLowerCase())
    )
    ?.sort((a, b) => (a.levelToGetRole || 0) - (b.levelToGetRole || 0)) || [];

  return (
    <div className="bg-navbar p-4 rounded-lg border border-border">
      <h4 className="font-medium mb-4 flex items-center gap-2">
        <FiAward className="w-4 h-4" /> {t("leveling.currentLevelRoles")}
      </h4>
      
      {isLoadingRoles ? (
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
      ) : filteredRoles.length === 0 ? (
        <p className="text-muted-foreground">{t("leveling.noLevelRoles")}</p>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredRoles.map((role) => (
            <div 
              key={role.id} 
              className="rounded-lg border border-border p-4 hover:bg-accent dark:hover:bg-accent/50 transition-colors"
            >
              {/* Role Header */}
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                  <span 
                    className="h-3 w-3 rounded-full flex-shrink-0" 
                    style={{ backgroundColor: role.color || 'transparent' }} 
                  />
                  <span className="font-medium text-sm truncate" title={role.name}>
                    {role.name}
                  </span>
                </div>
                <div className="bg-primary/10 text-primary text-xs font-medium px-2 py-1 rounded">
                  {t("leveling.level")} {role.levelToGetRole}
                </div>
              </div>

              {/* Message Template */}
              <div className="mb-4">
                <label className="text-xs text-muted-foreground block mb-2">
                  {t("leveling.messageTemplate") ?? "Message Template"}
                </label>
                {editingRoleId === role.id ? (
                  <input
                    autoFocus
                    className="w-full bg-background border border-border rounded p-2 text-sm"
                    value={editValue}
                    onChange={(e) => setEditValue(e.target.value)}
                    onBlur={handleBlur}
                    placeholder={t("leveling.templatePlaceholder") ?? "e.g. {user} достиг {level} уровня и получает роль {role}!"}
                  />
                ) : (
                  <div 
                    className="border border-border rounded p-2 text-sm min-h-[42px] break-words"
                    title={role.levelUpMessageTemplate ?? ""}
                  >
                    {role.levelUpMessageTemplate || (
                      <span className="text-muted-foreground italic">
                        {""}
                      </span>
                    )}
                  </div>
                )}
              </div>

              {/* Actions */}
              <div className="flex justify-end gap-1">
                {editingRoleId === role.id ? (
                  <>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="cursor-pointer h-8 w-8 p-0"
                      onMouseDown={() => (ignoreBlurRef.current = true)}
                      onClick={() => commitEdit()}
                      title={t("common.save") ?? "Save"}
                    >
                      <FiCheck className="w-4 h-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="cursor-pointer h-8 w-8 p-0"
                      onMouseDown={() => (ignoreBlurRef.current = true)}
                      onClick={() => cancelEdit()}
                      title={t("common.cancel") ?? "Cancel"}
                    >
                      <FiX className="w-4 h-4" />
                    </Button>
                  </>
                ) : (
                  <>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="cursor-pointer h-8 w-8 p-0"
                      onClick={() => startEdit(role)}
                      title={t("common.edit") ?? "Edit"}
                    >
                      <FiEdit className="w-4 h-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="cursor-pointer h-8 w-8 p-0"
                      onClick={() => removeLevelRole(role.id)}
                      title={t("common.delete") ?? "Delete"}
                    >
                      <FiTrash2 className="w-4 h-4" />
                    </Button>
                  </>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}