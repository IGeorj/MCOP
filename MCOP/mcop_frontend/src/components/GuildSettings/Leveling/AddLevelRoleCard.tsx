import { Button } from "@/components/ui/button";
import { FiPlus, FiCheck, FiX, FiAward } from "react-icons/fi";
import { useTranslation } from "react-i18next";
import { RoleSelect } from "@/components/common/RoleSelect";
import { useEffect } from "react";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";

interface AddLevelRoleCardProps {
    availableRoles: Role[];
    isAddingRole: boolean;
    newRoleId: string;
    newRoleLevel: number;
    newRoleTemplate: string;
    isAdding: boolean;
    onStartAddRole: () => void;
    onCancelAddRole: () => void;
    onSubmitAddRole: () => void;
    onSetNewRoleId: (value: string) => void;
    onSetNewRoleLevel: (value: number) => void;
    onSetNewRoleTemaplte: (value: string) => void;
}

export function AddLevelRoleCard({
    availableRoles,
    isAddingRole,
    newRoleId,
    newRoleLevel,
    newRoleTemplate,
    isAdding,
    onStartAddRole,
    onCancelAddRole,
    onSubmitAddRole,
    onSetNewRoleId,
    onSetNewRoleLevel,
    onSetNewRoleTemaplte
}: AddLevelRoleCardProps) {
    const { t } = useTranslation();

    const handleRoleSelect = (roleId: string) => {
        onSetNewRoleId(roleId);
    };

    const handleKeyDown = (e: KeyboardEvent) => {
        if (e.key === 'Escape') {
            onCancelAddRole();
        }
    };

    useEffect(() => {
        if (isAddingRole) {
            document.addEventListener('keydown', handleKeyDown);

            return () => {
                document.removeEventListener('keydown', handleKeyDown);
            };
        }
    }, [isAddingRole, onCancelAddRole]);

    if (!isAddingRole) {
        return (
            <div
                className="rounded-lg border border-border p-4 hover:bg-accent dark:hover:bg-accent/50 transition-colors cursor-pointer flex flex-col items-center justify-center text-center min-h-[140px] group"
                onClick={onStartAddRole}
                title={t("common.add")}
            >
                <div className="text-muted-foreground group-hover:text-primary transition-colors mb-2">
                    <FiPlus className="w-6 h-6" />
                </div>
            </div>
        );
    }

    return (
        <div
            className="rounded-lg border border-border p-4 hover:bg-accent dark:hover:bg-accent/50 transition-colors"
        >
            <div className="flex items-center justify-between mb-2 gap-3">
                <div className="flex-1 min-w-0">
                    <RoleSelect
                        roles={availableRoles}
                        selectedRole={newRoleId}
                        onRoleSelect={handleRoleSelect}
                        placeholder="leveling.selectRole"
                        searchPlaceholder="leveling.searchRoles"
                        emptyMessage="leveling.noAvailableRoles"
                        disabled={isAdding}
                        className="h-7 text-sm border-none shadow-none focus:ring-0 p-0 hover:bg-transparent w-full"
                    />
                </div>

                {/* Level Input - фиксированной ширины */}
                <div className="flex items-center gap-1 flex-shrink-0">
                    <Input
                        autoFocus
                        type="number"
                        min="1"
                        placeholder={t("leveling.level")}
                        className="h-6 w-12 px-2 py-1 text-center text-sm font-medium"
                        value={newRoleLevel}
                        onChange={(e) => onSetNewRoleLevel(Number(e.target.value))}
                        disabled={isAdding}
                    />
                </div>
            </div>

            <div className="mb-4">
                <Label className="text-xs text-muted-foreground mb-2 block">
                    {t("leveling.messageTemplate") ?? "Message Template"}
                </Label>
                <Textarea
                    autoFocus
                    className="w-full bg-background min-h-[80px] resize-none"
                    value={newRoleTemplate}
                    onChange={(e) => onSetNewRoleTemaplte(e.target.value)}
                    placeholder={t("leveling.templatePlaceholder")}
                    disabled={isAdding}
                />
            </div>

            <div className="flex justify-end gap-1">
                <Button
                    variant="ghost"
                    size="sm"
                    className="cursor-pointer h-8 w-8 p-0"
                    onClick={onCancelAddRole}
                    disabled={isAdding}
                    title={t("common.cancel") ?? "Cancel"}
                >
                    <FiX className="w-4 h-4" />
                </Button>
                <Button
                    variant="ghost"
                    size="sm"
                    className="cursor-pointer h-8 w-8 p-0 text-green-600 hover:text-green-700 hover:bg-green-50"
                    onClick={onSubmitAddRole}
                    disabled={!newRoleId || !newRoleLevel || isAdding}
                    title={t("common.add")}
                >
                    <FiCheck className="w-4 h-4" />
                </Button>
            </div>
        </div>
    );
}