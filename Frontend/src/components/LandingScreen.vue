<template>
    <div class="main-container">
        <img
            src="../assets/BlueBall.png"
            class="blue-circle"
            :style="blueCircleStyle"
        >
        <HeaderComponent></HeaderComponent>
        <div class="screen-track" :style="screenTrackStyle">
            <section class="screen-section">
        <div class="content-container">
            <SplitText
                tag="h1"
                class="main-text"
                split-type="words"
                :active="animationScreen === 0"
            >
                <span style="color:#FF493B; font-weight: 700;">AI</span>-агрегатор
                <br>фриланс-заказов
                <br>для дизайнеров
            </SplitText>
            <div class="button-container">
                <RevealOnActive
                    class="button-reveal"
                    :active="animationScreen === 0"
                    :delay="0.1"
                >
                <v-btn 
                color="black" 
                size="large" 
                rounded="xl" 
                variant="flat"
                class="form-button">
                    Попробовать бесплатно
                    <template #append>
                        <img 
                        src="../assets/CoralArrow.svg" 
                        alt="" 
                        class="button-icon">
                    </template>
                </v-btn>
                </RevealOnActive>

                <RevealOnActive
                    class="button-reveal button-reveal--secondary"
                    :active="animationScreen === 0"
                    :delay="0.18"
                >
                <v-btn 
                color="#FF493B" 
                size="large" 
                rounded="xl" 
                variant="flat"
                class="form-button">
                    Узнать больше
                </v-btn>
                </RevealOnActive>
            </div>
            <!-- <RatingProof
                class="landing-rating-proof"
                :photos="ratingProofPhotos"
                text="1000+ офферов получено"
            /> -->
            <div class="info-sheets-container">
                <RevealOnActive
                    class="sheet-reveal"
                    :active="animationScreen === 0"
                    :delay="0.22"
                    :y="42"
                >
                    <BottomLeftCard :active="animationScreen === 0" />
                </RevealOnActive>
                <RevealOnActive
                    class="sheet-reveal"
                    :active="animationScreen === 0"
                    :delay="0.3"
                    :y="42"
                >
                    <BottomRightCard />
                </RevealOnActive>
            </div>
        </div>
            </section>

            <section class="screen-section">
                <HowItWorksScreen :active="animationScreen === 1" />
            </section>
            <section class="screen-section">
                <PricingScreen :active="animationScreen === 2" />
            </section>
        </div>
        <div class="screen-indicator-container">
            <ScreenIndicator
                :count="screenCount"
                :active-index="activeScreen"
                :active-color="indicatorActiveColor"
                :inactive-color="indicatorInactiveColor"
                orientation="vertical"
                spacing="1.1rem"
                base-size="0.58rem"
                :active-scale="1.64"
                @select="setScreen"
            />
        </div>
        
    </div>

</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import logoImage from '../assets/logo.png';
import BottomLeftCard from './BottomLeftCard.vue';
import BottomRightCard from './BottomRightCard.vue';
import HeaderComponent from './HeaderComponent.vue';
import HowItWorksScreen from './HowItWorksScreen.vue';
import PricingScreen from './PricingScreen.vue';
import RatingProof from './RatingProof.vue';
import RevealOnActive from './RevealOnActive.vue';
import ScreenIndicator from './ScreenIndicator.vue';
import SplitText from './SplitText.vue';

const maxScreenIndex = 2;
const screenCount = maxScreenIndex + 1;
const scrollThreshold = 90;
const transitionDuration = 900;
const animationStartDelay = transitionDuration / 2;
const inputCooldownDuration = 320;
const wheelResetDelay = 140;

const getInitialScreen = () => {
    const requestedScreen = Number(new URLSearchParams(window.location.search).get('screen'));

    if (!Number.isInteger(requestedScreen)) {
        return 0;
    }

    return Math.min(Math.max(requestedScreen, 0), maxScreenIndex);
};

const initialScreen = getInitialScreen();
const viewportHeight = ref(1);
const viewportWidth = ref(1);
const activeScreen = ref(initialScreen);
const animationScreen = ref(initialScreen);

const ratingProofPhotos = [
    logoImage,
    logoImage,
    logoImage,
    logoImage,
];

let wheelDelta = 0;
let touchStartY = 0;
let transitionTimeout = 0;
let animationStartTimeout = 0;
let wheelResetTimeout = 0;
let isInputLocked = false;
let originalBodyOverflow = '';
let originalHtmlOverflow = '';

const screenTrackStyle = computed(() => ({
    transform: `translate3d(0, ${activeScreen.value * -100}vh, 0)`,
}));

const isBlueScreen = computed(() => activeScreen.value === 1);
const indicatorActiveColor = computed(() => (isBlueScreen.value ? '#FFFFFFFF' : '#004EEAFF'));
const indicatorInactiveColor = computed(() => (isBlueScreen.value ? '#FFFFFF6B' : '#004EEA61'));

const resetScrollInput = () => {
    wheelDelta = 0;
    window.clearTimeout(wheelResetTimeout);
};

const lockScrollInput = () => {
    isInputLocked = true;
    resetScrollInput();
    window.scrollTo(0, 0);

    window.clearTimeout(transitionTimeout);
    transitionTimeout = window.setTimeout(() => {
        isInputLocked = false;
    }, transitionDuration + inputCooldownDuration);
};

const setScreen = (screenIndex) => {
    const nextScreen = Math.min(Math.max(screenIndex, 0), maxScreenIndex);

    if (isInputLocked || nextScreen === activeScreen.value) {
        return;
    }

    activeScreen.value = nextScreen;
    animationScreen.value = -1;

    window.clearTimeout(animationStartTimeout);
    animationStartTimeout = window.setTimeout(() => {
        animationScreen.value = nextScreen;
    }, animationStartDelay);

    lockScrollInput();
};

const changeScreen = (direction) => {
    setScreen(activeScreen.value + direction);
};

const handleWheel = (event) => {
    event.preventDefault();

    if (isInputLocked) {
        resetScrollInput();
        return;
    }

    wheelDelta += event.deltaY;
    window.clearTimeout(wheelResetTimeout);
    wheelResetTimeout = window.setTimeout(resetScrollInput, wheelResetDelay);

    if (Math.abs(wheelDelta) < scrollThreshold) {
        return;
    }

    changeScreen(wheelDelta > 0 ? 1 : -1);
};

const handleTouchStart = (event) => {
    touchStartY = event.touches[0]?.clientY || 0;
    resetScrollInput();
};

const handleTouchMove = (event) => {
    event.preventDefault();

    if (isInputLocked) {
        return;
    }

    const currentY = event.touches[0]?.clientY || touchStartY;
    const distance = touchStartY - currentY;

    if (Math.abs(distance) < scrollThreshold) {
        return;
    }

    changeScreen(distance > 0 ? 1 : -1);
    touchStartY = currentY;
};

const handleKeydown = (event) => {
    const downKeys = ['ArrowDown', 'PageDown', ' '];
    const upKeys = ['ArrowUp', 'PageUp'];

    if (downKeys.includes(event.key)) {
        event.preventDefault();
        changeScreen(1);
    }

    if (upKeys.includes(event.key)) {
        event.preventDefault();
        changeScreen(-1);
    }
};

const updateViewport = () => {
    viewportHeight.value = window.innerHeight || 1;
    viewportWidth.value = window.innerWidth || 1;
};

const blueCircleStyle = computed(() => {
    const circleDiameter = viewportHeight.value * 1.75;
    const viewportDiagonal = Math.hypot(viewportWidth.value, viewportHeight.value);
    const fullScreenScale = Math.max(1.85, (viewportDiagonal / circleDiameter) * 1.65);

    if (activeScreen.value === 1) {
        return {
            opacity: 1,
            transform: `translate3d(0, ${viewportHeight.value * 0.4125}px, 0) scale(${fullScreenScale})`,
        };
    }

    if (activeScreen.value === 2) {
        return {
            opacity: 0,
            transform: `translate3d(0, ${viewportHeight.value * 0.4125}px, 0) scale(${fullScreenScale})`,
        };
    }

    return {
        opacity: 1,
        transform: 'translate3d(0, 0, 0) scale(1)',
    };
});

onMounted(() => {
    updateViewport();
    window.scrollTo(0, 0);

    originalBodyOverflow = document.body.style.overflow;
    originalHtmlOverflow = document.documentElement.style.overflow;
    document.body.style.overflow = 'hidden';
    document.documentElement.style.overflow = 'hidden';

    window.addEventListener('wheel', handleWheel, { passive: false });
    window.addEventListener('touchstart', handleTouchStart, { passive: true });
    window.addEventListener('touchmove', handleTouchMove, { passive: false });
    window.addEventListener('keydown', handleKeydown);
    window.addEventListener('resize', updateViewport, { passive: true });
});

onBeforeUnmount(() => {
    document.body.style.overflow = originalBodyOverflow;
    document.documentElement.style.overflow = originalHtmlOverflow;

    window.removeEventListener('wheel', handleWheel);
    window.removeEventListener('touchstart', handleTouchStart);
    window.removeEventListener('touchmove', handleTouchMove);
    window.removeEventListener('keydown', handleKeydown);
    window.removeEventListener('resize', updateViewport);
    window.clearTimeout(transitionTimeout);
    window.clearTimeout(animationStartTimeout);
    window.clearTimeout(wheelResetTimeout);
});
</script>

<style scoped>

.main-container {
    position: relative;
    height: 100vh;
    width: 100vw;
    overflow: hidden;
    background: linear-gradient(to top, #E3ECFF,white);
}

.screen-track {
    position: relative;
    z-index: 20;
    width: 100%;
    height: 300vh;
    transition: transform 900ms cubic-bezier(0.76, 0, 0.24, 1);
    will-change: transform;
}

.screen-section {
    height: 100vh;
    width: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
    overflow: hidden;
}

.button-container {
    width: min(600px, calc(100% - 2rem));
    margin-top: 2rem;
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    gap: 1rem;
}

.button-reveal {
    flex: 1;
    min-width: 0;
}

.button-reveal--secondary {
    flex: 0.6;
}

.button-reveal :deep(.form-button) {
    width: 100%;
}

.button-icon {
    margin-left: 0.75rem;
    width: 1rem;
    height: 1rem;
}

.landing-rating-proof {
    margin-top: 2rem;
}

.info-sheets-container {
    width: min(72vw, 1120px);
    margin-top: 2rem;
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: clamp(0.85rem, 1.6vw, 1.45rem);
}

.sheet-reveal {
    min-width: 0;
}

.sheet-reveal :deep(.bottom-left-card),
.sheet-reveal :deep(.bottom-right-card) {
    height: 100%;
}

.content-container {
    margin-top : var(--header-container-height);
    width: min(100%, 1440px);
    display: flex;
    flex-direction: column;
    align-items: center;
}

.main-text {
    margin-top: 2rem;
    max-width: 100%;
    font-size: 3.5rem;
    font-weight: 500;
    line-height: 1.12;
}

.blue-circle{
    z-index:0;
    position: absolute;
    top: -78.75vh;
    left: 50%;
    height: 175vh;
    width: 175vh;
    margin-left: -87.5vh;
    aspect-ratio: 1;
    border-radius: 100%;
    filter: blur(8px);
    pointer-events: none;
    transform-origin: center center;
    transition:
        transform 900ms cubic-bezier(0.76, 0, 0.24, 1),
        opacity 520ms ease;
    will-change: transform;
}

.form-button {
  flex: 1;
  padding: 0 2rem 0 2rem;
  transition: padding 0.2s ease-in-out;
}

.form-button:hover {
  padding: 0 3rem 0 3rem;
}

.screen-indicator-container {
    position: fixed;
    z-index: 1000;
    top: 50%;
    right: clamp(0.45rem, 1.6vw, 1.5rem);
    transform: translateY(-50%);
    display: flex;
    align-items: center;
}
</style>
