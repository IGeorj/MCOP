import { Dispatch, SetStateAction, useState } from "react";

export const useSetState = <T>(initial: Set<T> = new Set()): [Set<T>, Dispatch<SetStateAction<Set<T>>>] => {
    const [set, setState] = useState<Set<T>>(initial);
    const setSetState = (action: SetStateAction<Set<T>>) => {
        if (typeof action === "function") {
            setState(prev => new Set(action(prev)));
        } else {
            setState(new Set(action));
        }
    };
    return [set, setSetState];
};
