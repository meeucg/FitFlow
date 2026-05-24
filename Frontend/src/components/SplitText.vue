<template>
    <component
        :is="tag"
        ref="elementRef"
        class="split-text"
        :class="{ 'split-text--visible': isVisible }"
        :style="rootStyle"
    >
        <slot>{{ text }}</slot>
    </component>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { gsap } from 'gsap';
import { SplitText as GSAPSplitText } from 'gsap/SplitText';

gsap.registerPlugin(GSAPSplitText);

type TweenProps = gsap.TweenVars;
type CssLength = string | number;

const props = withDefaults(defineProps<{
    text?: string;
    tag?: string;
    active?: boolean;
    delay?: number;
    duration?: number;
    exitDuration?: number;
    ease?: string;
    exitEase?: string;
    splitType?: string;
    from?: TweenProps;
    to?: TweenProps;
    blur?: CssLength;
    textAlign?: string;
}>(), {
    text: '',
    tag: 'p',
    active: true,
    delay: 50,
    duration: 1.25,
    exitDuration: 0.55,
    ease: 'power3.out',
    exitEase: 'power3.in',
    splitType: 'words',
    blur: 4,
    textAlign: 'inherit',
});

const emit = defineEmits<{
    animationComplete: [];
}>();

const elementRef = ref<HTMLElement | null>(null);
const fontsLoaded = ref(false);
const isVisible = ref(false);

let splitInstance: SplitText | null = null;
let currentTween: gsap.core.Tween | null = null;
let targets: Element[] = [];

const textAlign = computed(() => props.textAlign);
const shouldWaitForFonts = computed(() => props.splitType.includes('lines'));
const rootStyle = computed(() => ({
    textAlign: textAlign.value,
    visibility: isVisible.value ? 'visible' : 'hidden',
}));

const toCssLength = (value: CssLength) => (
    typeof value === 'number' ? `${value}px` : value
);

const getFromVars = () => props.from || {
    opacity: 0,
    y: 40,
    filter: `blur(${toCssLength(props.blur)})`,
};

const getToVars = () => props.to || {
    opacity: 1,
    y: 0,
    filter: 'blur(0px)',
};

const resolveTargets = (split: SplitText) => {
    if (props.splitType.includes('chars') && split.chars.length) {
        return split.chars;
    }

    if (props.splitType.includes('words') && split.words.length) {
        return split.words;
    }

    if (props.splitType.includes('lines') && split.lines.length) {
        return split.lines;
    }

    return split.chars.length ? split.chars : split.words.length ? split.words : split.lines;
};

const killTween = () => {
    currentTween?.kill();
    currentTween = null;
};

const cleanupSplit = () => {
    killTween();

    if (!splitInstance) {
        return;
    }

    splitInstance.revert();
    splitInstance = null;
    targets = [];
};

const createSplit = async () => {
    await nextTick();

    const element = elementRef.value;

    if (!element) {
        return false;
    }

    cleanupSplit();

    splitInstance = new GSAPSplitText(element, {
        type: props.splitType,
        smartWrap: true,
        autoSplit: props.splitType.includes('lines'),
        linesClass: 'split-line',
        wordsClass: 'split-word',
        charsClass: 'split-char',
        reduceWhiteSpace: false,
    });
    targets = resolveTargets(splitInstance);

    gsap.set(targets, getFromVars());
    element.style.visibility = 'hidden';

    return true;
};

const animateIn = () => {
    if (!targets.length) {
        return;
    }

    killTween();
    const element = elementRef.value;

    gsap.set(targets, getFromVars());

    if (element) {
        element.style.visibility = 'visible';
    }

    isVisible.value = true;
    currentTween = gsap.fromTo(
        targets,
        getFromVars(),
        {
            ...getToVars(),
            duration: props.duration,
            ease: props.ease,
            stagger: props.delay / 1000,
            willChange: 'transform, opacity, filter',
            force3D: true,
            onComplete: () => emit('animationComplete'),
        },
    );
};

const animateOut = () => {
    if (!targets.length) {
        return;
    }

    killTween();
    currentTween = gsap.to(targets, {
        ...getFromVars(),
        duration: props.exitDuration,
        ease: props.exitEase,
        stagger: props.delay / 2000,
        willChange: 'transform, opacity, filter',
        force3D: true,
        onComplete: () => {
            elementRef.value?.style.setProperty('visibility', 'hidden');
            isVisible.value = false;
        },
    });
};

const initialize = async () => {
    if (!fontsLoaded.value) {
        return;
    }

    const splitCreated = await createSplit();

    if (!splitCreated) {
        return;
    }

    if (props.active) {
        animateIn();
    }
};

onMounted(() => {
    if (!shouldWaitForFonts.value || !('fonts' in document) || document.fonts.status === 'loaded') {
        fontsLoaded.value = true;
        return;
    }

    document.fonts.ready.then(() => {
        fontsLoaded.value = true;
    });
});

watch(fontsLoaded, initialize);

watch(
    () => [props.text, props.splitType],
    initialize,
);

watch(
    () => props.active,
    async (isActive) => {
        if (!fontsLoaded.value) {
            return;
        }

        if (!splitInstance) {
            await initialize();
            return;
        }

        if (isActive) {
            animateIn();
            return;
        }

        animateOut();
    },
);

onBeforeUnmount(cleanupSplit);
</script>

<style scoped>
.split-text {
    display: inline-block;
    visibility: hidden;
    overflow: hidden;
    white-space: normal;
    overflow-wrap: break-word;
    will-change: transform, opacity, filter;
}

.split-text--visible {
    visibility: visible;
}

:deep(.split-line),
:deep(.split-word),
:deep(.split-char) {
    display: inline-block;
    will-change: transform, opacity, filter;
}
</style>
