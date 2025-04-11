import type {ReactNode} from 'react';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import Heading from '@theme/Heading';

import styles from './index.module.css';
import IndexTextScroller from "@site/src/components/IndexTextScroller";
import IndexBackgroundSlideshow from '@site/src/components/IndexBackgroundSlideshow';

// Function to get all media files from a directory
function getMediaFiles() {
    const images: string[] = [];
    const videos: string[] = [];

    // Base path to the static directory
    const basePath = '/img/index-slideshow/';

    // Example files - replace these with your actual files
    const exampleImages = [
        'QuestNav1732.webp',
        'QuestNav5152CloseUp.webp',
        'QuestNav2471.webp',
        'QuestNav4451Mid.webp'
    ];

    const exampleVideos = [
    ];

    // Add the base path to each filename
    exampleImages.forEach(file => {
        images.push(`${basePath}${file}`);
    });

    exampleVideos.forEach(file => {
        videos.push(`${basePath}${file}`);
    });

    return { images, videos };
}

function HomepageHeader() {
    const {siteConfig} = useDocusaurusContext();
    const { images, videos } = getMediaFiles();

    return (
        <header className={styles.heroContainer}>
            {/* Background Slideshow */}
            <IndexBackgroundSlideshow
                imageFiles={images}
                videoFiles={videos}
                duration={5000} // 5 seconds per image
            />

            <Heading as="h1" className={styles.heroTitle}>
                {siteConfig.title}
            </Heading>

            <div className={styles.textScrollerWrapper}>
                <IndexTextScroller
                    staticText="QuestNav is"
                    movingTextItems={["a winner.", "reliable.", "fast.", "accurate.", "robust.", "unique.", "new."]}
                    duration={2000}
                />
            </div>

            <p className={styles.heroSubtitle}>
                {siteConfig.tagline}
            </p>

            <Link
                className={styles.ctaButton}
                to="/docs/getting-started/about">
                Get Started
            </Link>
        </header>
    );
}

function Feature({title, description, icon}) {
    return (
        <div className={styles.featureCard}>
            <div className={styles.featureIcon}>{icon}</div>
            <h3 className={styles.featureTitle}>{title}</h3>
            <p className={styles.featureDescription}>{description}</p>
        </div>
    );
}

function HomepageFeatures() {
    const features = [
        {
            title: 'Fast & Reliable',
            icon: '⚡',
            description: 'QuestNav delivers lightning-fast performance you can rely on, every time.',
        },
        {
            title: 'Easy to Use',
            icon: '🔍',
            description: 'Simple, intuitive interface that makes navigation a breeze for everyone.',
        },
        {
            title: 'Highly Accurate',
            icon: '🎯',
            description: 'Advanced V-SLAM ensures you always get precise results.',
        },
        {
            title: 'Customizable',
            icon: '🔧',
            description: 'Tailor QuestNav to fit your specific needs and preferences.',
        },
        {
            title: 'Robust Integration',
            icon: '🔄',
            description: 'Seamlessly connects with your existing systems and workflows.',
        },
        {
            title: 'Continuous Updates',
            icon: '📈',
            description: 'Regular improvements and new features to enhance your experience.',
        },
    ];

    return (
        <section className={styles.featuresSection}>
            <div className={styles.featuresContainer}>
                <Heading as="h2" className={styles.featuresHeading}>
                    Why Choose QuestNav?
                </Heading>
                <div className={styles.featuresGrid}>
                    {features.map((feature, idx) => (
                        <Feature key={idx} {...feature} />
                    ))}
                </div>
            </div>
        </section>
    );
}

export default function Home(): ReactNode {
    const {siteConfig} = useDocusaurusContext();
    return (
        <Layout
            title={`Welcome to ${siteConfig.title}`}
            description="QuestNav - The next generation navigation solution">
            <main>
                <HomepageHeader />
                <HomepageFeatures />
            </main>
        </Layout>
    );
}