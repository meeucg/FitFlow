<template>
    <nav
        class="screen-indicator"
        :class="`screen-indicator--${orientation}`"
        :style="indicatorStyle"
        aria-label="Screen navigation"
    >
        <button
            v-for="dotIndex in dotIndexes"
            :key="dotIndex"
            type="button"
            class="screen-indicator__dot"
            :class="{ 'screen-indicator__dot--active': activeIndex === dotIndex }"
            :aria-label="`Go to screen ${dotIndex + 1}`"
            :aria-current="activeIndex === dotIndex ? 'step' : undefined"
            @click="emit('select', dotIndex)"
        ></button>
    </nav>
</template>

<script setup lang="ts">
import { computed } from 'vue';

type CssLength = string | number;
type IndicatorOrientation = 'vertical' | 'horizontal';

const props = withDefaults(defineProps<{
    count: number;
    activeIndex: number;
    activeColor: string;
    inactiveColor: string;
    orientation?: IndicatorOrientation;
    spacing?: CssLength;
    baseSize?: CssLength;
    activeScale?: number;
}>(), {
    orientation: 'vertical',
    spacing: '1.1rem',
    baseSize: '0.58rem',
    activeScale: 1.64,
});

const orientation = computed(() => props.orientation);

const emit = defineEmits<{
    select: [index: number];
}>();

const toCssLength = (value: CssLength) => (
    typeof value === 'number' ? `${value}px` : value
);

const dotIndexes = computed(() => (
    Array.from({ length: Math.max(0, props.count) }, (_, index) => index)
));

const indicatorStyle = computed(() => ({
    '--screen-indicator-active-color': props.activeColor,
    '--screen-indicator-inactive-color': props.inactiveColor,
    '--screen-indicator-gap': toCssLength(props.spacing),
    '--screen-indicator-base-size': toCssLength(props.baseSize),
    '--screen-indicator-active-scale': props.activeScale,
}));
</script>

<style scoped>
.screen-indicator {
    position: relative;
    z-index: 1;
    display: flex;
    align-items: center;
    gap: var(--screen-indicator-gap);
}

.screen-indicator--vertical {
    flex-direction: column;
}

.screen-indicator--horizontal {
    flex-direction: row;
}

.screen-indicator__dot {
    width: var(--screen-indicator-base-size);
    height: var(--screen-indicator-base-size);
    padding: 0;
    border: 0;
    border-radius: 50%;
    background-color: var(--screen-indicator-inactive-color);
    cursor: pointer;
    transform: scale(1);
    transition:
        transform 320ms ease,
        background-color 320ms ease,
        opacity 320ms ease;
}

.screen-indicator__dot--active {
    background-color: var(--screen-indicator-active-color);
    transform: scale(var(--screen-indicator-active-scale));
}
</style>