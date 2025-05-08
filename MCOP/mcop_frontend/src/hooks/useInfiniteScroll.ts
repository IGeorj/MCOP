import { useEffect, useRef, useState, RefObject } from "react";

export default function useInfiniteScroll<T extends HTMLElement = HTMLElement>() {
  const ref = useRef<T | null>(null);
  const [isIntersecting, setIntersecting] = useState(false);

  useEffect(() => {
    const node = ref.current;
    if (!node) return;
    
    const observer = new window.IntersectionObserver(
      ([entry]) => setIntersecting(entry.isIntersecting),
      { rootMargin: "200px" }
    );
    observer.observe(node);

    return () => observer.disconnect();
  }, [ref.current]);

  return { ref: ref as RefObject<T>, isIntersecting };
}
