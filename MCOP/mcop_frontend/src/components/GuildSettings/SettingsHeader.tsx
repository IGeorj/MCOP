import React from 'react';
import { Button } from '@/components/ui/button';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';

interface SettingsHeaderProps {
    title: React.ReactNode;
    icon: React.ReactNode;
    tooltipText?: string;
    children?: React.ReactNode;
    className?: string;
}

export const SettingsHeader: React.FC<SettingsHeaderProps> = ({
    title,
    icon,
    tooltipText,
    children,
    className = ""
}) => {
    return (
        <div className={`flex items-center justify-between mb-4 ${className}`}>
            <div className="flex items-center gap-2">
                {tooltipText ? (
                    <TooltipProvider>
                        <Tooltip>
                            <TooltipTrigger asChild>
                                <Button variant="ghost" size="sm" className="h-8 w-8 p-0 text-default">
                                    {icon}
                                </Button>
                            </TooltipTrigger>
                            <TooltipContent>
                                <p className="max-w-[200px] text-center">{tooltipText}</p>
                            </TooltipContent>
                        </Tooltip>
                    </TooltipProvider>
                ) : (
                    <Button variant="ghost" size="sm" className="h-8 w-8 p-0 text-default">
                        {icon}
                    </Button>
                )}
                <div>
                    <h4 className="flex items-center text-center gap-2 font-semibold text-lg">
                        {title}
                    </h4>
                    {children && (
                        <>
                            {children}
                        </>
                    )}
                </div>
            </div>
        </div>
    );
};