import React, { useState, useEffect, JSX } from 'react';
import clsx from 'clsx';
import styles from './styles.module.css';

interface AnimatedTextProps {
    /**
     * Static text that doesn't change
     */
    staticText: string;

    /**
     * List of text items to cycle through
     */
    movingTextItems: string[];

    /**
     * Duration for each animation cycle in milliseconds
     */
    duration?: number;

    /**
     * Optional className for additional styling
     */
    className?: string;
}

/**
 * AnimatedText component that displays static text alongside animated moving text
 * with a slide-in from top and bounce-settle effect.
 *
 * @example
 * ```jsx
 * <AnimatedText
 *   staticText="I love"
 *   movingTextItems={['React', 'Docusaurus', 'TypeScript']}
 *   duration={2000}
 * />
 * ```
 */
export default function IndexTextScroller({
                                         staticText,
                                         movingTextItems,
                                         duration = 2000,
                                         className,
                                     }: AnimatedTextProps): JSX.Element {
    const [currentIndex, setCurrentIndex] = useState(0);
    const [isAnimating, setIsAnimating] = useState(false);

    useEffect(() => {
        if (movingTextItems.length <= 1) return;

        const animationTimeout = setTimeout(() => {
            setIsAnimating(true);

            // After animation out, change the text
            const textChangeTimeout = setTimeout(() => {
                setCurrentIndex((prevIndex) => (prevIndex + 1) % movingTextItems.length);
                setIsAnimating(false);
            }, duration / 3); // Faster fade-out (1/3 of total duration)

            return () => clearTimeout(textChangeTimeout);
        }, duration);

        return () => clearTimeout(animationTimeout);
    }, [currentIndex, duration, movingTextItems]);

    return (
        <div className={clsx(styles.container, className)}>
            <span className={styles.staticText}>{staticText}</span>
            <span
                className={clsx(
                    styles.movingText,
                    isAnimating ? styles.slideOut : styles.slideInBounce
                )}
                style={{
                    animationDuration: isAnimating ? `${duration / 3}ms` : `${duration / 2}ms`,
                }}
            >
        {movingTextItems[currentIndex]}
      </span>
        </div>
    );
}