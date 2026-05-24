<template>
    <div class="header-container">
        <RevealOnActive
            tag="div"
            class="header-body"
            :active="true"
            :appear="animateOnMount"
            :y="-150"
            :blur="4"
            :duration="0.9"
            ease="power3.out"
        >
            <button class="logo-container" type="button" aria-label="Открыть лендинг" @click="goLanding">
                <img src="../assets/logo.svg" class="logo" alt="FitFlow logo">
                <SplitText
                    tag="h3"
                    class="text"
                    split-type="words"
                    :delay="35"
                >
                    fitflow.art
                </SplitText>
            </button>

            <div class="button-container">
                <v-btn
                    variant="text"
                    color="black"
                    rounded="xl"
                >
                    Отзывы
                </v-btn>

                <v-btn
                    variant="text"
                    color="black"
                    rounded="xl"
                >
                    Как это работает?
                </v-btn>

                <v-btn
                    variant="text"
                    color="black"
                    rounded="xl"
                >
                    Тарифы
                </v-btn>
            </div>

            <div class="sign-in-container">
                <v-btn
                    v-if="!isAuthenticated"
                    variant="flat"
                    color="black"
                    rounded="xl"
                    class="form-button"
                    @click="handleSignIn"
                >
                    Вход
                </v-btn>

                <v-btn
                    v-else
                    icon
                    variant="flat"
                    color="black"
                    class="profile-button"
                    aria-label="Открыть профиль"
                    @click="goProfile"
                >
                    <img :src="profileIcon" class="profile-icon" alt="">
                </v-btn>
            </div>
        </RevealOnActive>
    </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import profileIcon from '@/assets/profile-icon.svg';
import { isAuthenticated, login } from '@/services/auth';
import RevealOnActive from './RevealOnActive.vue';
import SplitText from './SplitText.vue';

defineProps({
    animateOnMount: {
        type: Boolean,
        default: true,
    },
});

const router = useRouter();

function handleSignIn() {
    void login();
}

function goLanding() {
    void router.push({ name: 'landing' });
}

function goProfile() {
    void router.push({ name: 'profile', hash: '#profile' });
}
</script>

<style scoped>
.text {
    color: black;
    font-weight: 500;
    user-select: none;
}

.header-body {
    max-width: 1000px;
    width: min(90%, 1000px);
    min-width: 0;
    height: calc(100% - 80px);
    margin-top: 40px;
    background: linear-gradient(
        to right in oklch,
        white, 15%,
        rgba(255,255,255,0.8),
        85%,
        white
    );
    box-sizing: border-box;
    border-radius: var(--header-container-height);
    backdrop-filter: blur(8px);
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    padding-left: 0.8rem;
    padding-right: 1.1rem;
}

.logo-container {
    height: 100%;
    width: calc(200px - 0.8rem);
    min-width: 0;
    flex: 0 1 calc(200px - 0.8rem);
    display: flex;
    flex-direction: row;
    justify-content: baseline;
    align-items: center;
    gap: 1rem;
    border: 0;
    padding: 0;
    background: transparent;
    cursor: pointer;
    font: inherit;
}

.logo {
    height: calc(100% - 1.6rem);
    aspect-ratio: 1;
    border-radius: 100%;
}

.button-container {
    display: flex;
    flex-direction: row;
    justify-content: center;
    align-items: center;
    flex: 1 1 auto;
    gap: 1rem;
    min-width: 0;
}

.sign-in-container {
    height: 100%;
    width: calc(200px - 1.1rem);
    flex: 0 1 calc(200px - 1.1rem);
    display: flex;
    flex-direction: row;
    justify-content: end;
    align-items: center;
}

.header-container {
    position: fixed;
    display: flex;
    flex-direction: row;
    justify-content: center;
    z-index: 999;
    height: var(--header-container-height);
    width: 100%;
}

.form-button {
    padding: 0 1.5rem 0 1.5rem;
    transition: padding 0.2s ease-in-out;
}

.form-button:hover {
    padding: 0 2rem 0 2rem;
}

.profile-button {
    width: auto !important;
    height: calc(100% - 1.6rem) !important;
    min-width: 0 !important;
    aspect-ratio: 1 / 1;
    border-radius: 50%;
    padding: 0;
    flex: 0 0 auto;
}

.profile-button :deep(.v-btn__content) {
    width: 100%;
    height: 100%;
}

.profile-icon {
    width: 62%;
    height: 62%;
    display: block;
    object-fit: contain;
    filter: invert(1);
}
</style>
