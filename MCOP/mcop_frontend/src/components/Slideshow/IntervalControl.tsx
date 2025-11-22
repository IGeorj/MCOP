import React from "react";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { Button } from "../ui/button";
import { Input } from "../ui/input";

export const IntervalControl = ({
    intervalSec,
    onIntervalChange,
}: {
    intervalSec: number;
    onIntervalChange: (value: number) => void;
}) => {
    const handleIncrement = () => onIntervalChange(Math.min(30, intervalSec + 1));
    const handleDecrement = () => onIntervalChange(Math.max(1, intervalSec - 1));
    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if(!Number(e.target.value)) return;
        const value = Math.max(1, Math.min(30, Number(e.target.value)));
        onIntervalChange(value);
    };

    return (
        <div className="flex items-center space-x-1">
            <Tooltip>
                <TooltipTrigger asChild>
                    <Button
                        variant="ghost"
                        size="sm"
                        onClick={handleDecrement}
                        className="px-2"
                    >
                        −
                    </Button>
                </TooltipTrigger>
                <TooltipContent>Decrease interval</TooltipContent>
            </Tooltip>

            <div className="relative w-12">
                <Input
                    value={intervalSec}
                    onChange={handleInputChange}
                    className="text-center pr-3 border-0 text-primary bg-navbar"
                />
            </div>

            <Tooltip>
                <TooltipTrigger asChild>
                    <Button
                        variant="ghost"
                        size="sm"
                        onClick={handleIncrement}
                        className="px-2"
                    >
                        +
                    </Button>
                </TooltipTrigger>
                <TooltipContent>Increase interval</TooltipContent>
            </Tooltip>
        </div>
    );
};