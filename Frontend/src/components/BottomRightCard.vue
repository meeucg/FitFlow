<template>
    <article class="bottom-right-card">
        <div class="upper-part">
            <div class="feed-icon"></div>

            <SplitText
                tag="h3"
                class="question-text"
                split-type="words"
                :active="active"
            >
                Все площадки в одной ленте
            </SplitText>
        </div>

        <div class="lower-part">
            <div class="carousel-content">
                <Transition name="carousel-slide" mode="out-in">
                    <div
                        :key="activeSlide"
                        class="carousel-slide"
                    >
                        <img
                            :src="currentSlide.platformLogo"
                            :alt="`${currentSlide.platform} logo`"
                            class="platform-logo"
                            @error="useFallbackLogo"
                        >

                        <div class="job-content">
                            <div class="job-topline">
                                <p class="platform-name">
                                    {{ currentSlide.platform }}
                                </p>

                                <p class="posted-ago">
                                    {{ currentSlide.postedAgo }}
                                </p>
                            </div>

                            <p class="carousel-title">
                                {{ currentSlide.title }}
                            </p>

                            <div class="carousel-text">
                                <span>{{ currentSlide.price }}</span>
                                <span>{{ currentSlide.workType }}</span>
                                <span>{{ currentSlide.category }}</span>
                                <span>{{ currentSlide.location }}</span>
                            </div>
                        </div>
                    </div>
                </Transition>
            </div>

            <ScreenIndicator
                :count="slides.length"
                :active-index="activeSlide"
                :active-color="'#004EEA'"
                :inactive-color="'#004EEA30'"
                orientation="vertical"
                spacing="0.75rem"
                base-size="0.5rem"
                :active-scale="1"
                class="carousel-indicator"
                @select="setSlide"
            />
        </div>
    </article>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import ScreenIndicator from './ScreenIndicator.vue';
import SplitText from './SplitText.vue';

const props = withDefaults(defineProps<{
    active?: boolean;
}>(), {
    active: true,
});

const fallbackLogo = new URL('../assets/logo.svg', import.meta.url).href;

const slides = [
    {
        platform: 'Upwork',
        postedAgo: 'posted 1h ago',
        platformLogo: 'https://cdn.simpleicons.org/upwork/0F8D00',
        title: 'Дизайн лендинга для стартапа',
        price: '$600',
        workType: 'Проект',
        category: 'UI/UX',
        location: 'Удалённо',
    },
    {
        platform: 'Telegram',
        postedAgo: 'posted 2h ago',
        platformLogo: 'https://cdn.simpleicons.org/telegram/1688C7',
        title: 'Оформить дизайн Telegram-бота',
        price: '45 000 ₽',
        workType: 'Фриланс',
        category: 'Product design',
        location: 'Удалённо',
    },
    {
        platform: 'Behance',
        postedAgo: 'posted 5h ago',
        platformLogo: 'https://cdn.simpleicons.org/behance/2458E8',
        title: 'Редизайн личного кабинета',
        price: '120 000 ₽',
        workType: 'Full time',
        category: 'Web design',
        location: 'Офис',
    },
];

const activeSlide = ref(1);
let carouselInterval = 0;

const currentSlide = computed(() => slides[activeSlide.value]);

const stopCarousel = () => {
    window.clearInterval(carouselInterval);
    carouselInterval = 0;
};

const startCarousel = () => {
    stopCarousel();

    if (!props.active) {
        return;
    }

    carouselInterval = window.setInterval(() => {
        activeSlide.value = (activeSlide.value + 1) % slides.length;
    }, 5500);
};

const setSlide = (slideIndex: number) => {
    activeSlide.value = slideIndex;
    startCarousel();
};

const useFallbackLogo = (event: Event) => {
    const image = event.target as HTMLImageElement;
    image.src = fallbackLogo;
};

onMounted(startCarousel);

watch(
    () => props.active,
    (isActive) => {
        if (isActive) {
            startCarousel();
            return;
        }

        stopCarousel();
    },
);

onBeforeUnmount(stopCarousel);
</script>

<style scoped>
.bottom-right-card {
    min-height: 265px;
    border-radius: 2rem;
    background: rgba(255, 255, 255, 0.95);
    backdrop-filter: blur(32px);
    box-shadow: 0 18px 54px rgba(23, 42, 97, 0.13);
    display: flex;
    flex-direction: column;
    gap: 1rem;
    padding: 1.5rem;
}

.upper-part {
    flex: 1;
    display: flex;
    flex-direction: row;
    justify-content: baseline;
    align-items: center;
    gap: 2rem;
}

.question-text {
    font-size: clamp(1rem, 1.25rem, 1.25rem);
    font-weight: 500;
    line-height: 1.2;
    max-width: 420px;
    color: black;
}

.lower-part {
    flex: 2;
    background: white;
    border-radius: 1.5rem;
    box-shadow: 0 12px 36px rgba(17, 57, 177, 0.164);
    display: flex;
    flex-direction: row;
    align-items: stretch;
    justify-content: flex-start;
    overflow: hidden;
}

.feed-icon {
    width: 4rem;
    box-sizing: border-box;
    aspect-ratio: 1;
    background: url('../assets/interests_fitflow_blue_coral.svg') center center no-repeat;
    background-size: 80%;
}

.carousel-content {
    flex: 1 1 auto;
    min-width: 0;
    min-height: 100%;
    position: relative;
    overflow: hidden;
    display: flex;
    align-items: center;
}

.carousel-slide {
    width: 100%;
    padding: 1rem 1rem 1rem 1.5rem;
    box-sizing: border-box;
    display: flex;
    align-items: flex-start;
    gap: 1rem;
}

.platform-logo {
    width: 2.6rem;
    height: 2.6rem;
    border-radius: 0.7rem;
    object-fit: contain;
    flex: 0 0 auto;
}

.job-content {
    min-width: 0;
    flex: 1 1 auto;
}

.job-topline {
    width: 100%;
    display: flex;
    align-items: baseline;
    gap: 1rem;
    margin-bottom: 0.45rem;
}

.platform-name {
    margin: 0;
    color: black;
    font-size: 0.82rem;
    font-weight: 500;
    line-height: 1;
    white-space: nowrap;
}

.posted-ago {
    margin: 0;
    color: rgba(0, 0, 0, 0.38);
    font-size: 0.82rem;
    font-weight: 400;
    line-height: 1;
    white-space: nowrap;
}

.carousel-title {
    color: black;
    font-size: 1rem;
    font-weight: 600;
    line-height: 1.2;
    margin-bottom: 0.55rem;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.carousel-text {
    color: rgba(0, 0, 0, 0.58);
    font-size: 0.85rem;
    font-weight: 400;
    line-height: 1.35;
    display: flex;
    flex-wrap: wrap;
    gap: 0.25rem 0.65rem;
}

.carousel-text span:not(:last-child)::after {
    content: "•";
    margin-left: 0.65rem;
    color: rgba(0, 0, 0, 0.38);
}

.carousel-indicator {
    flex: 0 0 auto;
    width: fit-content;
    margin-left: 1.25rem;
    margin-right: 1.5rem;
    align-self: center;
}

.carousel-slide-enter-active,
.carousel-slide-leave-active {
    transition:
        opacity 360ms ease,
        transform 360ms ease;
}

.carousel-slide-enter-from {
    opacity: 0;
    transform: translateY(10px);
}

.carousel-slide-leave-to {
    opacity: 0;
    transform: translateY(-10px);
}
</style>