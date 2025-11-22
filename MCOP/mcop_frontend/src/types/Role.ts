export type Role = {
    id: string;
    name: string;
    color: string;
    iconUrl: string;
    levelToGetRole: number | null;
    isGainExpBlocked: boolean;
    levelUpMessageTemplate?: string | null;
};