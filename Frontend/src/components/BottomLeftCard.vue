<template>
    <article class="bottom-left-card">
        <div class="upper-part">
            <div class="star-icon"></div>

            <SplitText
                tag="h3"
                class="question-text"
                split-type="words"
                :active="active"
            >
                Почему FitFlow удобнее ручного поиска?
            </SplitText>
        </div>

        <div class="lower-part">
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

            <div class="carousel-content">
                <Transition name="carousel-slide" mode="out-in">
                    <div
                        :key="activeSlide"
                        class="carousel-slide"
                    >
                        <p class="carousel-title">
                            {{ currentSlide.title }}
                        </p>

                        <p class="carousel-text">
                            {{ currentSlide.text }}
                        </p>
                    </div>
                </Transition>
            </div>
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

const slides = [
    {
        title: 'Все заказы в одном месте',
        text: 'FitFlow собирает подходящие предложения, чтобы не искать их вручную на разных площадках.',
    },
    {
        title: 'Фильтрация под дизайнера',
        text: 'Сервис помогает отсеивать нерелевантные задачи и оставлять только те, что подходят по профилю.',
    },
    {
        title: 'Быстрее к отклику',
        text: 'Вы тратите меньше времени на поиск и быстрее переходите к выбору интересного проекта.',
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
.bottom-left-card {
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
    max-width: 300px;
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

.star-icon {
    width: 4rem;
    box-sizing: border-box;
    aspect-ratio: 1;
    background: url('../assets/Stars.svg') center center no-repeat;
    background-size: contain;
}

.carousel-indicator {
    flex: 0 0 auto;
    width: fit-content;
    margin-left: 1.5rem;
    margin-right: 1.25rem;
    align-self: center;
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
    padding: 1rem 1.5rem 1rem 0;
    box-sizing: border-box;
}

.carousel-title {
    color: black;
    font-size: 1rem;
    font-weight: 600;
    line-height: 1.2;
    margin-bottom: 0.55rem;
}

.carousel-text {
    color: rgba(0, 0, 0, 0.58);
    font-size: 0.85rem;
    font-weight: 400;
    line-height: 1.35;
    max-width: 320px;
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