import React, { useState, useEffect, useRef } from 'react';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import { usePluginData } from '@docusaurus/useGlobalData';
import styles from './styles.module.css';
import clsx from 'clsx';

interface AwardData {
    team_number: string;
    year: string;
    event_name: string;
    award_name: string;
}

interface AwardShowcaseProps {
    /**
     * Optional className for additional styling
     */
    className?: string;
}

/**
 * Award Showcase component that displays FRC awards won with QuestNav
 * Data is loaded from CSV files at build time
 */
const AwardShowcase: React.FC<AwardShowcaseProps> = ({
                                                         className
                                                     }) => {
    // Get build time from Docusaurus config
    const {siteConfig} = useDocusaurusContext();
    const buildTime = siteConfig.customFields?.buildTime ||
        new Date().toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });

    // Get data from plugin
    const { awardsData } = usePluginData('awards-csv-loader') as {
        awardsData: AwardData[];
    };

    // Refs
    const showcaseRef = useRef<HTMLDivElement>(null);

    // State for counters and animation
    const [awardCount, setAwardCount] = useState(0);
    const [isVisible, setIsVisible] = useState(false);
    const [hasAnimated, setHasAnimated] = useState(false);

    // Set up intersection observer to detect when component is in view
    useEffect(() => {
        // Debounce timer to prevent flickering
        let debounceTimer: NodeJS.Timeout | null = null;

        const observer = new IntersectionObserver((entries) => {
            const [entry] = entries;

            // Clear any existing timer
            if (debounceTimer) {
                clearTimeout(debounceTimer);
            }

            // Set a new timer to update visibility state
            debounceTimer = setTimeout(() => {
                // Component is visible
                if (entry.isIntersecting) {
                    setIsVisible(true);
                    if (!hasAnimated) {
                        setHasAnimated(true);
                    }
                }
                // Component is no longer visible with a threshold buffer
                else if (entry.intersectionRatio < 0.05) {
                    setIsVisible(false);
                }
            }, 100); // Small delay to prevent flickering
        }, {
            // Multiple thresholds for smoother transitions
            threshold: [0.05, 0.15, 0.5],
            // Add root margin to start transitions earlier
            rootMargin: '0px 0px -10% 0px'
        });

        if (showcaseRef.current) {
            observer.observe(showcaseRef.current);
        }

        return () => {
            if (showcaseRef.current && observer) {
                observer.unobserve(showcaseRef.current);
            }
            // Clear any remaining timers
            if (debounceTimer) {
                clearTimeout(debounceTimer);
            }
        };
    }, [hasAnimated]);

    // Handle counter animation when component becomes visible
    useEffect(() => {
        if (!isVisible || !hasAnimated) return;

        const targetAwardCount = awardsData.length;

        // Animate the counters
        const duration = 2500; // 2.5 seconds duration for more dramatic effect
        const frameRate = 30; // Updates per second
        const totalFrames = duration / (1000 / frameRate);

        let frame = 0;

        const animationInterval = setInterval(() => {
            frame++;

            if (frame <= totalFrames) {
                // Use easeOutExpo for dramatic start and smoother finish
                const progress = frame < totalFrames / 2
                    ? 4 * Math.pow(frame / totalFrames, 3)
                    : 1 - Math.pow(-2 * frame / totalFrames + 2, 3) / 2;

                setAwardCount(Math.floor(targetAwardCount * progress));
            } else {
                // Ensure we end up with exact target values
                setAwardCount(targetAwardCount);
                clearInterval(animationInterval);
            }
        }, 1000 / frameRate);

        return () => clearInterval(animationInterval);
    }, [isVisible, hasAnimated, awardsData.length]);

    // Create award items from data
    const awardItems = awardsData.map((award, i) => (
        <div key={`award-${i}`} className={styles.awardItem}>
            <div className={styles.awardIcon}>🏆</div>
            <div className={styles.awardContent}>
                <span className={styles.awardTitle}>{award.award_name}</span>
                <span className={styles.awardEvent}>
                    {award.event_name} {award.year} - Team {award.team_number}
                </span>
            </div>
        </div>
    ));

    return (
        <div className={styles.awardShowcaseWrapper}>
            <div
                ref={showcaseRef}
                className={clsx(
                    styles.awardShowcaseContainer,
                    isVisible && styles.visible,
                    className
                )}
            >
                {/* Diagonal background lines */}
                <div className={styles.diagonalLines}></div>

                {isVisible && hasAnimated && awardCount === awardsData.length && (
                    <div className={clsx(styles.pulseWaves, styles.awardWaves)}>
                        <div className={clsx(styles.wave, styles.wave1)}></div>
                        <div className={clsx(styles.wave, styles.wave2)}></div>
                        <div className={clsx(styles.wave, styles.wave3)}></div>
                    </div>
                )}

                <div className={styles.showcaseHeader}>
                    <h2 className={styles.showcaseTitle}>Built to Win.</h2>
                    <div className={styles.showcaseSubtitle}>Powering FIRST Robotics Champions</div>
                </div>

                <div className={styles.awardStatsContainer}>
                    {/* Award count section */}
                    <div className={styles.awardStatBox}>
                        <div className={clsx(
                            styles.statCount,
                            awardCount % 10 === 0 && awardCount > 0 && styles.milestone
                        )}>
                            {awardCount}
                        </div>
                        <div className={styles.statLabel}>Total Awards</div>
                        <div className={styles.statDescription}>Achieved with QuestNav</div>

                        {/* Dynamic shadow effect */}
                        <div className={clsx(styles.shadowAnimator, styles.awardShadow)}></div>

                        {/* Scrolling awards container */}
                        <div className={clsx(styles.scrollingContainer, styles.awardScroller)}>
                            <div className={styles.scrollTrack}>
                                {awardItems}
                                {awardItems} {/* Duplicate for continuous scroll */}
                            </div>
                        </div>
                    </div>
                </div>

                {/* Last updated timestamp */}
                <div className={styles.timestampContainer}>
                    <div className={styles.timestampPill}>
                        <div className={styles.liveIndicator}></div>
                        <span>Last updated: {buildTime}</span>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default AwardShowcase;