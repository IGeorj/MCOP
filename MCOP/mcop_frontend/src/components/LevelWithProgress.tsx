import React, { useState, useRef, useLayoutEffect } from "react";

interface LevelWithProgressProps {
  level: number;
  exp: number;
  currentLevelExp: number;
  nextLevelExp: number;
}

// Utility to return a gradient based on percentage
const getProgressGradient = (percent: number): string => {
  if (percent < 33) {
    // red to yellow
    return "linear-gradient(90deg, #ed2723 0%, #edc523 100%)";
  } else if (percent < 66) {
    // yellow to orange
    return "linear-gradient(90deg, #edc523 0%, #ed8723 100%)";
  } else {
    // orange to green
    return "linear-gradient(90deg, #ed8723 0%, #23ed55 100%)";
  }
};

const LevelWithProgress: React.FC<LevelWithProgressProps> = ({
  level,
  exp,
  currentLevelExp,
  nextLevelExp,
}) => {
  const [hovered, setHovered] = useState(false);
  const [showBelow, setShowBelow] = useState(false);
  const triggerRef = useRef<HTMLDivElement>(null);

  useLayoutEffect(() => {
    if (hovered && triggerRef.current) {
      const rect = triggerRef.current.getBoundingClientRect();
      // Check if there's enough space above (tooltip height + some margin)
      if (rect.top < 75) {
        setShowBelow(true);
      } else {
        setShowBelow(false);
      }
    }
  }, [hovered]);
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
    <div
      className="relative flex flex-col items-center min-w-[60px] cursor-pointer group"
      ref={triggerRef}
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
      tabIndex={0}
      onFocus={() => setHovered(true)}
      onBlur={() => setHovered(false)}
      aria-label={`Level ${level}, progress ${progress}%`}
    >
      {/* Level badge */}
      <span
        className="inline-block px-2 py-0.5 text-xs font-bold bg-primary/90"
        style={{
          minWidth: 34,
          textAlign: "center",
        }}
      >
        {level}
      </span>
      {/* Progress bar with gradient fill */}
      <div
        className="w-14 h-1.5 mt-1 rounded-full overflow-hidden"
        style={{
          backgroundColor: "var(--color-bg)",
          boxShadow: "0 1px 3px 0 rgb(0,0,0,0.04)",
        }}
      >
        <div
          className="h-full transition-all duration-200"
          style={{
            width: `${progress}%`,
            background: getProgressGradient(progress),
          }}
        />
      </div>
      {/* Tooltip on hover/focus */}
      {hovered && (
        <div
          className={`absolute text-center z-50 left-1/2 -translate-x-1/2 bg-navbar px-3 py-2 rounded shadow-lg text-sm min-w-[130px]
            ${showBelow ? "top-[calc(100%+8px)]" : "top-[-65px]"}`}
          style={{
            pointerEvents: "none",
            border: "1px solid var(--color-primary)",
            transition: "opacity 0.15s"
          }}
        >
          <div>
            <span className="font-semibold text-primary">
              {`Level ${level}`}
            </span>
          </div>
          <div>
            {`${exp} EXP`}
          </div>
          <div>
            {`(${exp - currentLevelExp} / ${nextLevelExp - currentLevelExp})`}
          </div>
        </div>
      )}
    </div>
  );
};

export default LevelWithProgress;
