import { Button } from "@/components/ui/button";
import { FiTrash2, FiEdit, FiCheck, FiX } from "react-icons/fi";
import { useTranslation } from "react-i18next";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Input } from "@/components/ui/input";

interface LevelRoleCardProps {
    role: Role;
    editingRoleId: string | null;
    editingField: "level" | "template" | null;
    editTemplateValue: string;
    editLevelValue: string;
    onStartEditTemplate: (role: Role) => void;
    onStartEditLevel: (role: Role) => void;
    onCancelEdit: () => void;
    onCommitEditTemplate: () => void;
    onCommitEditLevel: () => void;
    onRemoveRole: (roleId: string) => void;
    onSetEditTemplateValue: (value: string) => void;
    onSetEditLevelValue: (value: string) => void;
    onHandleKeyDown: (e: React.KeyboardEvent, commitFunction: () => void) => void;
    onHandleTemplateBlur: () => void;
    onHandleLevelBlur: () => void;
    ignoreBlurRef: React.RefObject<boolean>;
}

export function LevelRoleCard({
    role,
    editingRoleId,
    editingField,
    editTemplateValue,
    editLevelValue,
    onStartEditTemplate,
    onStartEditLevel,
    onCancelEdit,
    onCommitEditTemplate,
    onCommitEditLevel,
    onRemoveRole,
    onSetEditTemplateValue,
    onSetEditLevelValue,
    onHandleKeyDown,
    onHandleTemplateBlur,
    onHandleLevelBlur,
    ignoreBlurRef,
}: LevelRoleCardProps) {
    const { t } = useTranslation();

    const isEditing = editingRoleId === role.id;
    const isEditingTemplate = isEditing && editingField === "template";
    const isEditingLevel = isEditing && editingField === "level";

    return (
        <div className="rounded-lg border border-border p-4 hover:bg-accent dark:hover:bg-accent/50 transition-colors">
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

                {/* Level Display/Edit */}
                <div className="flex items-center gap-1">
                    {isEditingLevel ? (
                        <Input
                            autoFocus
                            type="number"
                            min="1"
                            className="h-6 w-12 px-2 py-1 text-center text-sm font-medium"
                            value={editLevelValue}
                            onChange={(e) => onSetEditLevelValue(e.target.value)}
                            onBlur={onHandleLevelBlur}
                            onKeyDown={(e) => onHandleKeyDown(e, onCommitEditLevel)}
                        />
                    ) : (
                        <div
                            className="bg-primary/10 text-primary text-xs font-medium px-2 py-1 rounded cursor-pointer hover:bg-primary/20 transition-colors group flex items-center gap-1"
                            onClick={() => onStartEditLevel(role)}
                            title={t("common.clickToEdit")}
                        >
                            {t("leveling.level")} {role.levelToGetRole}
                        </div>
                    )}
                </div>
            </div>

            {/* Message Template */}
            <div className="mb-4">
                <Label className="text-xs text-muted-foreground mb-2 block">
                    {t("leveling.messageTemplate") ?? "Message Template"}
                </Label>
                {isEditingTemplate ? (
                    <Textarea
                        autoFocus
                        className="w-full bg-background min-h-[80px] resize-none"
                        value={editTemplateValue}
                        onChange={(e) => onSetEditTemplateValue(e.target.value)}
                        onBlur={onHandleTemplateBlur}
                        onKeyDown={(e) => onHandleKeyDown(e, onCommitEditTemplate)}
                        placeholder={t("leveling.templatePlaceholder")}
                    />
                ) : (
                    <div
                        className="flex min-h-[80px] w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm ring-offset-background cursor-pointer hover:bg-background transition-colors"
                        title={role.levelUpMessageTemplate ?? ""}
                        onClick={() => onStartEditTemplate(role)}
                    >
                        <div className="break-words whitespace-pre-wrap">
                            {role.levelUpMessageTemplate || (
                                <span className="text-muted-foreground italic">
                                    {t("common.clickToEdit")}
                                </span>
                            )}
                        </div>
                    </div>
                )}
            </div>

            {/* Actions */}
            <div className="flex justify-end gap-1">
                {isEditing ? (
                    <>
                        <Button
                            variant="ghost"
                            size="sm"
                            className="cursor-pointer h-8"
                            onMouseDown={() => (ignoreBlurRef.current = true)}
                            onClick={onCancelEdit}
                            title={t("common.cancel")}
                        >
                            <FiX />
                        </Button>
                        <Button
                            variant="ghost"
                            size="sm"
                            className="cursor-pointer"
                            onMouseDown={() => (ignoreBlurRef.current = true)}
                            onClick={editingField === "template" ? onCommitEditTemplate : onCommitEditLevel}
                            title={t("common.save")}
                        >
                            <FiCheck />
                        </Button>
                    </>
                ) : (
                    <Button
                        variant="ghost"
                        size="sm"
                        className="cursor-pointer"
                        onClick={() => onRemoveRole(role.id)}
                        title={t("common.delete")}
                    >
                        <FiTrash2 />
                    </Button>
                )}
            </div>
        </div>
    );
}