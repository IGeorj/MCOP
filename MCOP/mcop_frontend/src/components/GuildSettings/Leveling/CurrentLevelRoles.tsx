import { config } from "@/config";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { FiTrash2, FiEdit, FiCheck, FiX } from "react-icons/fi";
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
      // Focus moved to action button (Save/Cancel); do not commit on blur
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
      <h4 className="font-medium mb-4">{t("leveling.currentLevelRoles")}</h4>

      {isLoadingRoles ? (
        <div className="space-y-2">
          {[...Array(5)].map((_, i) => (
            <Skeleton key={i} className="h-10 w-full" />
          ))}
        </div>
      ) : filteredRoles.length === 0 ? (
        <p className="text-muted-foreground">{t("leveling.noLevelRoles")}</p>
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t("leveling.role")}</TableHead>
              <TableHead>{t("leveling.level")}</TableHead>
              <TableHead>{t("leveling.messageTemplate") ?? "Message Template"}</TableHead>
              <TableHead>{t("leveling.actions")}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredRoles.map((role) => (
              <TableRow key={role.id} className="border-0 hover:bg-accent dark:hover:bg-accent/50">
                <TableCell className="flex items-center gap-1 text-base">
                  <span className="mr-2 h-4 w-4 rounded-full" style={{ backgroundColor: role.color || 'transparent' }} />
                  {role.name}
                </TableCell>
                <TableCell>{role.levelToGetRole}</TableCell>
                <TableCell className="max-w-[300px]">
                  {editingRoleId === role.id ? (
                    <input
                      autoFocus
                      className="w-full bg-background border border-border rounded p-1 text-sm"
                      value={editValue}
                      onChange={(e) => setEditValue(e.target.value)}
                      onBlur={handleBlur}
                      placeholder={t("leveling.templatePlaceholder") ?? "e.g. {user} достиг {level} уровня и получает роль {role}!"}
                    />
                  ) : (
                    <span className="truncate block" title={role.levelUpMessageTemplate ?? ""}>
                      {role.levelUpMessageTemplate ?? "-"}
                    </span>
                  )}
                </TableCell>
                <TableCell>
                  <div className="flex gap-1">
                    {editingRoleId === role.id ? (
                      <>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="cursor-pointer"
                          onMouseDown={() => (ignoreBlurRef.current = true)}
                          onClick={() => commitEdit()}
                          title={t("common.save") ?? "Save"}
                        >
                          <FiCheck className="w-4 h-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="cursor-pointer"
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
                          className="cursor-pointer"
                          onClick={() => startEdit(role)}
                        >
                          <FiEdit className="w-4 h-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="cursor-pointer"
                          onClick={() => removeLevelRole(role.id)}
                        >
                          <FiTrash2 className="w-4 h-4" />
                        </Button>
                      </>
                    )}
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  );
}