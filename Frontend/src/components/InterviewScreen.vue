<template>
  <div class="interview-screen">
    <button class="interview-logo" type="button" aria-label="Открыть лендинг" @click="goLanding">
      <img src="../assets/logo.svg" class="interview-logo-image" alt="FitFlow logo">
      <span>fitflow.art</span>
    </button>

    <v-btn
      v-if="viewState === 'conclusion' && userProfile"
      class="interview-profile-button text-none"
      rounded="xl"
      variant="text"
      @click="goProfile"
    >
      <img :src="profileIcon" class="interview-profile-icon" alt="">
      Профиль
    </v-btn>

    <PerfectScrollbar
      ref="scrollbarRef"
      class="interview-scrollbar"
      :class="{ 'interview-scrollbar--enabled': shouldUseScrollbar }"
      :options="scrollbarOptions"
    >
      <main
        class="interview-content"
        :style="contentLayoutStyle"
      >
        <div ref="contentInnerRef" class="interview-content-inner">
          <v-card
            v-if="viewState === 'loading' && !transitionOverlayVisible"
            class="status-card"
            elevation="0"
          >
            <GridLoader
              color="#004EEA"
              size="13px"
              margin="3px"
            />

            <p>Готовим интервью...</p>
          </v-card>

          <InterviewQuestionCard
            v-else-if="viewState === 'question' && currentQuestion"
            :question="currentQuestion"
            :loading="isSubmittingAnswer"
            @submit="handleSubmitAnswer"
          />

          <InterviewConclusionCard
            v-else-if="viewState === 'conclusion' && userProfile"
            :profile="userProfile"
            @view-change="handleConclusionViewChange"
          />

          <v-card
            v-else-if="viewState === 'error'"
            class="error-card"
            elevation="0"
          >
            <v-avatar class="error-icon" size="3rem">
              <v-icon icon="mdi-alert-circle-outline" />
            </v-avatar>

            <h1>Что-то пошло не так</h1>
            <p>{{ errorMessage }}</p>

            <v-btn
              color="black"
              rounded="xl"
              variant="flat"
              @click="loadInterview"
            >
              Попробовать снова
            </v-btn>
          </v-card>
        </div>
      </main>
    </PerfectScrollbar>

    <Transition name="transition-overlay">
      <div
        v-if="transitionOverlayVisible"
        class="interview-transition-overlay"
      >
        <div class="interview-transition-loader">
          <GridLoader
            color="#ffffff"
            size="13px"
            margin="3px"
          />

          <SplitText
            :key="transitionText"
            :text="transitionText"
            tag="p"
            split-type="words"
            text-align="center"
            :duration="0.8"
            :delay="35"
            :blur="6"
            class="interview-transition-text"
          />
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import type { ComponentPublicInstance } from 'vue'
import { useRouter } from 'vue-router'
import { PerfectScrollbar } from 'vue3-perfect-scrollbar'
import 'vue3-perfect-scrollbar/style.css'
import GridLoader from './GridLoader.vue'
import InterviewConclusionCard from './InterviewConclusionCard.vue'
import InterviewQuestionCard from './InterviewQuestionCard.vue'
import SplitText from './SplitText.vue'
import profileIcon from '@/assets/icons/google-account-circle.svg'
import {
  ApiError,
  getMyInterview,
  submitInterviewAnswer,
} from '@/services/interviewApi'
import type {
  AnswerDto,
  MyInterviewDisplayDto,
  QuestionDto,
  UserProfileDto,
} from '@/services/interviewApi'
import { clearRecommendationCacheToPreparing } from '@/services/recommendationsApi'

type ViewState = 'loading' | 'question' | 'conclusion' | 'error'

const router = useRouter()

const viewState = ref<ViewState>('loading')
const currentQuestion = ref<QuestionDto | null>(null)
const userProfile = ref<UserProfileDto | null>(null)
const errorMessage = ref('')
const isSubmittingAnswer = ref(false)

const scrollbarRef = ref<ComponentPublicInstance | null>(null)
const contentInnerRef = ref<HTMLElement | null>(null)

const scrollbarOverflowThresholdPx = 100
const shouldUseScrollbar = ref(false)

const contentTopOffsetPx = ref(160)
const contentBottomOffsetPx = ref(160)

const contentLayoutStyle = computed(() => ({
  '--content-top-offset': `${contentTopOffsetPx.value}px`,
  '--content-bottom-offset': `${contentBottomOffsetPx.value}px`,
}))

const scrollbarOptions = {
  suppressScrollX: true,
  wheelPropagation: false,
}

const transitionLoadingTexts = [
  'Наш ИИ-интервьюер думает...',
  'Анализируем ваш ответ...',
  'Подбираем следующий вопрос...',
  'Обновляем ваш профиль...',
  'Собираем контекст интервью...',
]

const transitionTextIndex = ref(0)
const transitionOverlayVisible = computed(() => viewState.value === 'loading' || isSubmittingAnswer.value)
const transitionText = computed(() => transitionLoadingTexts[transitionTextIndex.value])

let transitionTextTimer: ReturnType<typeof window.setInterval> | null = null
let scrollbarUpdateFrame: number | null = null
let scrollbarResizeObserver: ResizeObserver | null = null
let delayedScrollbarUpdateTimer: ReturnType<typeof window.setTimeout> | null = null

function goLanding() {
  void router.push({ name: 'landing' })
}

function goProfile() {
  void router.push({ name: 'profile', hash: '#profile' })
}

function getScrollbarElement() {
  const target = scrollbarRef.value

  if (!target) {
    return null
  }

  return target.$el instanceof HTMLElement ? target.$el : null
}

function getViewportHeight() {
  return window.visualViewport?.height ?? window.innerHeight
}

function getScrollTopOffset() {
  return window.innerWidth <= 720 ? 104 : 112
}

function getScrollBottomOffset() {
  return window.innerWidth <= 720 ? 32 : 48
}

function updateContentOffsets(contentHeight: number, nextShouldUseScrollbar: boolean) {
  const viewportHeight = getViewportHeight()

  if (nextShouldUseScrollbar) {
    contentTopOffsetPx.value = getScrollTopOffset()
    contentBottomOffsetPx.value = getScrollBottomOffset()
    return
  }

  const centeredOffset = Math.max((viewportHeight - contentHeight) / 2, 0)

  contentTopOffsetPx.value = centeredOffset
  contentBottomOffsetPx.value = centeredOffset
}

function resetScrollPosition() {
  const scrollbarElement = getScrollbarElement()

  if (scrollbarElement) {
    scrollbarElement.scrollTop = 0
  }

  if (document.scrollingElement) {
    document.scrollingElement.scrollTop = 0
  }
}

function updatePerfectScrollbarInstance() {
  const scrollbarInstance = scrollbarRef.value as ComponentPublicInstance & {
    ps?: {
      update?: () => void
    }
    update?: () => void
  }

  scrollbarInstance?.ps?.update?.()
  scrollbarInstance?.update?.()
}

function updateScrollbarState() {
  scrollbarUpdateFrame = null

  const contentInnerElement = contentInnerRef.value

  if (!contentInnerElement) {
    return
  }

  const contentHeight = contentInnerElement.getBoundingClientRect().height
  const overflowAmount = contentHeight - getViewportHeight()
  const nextShouldUseScrollbar = overflowAmount > scrollbarOverflowThresholdPx

  updateContentOffsets(contentHeight, nextShouldUseScrollbar)

  if (shouldUseScrollbar.value !== nextShouldUseScrollbar) {
    shouldUseScrollbar.value = nextShouldUseScrollbar

    void nextTick(() => {
      updatePerfectScrollbarInstance()

      if (!nextShouldUseScrollbar) {
        resetScrollPosition()
      }
    })

    return
  }

  if (!nextShouldUseScrollbar) {
    resetScrollPosition()
  }

  updatePerfectScrollbarInstance()
}

function scheduleScrollbarUpdate() {
  if (scrollbarUpdateFrame !== null) {
    window.cancelAnimationFrame(scrollbarUpdateFrame)
  }

  scrollbarUpdateFrame = window.requestAnimationFrame(updateScrollbarState)
}

function scheduleDelayedScrollbarUpdate() {
  if (delayedScrollbarUpdateTimer !== null) {
    window.clearTimeout(delayedScrollbarUpdateTimer)
  }

  scheduleScrollbarUpdate()

  delayedScrollbarUpdateTimer = window.setTimeout(() => {
    scheduleScrollbarUpdate()
  }, 260)
}

function connectScrollbarResizeObserver() {
  scrollbarResizeObserver?.disconnect()

  if (typeof ResizeObserver === 'undefined') {
    return
  }

  const scrollbarElement = getScrollbarElement()
  const contentInnerElement = contentInnerRef.value

  if (!scrollbarElement || !contentInnerElement) {
    return
  }

  scrollbarResizeObserver = new ResizeObserver(scheduleScrollbarUpdate)
  scrollbarResizeObserver.observe(scrollbarElement)
  scrollbarResizeObserver.observe(contentInnerElement)
}

function handleConclusionViewChange(isDetailed: boolean) {
  void nextTick(() => {
    if (!isDetailed) {
      resetScrollPosition()
    }

    scheduleDelayedScrollbarUpdate()
  })
}

function startTransitionTextTimer() {
  stopTransitionTextTimer()

  transitionTextIndex.value = 0

  transitionTextTimer = window.setInterval(() => {
    transitionTextIndex.value = (transitionTextIndex.value + 1) % transitionLoadingTexts.length
  }, 2200)
}

function stopTransitionTextTimer() {
  if (transitionTextTimer === null) {
    return
  }

  window.clearInterval(transitionTextTimer)
  transitionTextTimer = null
}

function setError(error: unknown) {
  if (error instanceof ApiError && error.status === 401) {
    errorMessage.value = 'Сессия истекла. Сейчас перенаправим вас на вход.'
    viewState.value = 'error'
    return
  }

  if (error instanceof ApiError && error.message) {
    errorMessage.value = error.message
    viewState.value = 'error'
    return
  }

  errorMessage.value = 'Не удалось связаться с сервером интервью. Проверьте, что backend запущен, и попробуйте еще раз.'
  viewState.value = 'error'
}

function applyInterviewState(interview: MyInterviewDisplayDto) {
  if (interview.conclusion) {
    userProfile.value = interview.conclusion
    currentQuestion.value = null
    viewState.value = 'conclusion'
    return
  }

  if (interview.currentQuestion) {
    currentQuestion.value = interview.currentQuestion
    userProfile.value = null
    viewState.value = 'question'
    return
  }

  errorMessage.value = 'Сервер не вернул следующий вопрос. Попробуйте обновить интервью.'
  viewState.value = 'error'
}

async function loadInterview() {
  viewState.value = 'loading'
  currentQuestion.value = null
  userProfile.value = null
  errorMessage.value = ''

  try {
    applyInterviewState(await getMyInterview())
  } catch (error) {
    setError(error)
  } finally {
    void nextTick(() => {
      connectScrollbarResizeObserver()
      scheduleDelayedScrollbarUpdate()
    })
  }
}

async function handleSubmitAnswer(answer: AnswerDto) {
  isSubmittingAnswer.value = true
  errorMessage.value = ''

  try {
    const response = await submitInterviewAnswer(answer)

    if (response.userProfile) {
      await clearRecommendationCacheToPreparing()
      userProfile.value = response.userProfile
      currentQuestion.value = null
      viewState.value = 'conclusion'
      return
    }

    if (response.question) {
      currentQuestion.value = response.question
      userProfile.value = null
      viewState.value = 'question'
      return
    }

    errorMessage.value = 'Сервер принял ответ, но не вернул следующий шаг.'
    viewState.value = 'error'
  } catch (error) {
    setError(error)
  } finally {
    isSubmittingAnswer.value = false

    void nextTick(() => {
      connectScrollbarResizeObserver()
      scheduleDelayedScrollbarUpdate()
    })
  }
}

watch(
  transitionOverlayVisible,
  (isVisible) => {
    if (isVisible) {
      startTransitionTextTimer()
      return
    }

    stopTransitionTextTimer()
  },
  { immediate: true },
)

watch(
  [viewState, currentQuestion, userProfile],
  () => {
    void nextTick(() => {
      connectScrollbarResizeObserver()
      scheduleDelayedScrollbarUpdate()
    })
  },
  { flush: 'post' },
)

onMounted(() => {
  void loadInterview()

  window.addEventListener('resize', scheduleDelayedScrollbarUpdate)

  void nextTick(() => {
    connectScrollbarResizeObserver()
    scheduleDelayedScrollbarUpdate()
  })
})

onBeforeUnmount(() => {
  stopTransitionTextTimer()
  scrollbarResizeObserver?.disconnect()
  window.removeEventListener('resize', scheduleDelayedScrollbarUpdate)

  if (scrollbarUpdateFrame !== null) {
    window.cancelAnimationFrame(scrollbarUpdateFrame)
  }

  if (delayedScrollbarUpdateTimer !== null) {
    window.clearTimeout(delayedScrollbarUpdateTimer)
  }
})
</script>

<style scoped>
.interview-screen {
  position: relative;
  height: 100vh;
  height: 100dvh;
  width: 100%;
  overflow: hidden;
  background:
    radial-gradient(circle at 50% -30%, rgba(0, 78, 234, 0.26), transparent 36rem),
    linear-gradient(180deg, #ffffff 0%, #e3ecff 100%);
  color: #000000;
}

.interview-scrollbar {
  height: 100vh;
  height: 100dvh;
}

.interview-scrollbar :deep(.ps__rail-y) {
  display: none !important;
}

.interview-scrollbar--enabled :deep(.ps__rail-y) {
  display: block !important;
  width: 10px;
  right: 10px;
  border-radius: 999px;
  background: rgba(0, 70, 212, 0.08);
  opacity: 1;
  transition: opacity 0.18s ease;
}

.interview-scrollbar--enabled :deep(.ps__thumb-y) {
  right: 2px;
  width: 6px;
  border-radius: 999px;
  background: rgba(0, 70, 212, 0.42);
}

.interview-scrollbar--enabled :deep(.ps__rail-y:hover),
.interview-scrollbar--enabled :deep(.ps__rail-y:focus),
.interview-scrollbar--enabled :deep(.ps__rail-y.ps--clicking) {
  width: 10px;
  background: rgba(0, 70, 212, 0.12);
}

.interview-scrollbar--enabled :deep(.ps__rail-y:hover > .ps__thumb-y),
.interview-scrollbar--enabled :deep(.ps__rail-y:focus > .ps__thumb-y),
.interview-scrollbar--enabled :deep(.ps__rail-y.ps--clicking .ps__thumb-y) {
  width: 6px;
  background: rgba(0, 70, 212, 0.58);
}

.interview-logo {
  position: fixed;
  top: 40px;
  left: 2rem;
  z-index: 10;
  height: calc(var(--header-container-height) - 80px);
  display: flex;
  align-items: center;
  gap: 1rem;
  border: 0;
  padding: 0;
  background: transparent;
  color: black;
  font: inherit;
  font-weight: 500;
  cursor: pointer;
}

.interview-logo-image {
  height: calc(100% - 1.6rem);
  aspect-ratio: 1;
  border-radius: 50%;
  display: block;
}

.interview-profile-button {
  position: fixed;
  top: 50px;
  right: 2rem;
  z-index: 10;
  color: black;
  font-weight: 600;
  padding: 0 1rem;
}

.interview-profile-icon {
  width: 1.35rem;
  height: 1.35rem;
  display: block;
  object-fit: contain;
  opacity: 0.72;
  margin-right: 0.45rem;
}

.interview-content {
  min-height: 100vh;
  min-height: 100dvh;
  width: min(80vw, 1120px);
  margin: 0 auto;
  padding: var(--content-top-offset, 0px) 0 var(--content-bottom-offset, 0px);
  box-sizing: border-box;
  display: flex;
  align-items: flex-start;
  transition: padding 0.32s cubic-bezier(0.22, 1, 0.36, 1);
}

.interview-content-inner {
  width: 100%;
  height: fit-content;
  align-self: flex-start;
  margin: 0 auto;
  display: flex;
  justify-content: center;
  align-items: flex-start;
  transition:
    transform 0.32s cubic-bezier(0.22, 1, 0.36, 1),
    opacity 0.2s ease;
}

.interview-transition-overlay {
  position: fixed;
  inset: 0;
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  backdrop-filter: blur(8px) saturate(0) brightness(35%);
  -webkit-backdrop-filter: blur(8px) saturate(0) brightness(35%);
}

.interview-transition-loader {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1.35rem;
  padding: 2rem;
  color: white;
  text-align: center;
}

.interview-transition-text {
  min-height: 2rem;
  color: white;
  font-size: 1.25rem;
  font-weight: 400;
  line-height: 1.25;
  letter-spacing: -0.02em;
  text-shadow: 0 10px 32px rgba(0, 0, 0, 0.22);
}

.transition-overlay-enter-active,
.transition-overlay-leave-active {
  transition:
    opacity 0.28s ease,
    backdrop-filter 0.28s ease;
}

.transition-overlay-enter-from,
.transition-overlay-leave-to {
  opacity: 0;
  background: transparent;
  backdrop-filter: blur(0) saturate(1) brightness(100%);
  -webkit-backdrop-filter: blur(0) saturate(1) brightness(100%);
}

.status-card,
.error-card {
  width: min(560px, 100%);
  border-radius: 2rem;
  padding: 1.7rem;
  background: rgba(255, 255, 255, 0.96);
  box-shadow: 0 18px 54px rgba(23, 42, 97, 0.13);
  color: black;
  text-align: center;
}

.status-card,
.error-card {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.status-card p,
.error-card p {
  max-width: 430px;
  margin: 0.8rem auto 0;
  color: rgba(0, 0, 0, 0.58);
  font-size: 0.98rem;
  line-height: 1.45;
}

.error-card h1 {
  margin: 0;
  font-size: 2.15rem;
  font-weight: 600;
  line-height: 1.08;
}

.error-card .v-btn {
  margin-top: 1.2rem;
}

.error-icon {
  margin-bottom: 1rem;
  background: rgba(255, 73, 59, 0.12);
  color: #ff493b;
}

@media (max-width: 720px) {
  .interview-content {
    width: min(100% - 2rem, 1120px);
  }

  .interview-logo {
    left: 1rem;
  }
}
</style>
