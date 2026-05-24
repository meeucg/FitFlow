<template>
  <v-app>
    <v-main class="app-main">
      <RouterView v-slot="{ Component, route }">
        <Transition name="route-screen" mode="out-in">
          <div :key="route.path" class="route-screen">
            <component :is="Component" />
          </div>
        </Transition>
      </RouterView>

      <Transition name="route-veil">
        <div v-if="routeTransitionActive" class="route-veil" />
      </Transition>

      <Transition name="route-loader">
        <div v-if="routeTransitionActive" class="route-loader-overlay">
          <GridLoader
            color="#004EEA"
            size="13px"
            margin="3px"
          />
        </div>
      </Transition>
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { onBeforeUnmount, ref } from 'vue'
import { RouterView, useRouter } from 'vue-router'
import GridLoader from '@/components/GridLoader.vue'

const router = useRouter()
const routeTransitionActive = ref(false)

const minimumRouteTransitionMs = 400
const maximumRouteTransitionMs = 5000
let routeTransitionStartedAt = 0
let routeTransitionId = 0
let finishTransitionTimer: ReturnType<typeof window.setTimeout> | null = null
let forceFinishTransitionTimer: ReturnType<typeof window.setTimeout> | null = null

function clearFinishTransitionTimer() {
  if (finishTransitionTimer === null) {
    return
  }

  window.clearTimeout(finishTransitionTimer)
  finishTransitionTimer = null
}

function clearForceFinishTransitionTimer() {
  if (forceFinishTransitionTimer === null) {
    return
  }

  window.clearTimeout(forceFinishTransitionTimer)
  forceFinishTransitionTimer = null
}

function startRouteTransition() {
  clearFinishTransitionTimer()
  clearForceFinishTransitionTimer()

  routeTransitionId += 1
  routeTransitionStartedAt = performance.now()
  routeTransitionActive.value = true

  const currentTransitionId = routeTransitionId

  forceFinishTransitionTimer = window.setTimeout(() => {
    if (currentTransitionId !== routeTransitionId) {
      return
    }

    routeTransitionActive.value = false
    forceFinishTransitionTimer = null
  }, maximumRouteTransitionMs)
}

function finishRouteTransition() {
  if (!routeTransitionActive.value) {
    return
  }

  const currentTransitionId = routeTransitionId

  const elapsedMs = performance.now() - routeTransitionStartedAt
  const remainingMs = Math.max(0, minimumRouteTransitionMs - elapsedMs)

  clearFinishTransitionTimer()

  if (remainingMs === 0) {
    routeTransitionActive.value = false
    clearForceFinishTransitionTimer()
    return
  }

  finishTransitionTimer = window.setTimeout(() => {
    if (currentTransitionId !== routeTransitionId) {
      return
    }

    routeTransitionActive.value = false
    finishTransitionTimer = null
    clearForceFinishTransitionTimer()
  }, remainingMs)
}

const removeBeforeGuard = router.beforeEach((to, from) => {
  const isHashOnlyNavigation = to.path === from.path
    && JSON.stringify(to.query) === JSON.stringify(from.query)
    && to.hash !== from.hash

  if (isHashOnlyNavigation) {
    return
  }

  startRouteTransition()
})

const removeAfterHook = router.afterEach(finishRouteTransition)
const removeErrorHook = router.onError(finishRouteTransition)

void router.isReady().then(finishRouteTransition)

onBeforeUnmount(() => {
  clearFinishTransitionTimer()
  clearForceFinishTransitionTimer()
  removeBeforeGuard()
  removeAfterHook()
  removeErrorHook()
})
</script>

<style>

:root {
    --header-container-height: 150px;
    font-family: 'Styrene A Web', Roboto, Arial, sans-serif;
}

html, body, div, span, applet, object, iframe,
h1, h2, h3, h4, h5, h6, p, blockquote, pre,
a, abbr, acronym, address, big, cite, code,
del, dfn, em, img, ins, kbd, q, s, samp,
small, strike, strong, sub, sup, tt, var,
b, u, i, center,
dl, dt, dd, ol, ul, li,
fieldset, form, label, legend,
table, caption, tbody, tfoot, thead, tr, th, td,
article, aside, canvas, details, embed,
figure, figcaption, footer, header, hgroup,
menu, nav, output, ruby, section, summary,
time, mark, audio, video {
  margin: 0;
  padding: 0;
  border: 0;
  font-size: 100%;
  font: inherit;
  vertical-align: baseline;
}
/* HTML5 display-role reset for older browsers */
article, aside, details, figcaption, figure,
footer, header, hgroup, menu, nav, section {
  display: block;
}
body {
  line-height: 1;
  font-family: 'Styrene A Web', Roboto, Arial, sans-serif;
}

.v-application {
  font-family: 'Styrene A Web', Roboto, Arial, sans-serif;
}

.app-main {
  position: relative;
  overflow: hidden;
}

.route-screen {
  min-height: 100vh;
  background: white;
  transform-origin: center;
  will-change: filter, opacity;
}

.route-screen-enter-active,
.route-screen-leave-active {
  transition:
    opacity 0.25s ease,
    filter 0.25s ease;
}

.route-screen-enter-from,
.route-screen-leave-to {
  opacity: 0;
  filter: blur(8px) brightness(1.72) contrast(0.82);
}

.route-screen-enter-to,
.route-screen-leave-from {
  opacity: 1;
  filter: blur(0) brightness(1) contrast(1);
}

.route-veil {
  position: fixed;
  inset: 0;
  z-index: 1000;
  pointer-events: none;
  overflow: hidden;
  background: rgba(255, 255, 255, 0.58);
  backdrop-filter: blur(8px) brightness(1.26) contrast(0.9);
  -webkit-backdrop-filter: blur(8px) brightness(1.26) contrast(0.9);
}

.route-veil::before,
.route-veil::after {
  position: absolute;
  inset: 0;
  content: '';
}

.route-veil::before {
  background: #004eea;
  opacity: 0.22;
  mix-blend-mode: color;
}

.route-veil::after {
  background:
    linear-gradient(
      180deg,
      rgba(255, 255, 255, 0.55),
      rgba(237, 244, 255, 0.72)
    );
}

.route-veil-enter-active,
.route-veil-leave-active {
  transition:
    opacity 0.25s ease,
    backdrop-filter 0.25s ease;
}

.route-veil-enter-from,
.route-veil-leave-to {
  opacity: 0;
  backdrop-filter: blur(0) brightness(1) contrast(1);
  -webkit-backdrop-filter: blur(0) brightness(1) contrast(1);
}

.route-veil-enter-to,
.route-veil-leave-from {
  opacity: 1;
  backdrop-filter: blur(8px) brightness(1.26) contrast(0.9);
  -webkit-backdrop-filter: blur(8px) brightness(1.26) contrast(0.9);
}

.route-loader-overlay {
  position: fixed;
  inset: 0;
  z-index: 1001;
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none;
}

.route-loader-enter-active,
.route-loader-leave-active {
  transition:
    opacity 0.1s ease,
    transform 0.1s ease;
}

.route-loader-enter-from,
.route-loader-leave-to {
  opacity: 0;
  transform: translateY(0.35rem);
}
ol, ul {
  list-style: none;
}
blockquote, q {
  quotes: none;
}
blockquote:before, blockquote:after,
q:before, q:after {
  content: '';
  content: none;
}
table {
  border-collapse: collapse;
  border-spacing: 0;
}
</style>
