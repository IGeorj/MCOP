import React from "react";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";

interface LevelWithProgressProps {
  level: number;
  exp: number;
  currentLevelExp: number;
  nextLevelExp: number;
}

const LevelWithProgress: React.FC<LevelWithProgressProps> = ({
  level,
  exp,
  currentLevelExp,
  nextLevelExp,
}) => {
  const progress =
    nextLevelExp > currentLevelExp
      ? Math.min(
          100,
          Math.round(
            ((exp - currentLevelExp) / (nextLevelExp - currentLevelExp)) * 100
          )
        )
      : 100;

  return (
    <TooltipProvider delayDuration={100}>
      <Tooltip>
        <TooltipTrigger asChild>
          <div
            className="relative flex flex-col items-center min-w-[60px] cursor-pointer"
            tabIndex={0}
            aria-label={`Level ${level}, progress ${progress}%`}
          >
            {/* Level badge */}
            <span
              className={"inline-flex items-center justify-center px-2 py-0.5 text-xs font-bold rounded-sm"}
              style={{ minWidth: 34 }}
            >
              {level}
            </span>
            
            {/* Progress bar with gradient fill */}
            <div className="w-14 h-1.5 mt-1 rounded-full overflow-hidden bg-muted shadow-sm">
              <div
                className="h-full transition-all duration-200"
                style={{
                  width: `${progress}%`,
                  background: "linear-gradient(90deg, #6366f1 0%, #06b6d4 100%)",
                }}
              />
            </div>
          </div>
        </TooltipTrigger>
        <TooltipContent className="text-sm text-center bg-navbar" arrowClassName="fill-navbar bg-navbar">
          <div className="font-semibold">{`Level ${level}`}</div>
          <div>{`${exp} EXP`}</div>
          <div>{`(${exp - currentLevelExp} / ${nextLevelExp - currentLevelExp})`}</div>
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
};

export default LevelWithProgress;