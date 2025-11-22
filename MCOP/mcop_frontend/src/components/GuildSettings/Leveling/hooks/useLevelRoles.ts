import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState, useRef } from "react";
import { roleMutations, roleQueries } from "@/api/roles";
import { useSetState } from "@/hooks/useSetState";
import { Role } from "@/types/Role";


export function useLevelRoles(roles: Role[] | undefined, guildId: string) {
  const queryClient = useQueryClient();
  const [editingRole, setEditingRole] = useState<{ roleId: string; field: "level" | "template" } | null>(null);
  const [editTemplateValue, setEditTemplateValue] = useState<string>("");
  const [editLevelValue, setEditLevelValue] = useState<string>("");
  const [isAddingRole, setIsAddingRole] = useState(false);
  const [newRole, setNewRole] = useState<{ roleId: string; level: number; template: string }>({
    roleId: "",
    level: 1,
    template: "",
  });
  const [pendingRoleIds, setPendingRoleIds] = useSetState<string>();
  
  const ignoreBlurRef = useRef(false);

  const invalidateRolesQuery = () => {
    queryClient.invalidateQueries({
      queryKey: roleQueries.getGuildRoles(guildId).queryKey
    });
  };

  const removeLevelRole = useMutation({
    ...roleMutations.removeLevelRole(guildId),
    onMutate: (roleId) => {
      setPendingRoleIds(prev => new Set(prev).add(roleId));
    },
    onSuccess: invalidateRolesQuery,
    onSettled: (_, __, roleId) => {
      setPendingRoleIds(prev => {
        const next = new Set(prev);
        next.delete(roleId);
        return next;
      });
    },
  });

  const updateRoleTemplate = useMutation({
    ...roleMutations.updateRoleTemplate(guildId),
    onMutate: async ({ roleId, template }) => {
      await queryClient.cancelQueries({ queryKey: roleQueries.getGuildRoles(guildId).queryKey });

      const previousRoles = queryClient.getQueryData<Role[]>(roleQueries.getGuildRoles(guildId).queryKey);

      queryClient.setQueryData<Role[]>(roleQueries.getGuildRoles(guildId).queryKey, (old) => {
        if (!old) return old;
        return old.map(role =>
          role.id === roleId
            ? { ...role, levelUpMessageTemplate: template }
            : role
        );
      });

      setPendingRoleIds(prev => new Set(prev).add(roleId));

      return { previousRoles, roleId };
    },
    onError: (err, variables, context) => {
      if (context?.previousRoles) {
        queryClient.setQueryData(roleQueries.getGuildRoles(guildId).queryKey, context.previousRoles);
      }
    },
    onSettled: (_, __, { roleId }) => {
      setPendingRoleIds(prev => {
        const next = new Set(prev);
        next.delete(roleId);
        return next;
      });
    },
  });

  const addOrUpdateLevelRole = useMutation({
    ...roleMutations.addOrUpdateLevelRole(guildId),
    onMutate: async ({ roleId, level }) => {
      await queryClient.cancelQueries({ queryKey: roleQueries.getGuildRoles(guildId).queryKey });

      const previousRoles = queryClient.getQueryData<Role[]>(roleQueries.getGuildRoles(guildId).queryKey);

      queryClient.setQueryData<Role[]>(roleQueries.getGuildRoles(guildId).queryKey, (old) => {
        if (!old) return old;
        return old.map(role =>
          role.id === roleId
            ? { ...role, levelToGetRole: level }
            : role
        );
      });

      setPendingRoleIds(prev => new Set(prev).add(roleId));

      return { previousRoles, roleId };
    },
    onError: (err, variables, context) => {
      if (context?.previousRoles) {
        queryClient.setQueryData(roleQueries.getGuildRoles(guildId).queryKey, context.previousRoles);
      }
    },
    onSettled: (_, __, { roleId }) => {
      setPendingRoleIds(prev => {
        const next = new Set(prev);
        next.delete(roleId);
        return next;
      });
    },
  });

  const { mutate: submitAddRoleMutation, isPending: isAddingPending } = useMutation({
    mutationFn: async ({ roleId, level, template }: { roleId: string; level: number; template: string }) => {
      await roleMutations.addOrUpdateLevelRole(guildId).mutationFn({ roleId, level });
      await roleMutations.updateRoleTemplate(guildId).mutationFn({ roleId, template });
    },
    onSuccess: () => {
      invalidateRolesQuery();
      cancelAddRole();
    },
  });

  const startEdit = (role: Role, field: "level" | "template") => {
    setEditingRole({ roleId: role.id, field });
    setEditTemplateValue(role.levelUpMessageTemplate ?? "");
    setEditLevelValue(role.levelToGetRole?.toString() ?? "");
  };

  const cancelEdit = () => {
    setEditingRole(null);
    setEditTemplateValue("");
    setEditLevelValue("");
    ignoreBlurRef.current = false;
  };

  const commitEdit = () => {
    if (!editingRole || !roles) return;

    const currentRole = roles.find(r => r.id === editingRole.roleId);
    if (!currentRole) {
      cancelEdit();
      return;
    }

    switch (editingRole.field) {
      case "template": {
        const newTemplate = editTemplateValue.trim() === "" ? null : editTemplateValue.trim();
        const currentTemplate = currentRole.levelUpMessageTemplate ?? null;

        if (newTemplate !== currentTemplate)
          updateRoleTemplate.mutate({ roleId: editingRole.roleId, template: newTemplate });
        break;
      }

      case "level": {
        const parsedLevel = parseInt(editLevelValue, 10);
        const isValidLevel = !isNaN(parsedLevel) && parsedLevel >= 1;

        if (!isValidLevel) {
          cancelEdit();
          return;
        }

        const currentLevel = currentRole.levelToGetRole ?? 1;

        if (parsedLevel !== currentLevel)
          addOrUpdateLevelRole.mutate({ roleId: editingRole.roleId, level: parsedLevel });
        break;
      }
    }

    cancelEdit();
  };

  const startAddRole = () => {
    setIsAddingRole(true);
    setNewRole({ roleId: "", level: 1, template: "" });
  };

  const cancelAddRole = () => {
    setIsAddingRole(false);
    setNewRole({ roleId: "", level: 1, template: "" });
  };

  const submitAddRole = () => {
    if (!newRole.roleId || newRole.level < 1) return;
    submitAddRoleMutation({ roleId: newRole.roleId, level: newRole.level, template: newRole.template });
  };

  const handleKeyDown = (e: React.KeyboardEvent, commitFunction: () => void) => {
    if (e.key === 'Enter') {
      commitFunction();
    } else if (e.key === 'Escape') {
      cancelEdit();
    }
  };

  const handleTemplateBlur = () => {
    if (ignoreBlurRef.current) {
      ignoreBlurRef.current = false;
    } else {
      commitEdit();
    }
  };

  const handleLevelBlur = () => {
    if (ignoreBlurRef.current) {
      ignoreBlurRef.current = false;
    } else {
      commitEdit();
    }
  };

  return {
    // State
    pendingRoleIds,
    editingRole,
    editTemplateValue,
    editLevelValue,
    isAddingRole,
    newRole,
    isAddingPending,

    // Setters
    setEditTemplateValue,
    setEditLevelValue,
    setNewRole,

    // Methods
    startEdit,
    cancelEdit,
    commitEdit,
    startAddRole,
    cancelAddRole,
    submitAddRole,
    removeLevelRole,
    handleKeyDown,
    handleTemplateBlur,
    handleLevelBlur,
    ignoreBlurRef,
  };
}