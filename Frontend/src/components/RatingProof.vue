<template>
    <section class="rating-proof" aria-label="Rating 4.9 out of 5">
        <div class="rating-proof__avatars" aria-hidden="true">
            <img
                v-for="(photo, index) in visiblePhotos"
                :key="`${photo}-${index}`"
                :src="photo"
                alt=""
                class="rating-proof__avatar"
                :style="{ zIndex: visiblePhotos.length - index }"
            >
        </div>

        <div class="rating-proof__content">
            <div class="rating-proof__rating-row">
                <div class="rating-proof__stars" aria-hidden="true">
                    <span
                        v-for="star in 5"
                        :key="star"
                        class="rating-proof__star"
                    ></span>
                </div>
                <strong class="rating-proof__score">4.9</strong>
            </div>
            <p class="rating-proof__text">{{ text }}</p>
        </div>
    </section>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = withDefaults(defineProps<{
    photos: string[];
    text: string;
    maxVisiblePhotos?: number;
}>(), {
    maxVisiblePhotos: 4,
});

const visiblePhotos = computed(() => props.photos.slice(0, props.maxVisiblePhotos));
</script>

<style scoped>
.rating-proof {
    --proof-text-size: 1rem;

    display: flex;
    align-items: center;
    justify-content: center;
    gap: calc(var(--proof-text-size) * 1.15);
    width: fit-content;
    max-width: 100%;
    color: white;
}

.rating-proof__avatars {
    display: flex;
    align-items: center;
    padding-left: calc(var(--proof-text-size) * 0.72);
}

.rating-proof__avatar {
    width: calc(var(--proof-text-size) * 1.85);
    height: calc(var(--proof-text-size) * 1.85);
    margin-left: calc(var(--proof-text-size) * -0.5);
    border: calc(var(--proof-text-size) * 0.14) solid white;
    border-radius: 50%;
    background: #dfe5f2;
    object-fit: cover;
    box-shadow: 0 0.22rem 0.5rem rgba(10, 24, 78, 0.16);
}

.rating-proof__content {
    display: flex;
    flex-direction: column;
    gap: calc(var(--proof-text-size) * 0.4);
}

.rating-proof__rating-row {
    display: flex;
    align-items: center;
    gap: calc(var(--proof-text-size) * 0.66);
}

.rating-proof__stars {
    display: flex;
    align-items: center;
    gap: calc(var(--proof-text-size) * 0.28);
}

.rating-proof__star {
    width: calc(var(--proof-text-size) * 0.84);
    height: calc(var(--proof-text-size) * 0.84);
    background: #f1cf3d;
    clip-path: polygon(
        50% 0%,
        61% 35%,
        98% 35%,
        68% 57%,
        79% 91%,
        50% 70%,
        21% 91%,
        32% 57%,
        2% 35%,
        39% 35%
    );
}

.rating-proof__score {
    color: white;
    font-size: calc(var(--proof-text-size) * 1.2);
    font-weight: 700;
    line-height: 1;
    letter-spacing: 0;
}

.rating-proof__text {
    margin: 0;
    color: rgba(255, 255, 255, 0.72);
    font-size: var(--proof-text-size);
    font-weight: 500;
    line-height: 1;
    letter-spacing: 0;
    white-space: nowrap;
}
</style>
