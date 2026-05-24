<template>
  <div class="profile-screen">
    <Teleport to="body">
      <ProfileSidebar @logout="showLogoutConfirm = true" />
    </Teleport>

    <img
      :src="blueBallImage"
      class="profile-blue-ball"
      :style="blueBallStyle"
      alt=""
    >

    <PerfectScrollbar
      ref="scrollbarRef"
      class="profile-scrollbar"
      :options="scrollbarOptions"
    >
      <main
        class="profile-main"
        :class="`profile-main--${currentSection}`"
      >
        <div
          ref="activeContentRef"
          class="section-stage"
          :class="{ 'section-stage--locked': sectionStageLocked }"
          :style="sectionStageStyle"
        >
          <RevealOnActive
            data-section-panel="profile"
            tag="section"
            class="profile-column section-panel"
            :active="currentSection === 'profile'"
            :enter-y="getSectionEnterY('profile')"
            :exit-y="getSectionExitY('profile')"
            :delay="getSectionEnterDelay('profile')"
            :duration="0.82"
            :exit-duration="0.48"
            :blur="8"
            collapse-when-hidden
          >
            <v-card
              v-if="viewState === 'loading' || viewState === 'idle'"
              class="status-card"
              elevation="0"
            >
              <v-progress-circular
                color="#004EEA"
                indeterminate
                size="2.6rem"
                width="4"
              />
              <p>Загружаем профиль...</p>
            </v-card>

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
                @click="loadProfile"
              >
                Попробовать снова
              </v-btn>
            </v-card>

            <template v-else>
              <v-card class="user-card" elevation="0">
                <v-avatar class="user-avatar" size="4.2rem">
                  <img :src="profileIcon" class="user-avatar-icon" alt="">
                </v-avatar>

                <div class="user-copy">
                  <span>Профиль</span>
                  <SplitText
                    class="user-name"
                    tag="h1"
                    :text="displayName"
                    :active="currentSection === 'profile'"
                    split-type="words"
                    :delay="24"
                    :duration="0.7"
                    :exit-duration="0.28"
                    :blur="4"
                  />
                </div>
              </v-card>

              <v-card
                v-if="viewState === 'start'"
                class="action-card"
                elevation="0"
              >
                <v-avatar class="action-icon" size="3.3rem">
                  <v-icon icon="mdi-auto-fix" />
                </v-avatar>

                <h2>Начните интервью</h2>
                <p>
                  Ответьте на несколько вопросов, чтобы FitFlow понял ваш профиль и позже смог подбирать заказы точнее.
                </p>

                <v-btn
                  class="action-button"
                  color="black"
                  rounded="xl"
                  size="large"
                  variant="flat"
                  @click="openInterview"
                >
                  Начать интервью
                </v-btn>
              </v-card>

              <v-card
                v-else-if="viewState === 'continue'"
                class="action-card"
                elevation="0"
              >
                <v-avatar class="action-icon" size="3.3rem">
                  <v-icon icon="mdi-play" />
                </v-avatar>

                <h2>Продолжить интервью</h2>
                <p>
                  Мы сохранили ваши ответы. Можно вернуться к интервью и продолжить с текущего вопроса.
                </p>

                <v-btn
                  class="action-button"
                  color="black"
                  rounded="xl"
                  size="large"
                  variant="flat"
                  @click="openInterview"
                >
                  Продолжить интервью
                </v-btn>
              </v-card>

              <v-card
                v-else-if="viewState === 'checking-interview'"
                class="status-card"
                elevation="0"
              >
                <v-progress-circular
                  color="#004EEA"
                  indeterminate
                  size="2.6rem"
                  width="4"
                />
                <p>Проверяем интервью...</p>
              </v-card>

              <InterviewConclusionCard
                v-else-if="viewState === 'conclusion' && userProfile"
                :profile="userProfile"
                @view-change="handleConclusionViewChange"
              />
            </template>
          </RevealOnActive>

          <RevealOnActive
            data-section-panel="settings"
            tag="section"
            class="settings-screen placeholder-column section-panel"
            :active="currentSection === 'settings'"
            :enter-y="getSectionEnterY('settings')"
            :exit-y="getSectionExitY('settings')"
            :delay="getSectionEnterDelay('settings')"
            :duration="0.82"
            :exit-duration="0.48"
            :blur="8"
            collapse-when-hidden
          >
            <ProfileSettingsSection :active="currentSection === 'settings'" />
          </RevealOnActive>

          <RevealOnActive
            data-section-panel="feed"
            tag="section"
            class="feed-screen placeholder-column section-panel"
            :active="currentSection === 'feed'"
            :enter-y="getSectionEnterY('feed')"
            :exit-y="getSectionExitY('feed')"
            :delay="getSectionEnterDelay('feed')"
            :duration="0.82"
            :exit-duration="0.48"
            :blur="8"
            collapse-when-hidden
          >
            <ProfileFeedSection
              :active="currentSection === 'feed'"
              :jobs="feedJobs"
              :feed-state="recommendationFeedState"
              :loading="recommendationsLoading"
              :error="recommendationError"
              :stream-state="recommendationStreamState"
              @retry="loadRecommendedJobs"
              @content-change="handleFeedContentChange"
            />
          </RevealOnActive>
        </div>
      </main>
    </PerfectScrollbar>

    <TopConfirmDialog
      v-model="showLogoutConfirm"
      title="Выйти из аккаунта?"
      text="Вы действительно хотите выйти?"
      cancel-text="Отмена"
      confirm-text="Выйти"
      @confirm="handleLogout"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import type { ComponentPublicInstance } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { PerfectScrollbar } from 'vue3-perfect-scrollbar'
import 'vue3-perfect-scrollbar/style.css'
import InterviewConclusionCard from './InterviewConclusionCard.vue'
import ProfileFeedSection from './ProfileFeedSection.vue'
import ProfileSettingsSection from './ProfileSettingsSection.vue'
import ProfileSidebar from './ProfileSidebar.vue'
import RevealOnActive from './RevealOnActive.vue'
import SplitText from './SplitText.vue'
import TopConfirmDialog from './TopConfirmDialog.vue'
import blueBallImage from '@/assets/BlueBall.png'
import profileIcon from '@/assets/icons/google-account-circle.svg'
import { logout } from '@/services/auth'
import {
  ApiError,
  getCurrentUser,
  getMyInterview,
} from '@/services/interviewApi'
import type {
  CurrentUserDto,
  UserProfileDto,
} from '@/services/interviewApi'
import {
  emptyRecommendationCursor,
  getJobPosting,
  loadRecommendationCache,
  openRecommendationStream,
  saveRecommendationCache,
  setRecommendationCacheUserId,
} from '@/services/recommendationsApi'
import type {
  JobPostingDto,
  RecommendationCache,
  RecommendationSseCommand,
  RecommendationStreamState,
} from '@/services/recommendationsApi'

type ViewState = 'idle' | 'loading' | 'start' | 'checking-interview' | 'continue' | 'conclusion' | 'error'
type ProfileSection = 'profile' | 'settings' | 'feed'
type RecommendationFeedState = 'preparing' | 'empty' | 'loaded' | 'error'

const router = useRouter()
const route = useRoute()

const viewState = ref<ViewState>('idle')
const currentUser = ref<CurrentUserDto | null>(null)
const userProfile = ref<UserProfileDto | null>(null)
const errorMessage = ref('')
const showLogoutConfirm = ref(false)
const validSectionHashes = new Set(['#profile', '#settings', '#feed'])
const scrollbarRef = ref<ComponentPublicInstance | null>(null)
const activeContentRef = ref<HTMLElement | null>(null)
const viewportHeight = ref(1)
const viewportWidth = ref(1)
const sectionDirection = ref(1)
const sectionStageHeightPx = ref(0)
const sectionStageLocked = ref(false)
const hasSectionSwitchHappened = ref(false)
const isSectionSwitching = ref(false)
const recommendationCache = ref<RecommendationCache>(loadRecommendationCache())
const recommendationJobsById = ref<Record<string, JobPostingDto>>({})
const recommendationsLoading = ref(false)
const recommendationError = ref('')
const recommendationStreamState = ref<RecommendationStreamState>('closed')

const sectionOrder: Record<ProfileSection, number> = {
  profile: 0,
  settings: 1,
  feed: 2,
}

const scrollbarOptions = {
  suppressScrollX: true,
  wheelPropagation: false,
}

let scrollbarUpdateFrame: number | null = null
let delayedScrollbarUpdateTimer: ReturnType<typeof window.setTimeout> | null = null
let targetHeightUpdateTimer: ReturnType<typeof window.setTimeout> | null = null
let sectionSwitchEndTimer: ReturnType<typeof window.setTimeout> | null = null
let finalScrollbarUpdateTimer: ReturnType<typeof window.setTimeout> | null = null
let scrollbarResizeObserver: ResizeObserver | null = null
let recommendationStream: ReturnType<typeof openRecommendationStream> | null = null

const currentSection = computed<ProfileSection>(() => {
  const section = route.hash.replace('#', '')

  if (section === 'settings') {
    return 'settings'
  }

  if (section === 'feed') {
    return 'feed'
  }

  return 'profile'
})

const currentSectionIndex = computed(() => sectionOrder[currentSection.value])
const sectionMotionDistance = computed(() => Math.max(viewportHeight.value * 1.35, 1180))

const sectionStageStyle = computed(() => ({
  '--section-stage-height': `${sectionStageHeightPx.value}px`,
}))

const blueBallStyle = computed(() => {
  const circleDiameter = viewportHeight.value * 1.75
  const viewportDiagonal = Math.hypot(viewportWidth.value, viewportHeight.value)
  const fullScreenScale = Math.max(1.85, (viewportDiagonal / circleDiameter) * 1.65)

  if (currentSectionIndex.value === 1) {
    return {
      opacity: 1,
      transform: `translate3d(0, ${viewportHeight.value * 0.4125}px, 0) scale(${fullScreenScale})`,
    }
  }

  if (currentSectionIndex.value === 2) {
    return {
      opacity: 0,
      transform: `translate3d(0, ${viewportHeight.value * 0.4125}px, 0) scale(${fullScreenScale})`,
    }
  }

  return {
    opacity: 1,
    transform: 'translate3d(0, 0, 0) scale(1)',
  }
})

const displayName = computed(() => {
  const firstName = currentUser.value?.firstName?.trim() ?? ''
  const lastName = currentUser.value?.lastName?.trim() ?? ''
  const fullName = [firstName, lastName].filter(Boolean).join(' ')

  return fullName || 'Пользователь FitFlow'
})

const recommendationFeedState = computed<RecommendationFeedState>(() => {
  if (recommendationError.value) {
    return 'error'
  }

  if (recommendationCache.value === null) {
    return 'preparing'
  }

  return recommendationCache.value.ids.length === 0 ? 'empty' : 'loaded'
})

const feedJobs = computed(() => {
  if (recommendationCache.value === null) {
    return []
  }

  return recommendationCache.value.ids
    .map((id) => recommendationJobsById.value[id])
    .filter((job): job is JobPostingDto => Boolean(job))
    .sort((firstJob, secondJob) => (
      new Date(secondJob.postedAt).getTime() - new Date(firstJob.postedAt).getTime()
    ))
})

function getUniqueIds(ids: string[]) {
  return Array.from(new Set(ids.filter(Boolean)))
}

function mergeRecommendationCache(command: RecommendationSseCommand, latestRecommendationAt: string | null) {
  const current = recommendationCache.value
  const latest = latestRecommendationAt
    ?? current?.latestRecommendationAt
    ?? emptyRecommendationCursor

  if (command.command_name === 'send_batch') {
    recommendationCache.value = {
      ids: getUniqueIds([...command.data.ids, ...(current?.ids ?? [])]),
      latestRecommendationAt: latest,
    }
  } else {
    recommendationCache.value = {
      ids: getUniqueIds([command.data.id, ...(current?.ids ?? [])]),
      latestRecommendationAt: latest,
    }
  }

  saveRecommendationCache(recommendationCache.value)
}

async function loadRecommendedJobs() {
  if (recommendationCache.value === null || recommendationCache.value.ids.length === 0) {
    return
  }

  recommendationsLoading.value = true
  recommendationError.value = ''

  try {
    const missingIds = recommendationCache.value.ids.filter((id) => !recommendationJobsById.value[id])
    const staleIds = new Set<string>()
    const jobs = await Promise.all(missingIds.map(async (id) => {
      try {
        return await getJobPosting(id)
      } catch (error) {
        if (error instanceof ApiError && error.status === 404) {
          staleIds.add(id)
          return null
        }

        throw error
      }
    }))
    const nextJobs = { ...recommendationJobsById.value }

    for (const job of jobs) {
      if (job) {
        nextJobs[job.id] = job
      }
    }

    for (const id of staleIds) {
      delete nextJobs[id]
    }

    if (staleIds.size > 0 && recommendationCache.value !== null) {
      recommendationCache.value = {
        ...recommendationCache.value,
        ids: recommendationCache.value.ids.filter((id) => !staleIds.has(id)),
      }
      saveRecommendationCache(recommendationCache.value)
    }

    recommendationJobsById.value = nextJobs
  } catch (error) {
    recommendationError.value = error instanceof Error
      ? error.message
      : 'Не удалось загрузить рекомендации.'
  } finally {
    recommendationsLoading.value = false
  }
}

async function preserveFeedScrollForPrependedJobs(operation: () => Promise<void> | void) {
  const scrollbarElement = getScrollbarElement()
  const shouldPreserve = currentSection.value === 'feed'
    && feedJobs.value.length > 0
    && Boolean(scrollbarElement)
  const previousScrollHeight = scrollbarElement?.scrollHeight ?? 0
  const previousScrollTop = scrollbarElement?.scrollTop ?? 0

  await operation()
  await nextTick()

  if (shouldPreserve && scrollbarElement) {
    const heightDiff = Math.max(scrollbarElement.scrollHeight - previousScrollHeight, 0)
    scrollbarElement.scrollTop = previousScrollTop + heightDiff
  }

  scheduleDelayedScrollbarUpdate(false, true)
}

function handleRecommendationCommand(
  command: RecommendationSseCommand,
  latestRecommendationAt: string | null,
) {
  void preserveFeedScrollForPrependedJobs(async () => {
    mergeRecommendationCache(command, latestRecommendationAt)
    await nextTick()
    await loadRecommendedJobs()
  })
}

function startRecommendationStream() {
  recommendationStream?.close()
  recommendationStream = openRecommendationStream(
    handleRecommendationCommand,
    (state) => {
      recommendationStreamState.value = state
    },
  )
}

function getSectionEnterY(section: ProfileSection) {
  if (!hasSectionSwitchHappened.value) {
    return 34
  }

  if (section !== currentSection.value) {
    return sectionMotionDistance.value
  }

  return sectionDirection.value >= 0
    ? sectionMotionDistance.value
    : -sectionMotionDistance.value
}

function getSectionExitY(section: ProfileSection) {
  if (!hasSectionSwitchHappened.value) {
    return 34
  }

  if (section === currentSection.value) {
    return 0
  }

  return sectionDirection.value >= 0
    ? -sectionMotionDistance.value
    : sectionMotionDistance.value
}

function getSectionEnterDelay(section: ProfileSection) {
  if (section !== currentSection.value) {
    return 0
  }

  if (!hasSectionSwitchHappened.value) {
    return 0
  }

  return 0.62
}

function openInterview() {
  void router.push({ name: 'interview' })
}

function handleLogout() {
  showLogoutConfirm.value = false
  void logout()
}

function updateViewport() {
  viewportHeight.value = window.innerHeight || 1
  viewportWidth.value = window.innerWidth || 1
}

function getScrollbarElement() {
  const target = scrollbarRef.value

  if (!target) {
    return null
  }

  return target.$el instanceof HTMLElement ? target.$el : null
}

function getSectionPanelElement(section: ProfileSection) {
  return activeContentRef.value?.querySelector(
    `[data-section-panel="${section}"]`,
  ) as HTMLElement | null
}

function getActiveSectionPanelElement() {
  return getSectionPanelElement(currentSection.value)
}

function lockSectionStageHeight(height: number) {
  if (height <= 0) {
    return
  }

  sectionStageHeightPx.value = Math.ceil(height)
  sectionStageLocked.value = true
}

function updateSectionStageHeight(force = false) {
  if (isSectionSwitching.value && !force) {
    return
  }

  const activePanel = getActiveSectionPanelElement()

  if (!activePanel) {
    return
  }

  lockSectionStageHeight(activePanel.scrollHeight)
}

function updateTargetSectionHeightLater(section: ProfileSection) {
  if (targetHeightUpdateTimer !== null) {
    window.clearTimeout(targetHeightUpdateTimer)
  }

  if (sectionSwitchEndTimer !== null) {
    window.clearTimeout(sectionSwitchEndTimer)
  }

  targetHeightUpdateTimer = window.setTimeout(() => {
    const targetPanel = getSectionPanelElement(section)

    if (targetPanel) {
      lockSectionStageHeight(targetPanel.scrollHeight)
    }

    scheduleScrollbarUpdate(true, true)
  }, 560)

  sectionSwitchEndTimer = window.setTimeout(() => {
    isSectionSwitching.value = false
    scheduleScrollbarUpdate(false, true)
  }, 1380)
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

function clampScrollbarPosition() {
  const scrollbarElement = getScrollbarElement()

  if (!scrollbarElement) {
    return
  }

  const maxScrollTop = Math.max(scrollbarElement.scrollHeight - scrollbarElement.clientHeight, 0)

  if (scrollbarElement.scrollTop > maxScrollTop) {
    scrollbarElement.scrollTop = maxScrollTop
  }
}

function resetScrollbarPosition() {
  const scrollbarElement = getScrollbarElement()

  if (scrollbarElement) {
    scrollbarElement.scrollTop = 0
  }
}

function scheduleScrollbarUpdate(shouldResetScroll = false, forceHeightUpdate = false) {
  if (scrollbarUpdateFrame !== null) {
    window.cancelAnimationFrame(scrollbarUpdateFrame)
  }

  scrollbarUpdateFrame = window.requestAnimationFrame(() => {
    scrollbarUpdateFrame = null

    updateSectionStageHeight(forceHeightUpdate)

    if (shouldResetScroll) {
      resetScrollbarPosition()
    } else {
      clampScrollbarPosition()
    }

    updatePerfectScrollbarInstance()
  })
}

function scheduleDelayedScrollbarUpdate(
  shouldResetScroll = false,
  forceHeightUpdate = false,
) {
  if (delayedScrollbarUpdateTimer !== null) {
    window.clearTimeout(delayedScrollbarUpdateTimer)
  }

  if (finalScrollbarUpdateTimer !== null) {
    window.clearTimeout(finalScrollbarUpdateTimer)
  }

  scheduleScrollbarUpdate(shouldResetScroll, forceHeightUpdate)

  delayedScrollbarUpdateTimer = window.setTimeout(() => {
    scheduleScrollbarUpdate(shouldResetScroll, forceHeightUpdate)
  }, 320)

  finalScrollbarUpdateTimer = window.setTimeout(() => {
    scheduleScrollbarUpdate(shouldResetScroll, true)
  }, 1300)
}

function connectScrollbarResizeObserver() {
  scrollbarResizeObserver?.disconnect()

  if (typeof ResizeObserver === 'undefined') {
    return
  }

  const scrollbarElement = getScrollbarElement()
  const activePanelElement = getActiveSectionPanelElement()

  if (!scrollbarElement || !activePanelElement) {
    return
  }

  scrollbarResizeObserver = new ResizeObserver(() => scheduleScrollbarUpdate())
  scrollbarResizeObserver.observe(scrollbarElement)
  scrollbarResizeObserver.observe(activePanelElement)
}

function handleConclusionViewChange(isDetailed: boolean) {
  void nextTick(() => {
    scheduleDelayedScrollbarUpdate(!isDetailed, true)
  })
}

function handleFeedContentChange() {
  void nextTick(() => {
    scheduleDelayedScrollbarUpdate(false, true)
  })
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

async function checkExistingInterview() {
  viewState.value = 'checking-interview'

  try {
    const interview = await getMyInterview()

    if (interview.conclusion) {
      userProfile.value = interview.conclusion
      viewState.value = 'conclusion'
      return
    }

    viewState.value = 'continue'
  } catch (error) {
    setError(error)
  }
}

async function loadProfile() {
  viewState.value = 'loading'
  userProfile.value = null
  errorMessage.value = ''

  try {
    currentUser.value = await getCurrentUser()
    setRecommendationCacheUserId(currentUser.value.id)
    recommendationCache.value = loadRecommendationCache()

    if (currentUser.value.hasInterview) {
      await checkExistingInterview()
      return
    }

    viewState.value = 'start'
  } catch (error) {
    setError(error)
  }
}

watch(
  () => route.hash,
  (hash) => {
    if (!validSectionHashes.has(hash)) {
      void router.replace({
        name: 'profile',
        hash: '#profile',
      })
    }
  },
  { immediate: true },
)

watch(
  currentSection,
  (section, previousSection) => {
    const currentIndex = sectionOrder[section]
    const previousIndex = previousSection ? sectionOrder[previousSection] : currentIndex
    const sectionChanged = currentIndex !== previousIndex

    if (sectionChanged) {
      const previousPanel = previousSection ? getSectionPanelElement(previousSection) : null
      const previousHeight = previousPanel?.scrollHeight || sectionStageHeightPx.value

      isSectionSwitching.value = true
      lockSectionStageHeight(previousHeight)

      sectionDirection.value = currentIndex > previousIndex ? 1 : -1
      hasSectionSwitchHappened.value = true

      updateTargetSectionHeightLater(section)
      scheduleScrollbarUpdate(true, false)
    } else {
      scheduleDelayedScrollbarUpdate(true, true)
    }
  },
  { immediate: true },
)

watch(
  [currentSection, viewState, userProfile, feedJobs],
  () => {
    void nextTick(() => {
      connectScrollbarResizeObserver()
      scheduleDelayedScrollbarUpdate(false, !isSectionSwitching.value)
    })
  },
  { flush: 'post' },
)

onMounted(() => {
  updateViewport()
  window.addEventListener('resize', updateViewport, { passive: true })

  void loadProfile().then(() => {
    void loadRecommendedJobs()
    startRecommendationStream()
  })

  void nextTick(() => {
    connectScrollbarResizeObserver()
    scheduleDelayedScrollbarUpdate(false, true)
  })
})

onBeforeUnmount(() => {
  recommendationStream?.close()
  scrollbarResizeObserver?.disconnect()
  window.removeEventListener('resize', updateViewport)

  if (scrollbarUpdateFrame !== null) {
    window.cancelAnimationFrame(scrollbarUpdateFrame)
  }

  if (delayedScrollbarUpdateTimer !== null) {
    window.clearTimeout(delayedScrollbarUpdateTimer)
  }

  if (targetHeightUpdateTimer !== null) {
    window.clearTimeout(targetHeightUpdateTimer)
  }

  if (sectionSwitchEndTimer !== null) {
    window.clearTimeout(sectionSwitchEndTimer)
  }

  if (finalScrollbarUpdateTimer !== null) {
    window.clearTimeout(finalScrollbarUpdateTimer)
  }
})
</script>

<style scoped>
.profile-screen {
  position: fixed;
  inset: 0;
  z-index: 0;
  height: 100vh;
  height: 100dvh;
  width: 100vw;
  overflow: hidden;
  background: linear-gradient(to top, #e3ecff, white);
  color: #000000;
}

.profile-blue-ball {
  position: fixed;
  top: -78.75vh;
  left: 50%;
  z-index: 0;
  width: 175vh;
  height: 175vh;
  margin-left: -87.5vh;
  aspect-ratio: 1;
  border-radius: 50%;
  filter: blur(8px);
  pointer-events: none;
  transform-origin: center center;
  transition:
    transform 900ms cubic-bezier(0.76, 0, 0.24, 1),
    opacity 520ms ease;
  will-change: transform, opacity;
}

.profile-scrollbar {
  position: fixed;
  inset: 0;
  z-index: 1;
  width: 100vw;
  height: 100vh;
  height: 100dvh;
  overflow-x: hidden;
}

.profile-scrollbar:not(.ps--active-y) :deep(.ps__rail-y) {
  display: none !important;
}

.profile-scrollbar :deep(.ps__rail-y) {
  position: fixed !important;
  top: 0 !important;
  right: 10px !important;
  bottom: 0 !important;
  width: 10px;
  height: 100vh !important;
  height: 100dvh !important;
  border-radius: 999px;
  background: transparent;
  opacity: 0;
  transition: opacity 0.18s ease;
}

.profile-scrollbar :deep(.ps__rail-y[style]) {
  top: 0 !important;
  height: 100dvh !important;
}

.profile-scrollbar.ps--active-y :deep(.ps__rail-y),
.profile-scrollbar :deep(.ps__rail-y:hover),
.profile-scrollbar :deep(.ps__rail-y:focus),
.profile-scrollbar :deep(.ps__rail-y.ps--clicking) {
  display: block;
  opacity: 1;
  background: rgba(0, 70, 212, 0.08);
}

.profile-scrollbar :deep(.ps__thumb-y) {
  right: 2px;
  width: 6px;
  border-radius: 999px;
  background: rgba(0, 70, 212, 0.42);
}

.profile-scrollbar :deep(.ps__rail-y:hover),
.profile-scrollbar :deep(.ps__rail-y:focus),
.profile-scrollbar :deep(.ps__rail-y.ps--clicking) {
  width: 10px;
  background: rgba(0, 70, 212, 0.12);
}

.profile-scrollbar :deep(.ps__rail-y:hover > .ps__thumb-y),
.profile-scrollbar :deep(.ps__rail-y:focus > .ps__thumb-y),
.profile-scrollbar :deep(.ps__rail-y.ps--clicking .ps__thumb-y) {
  width: 6px;
  background: rgba(0, 70, 212, 0.58);
}

.profile-main {
  min-height: 100vh;
  min-height: 100dvh;
  width: 100%;
  max-width: none;
  box-sizing: border-box;
  padding: 3rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.section-stage {
  position: relative;
  width: min(860px, 100%);
  overflow: visible;
}

.section-stage--locked {
  height: var(--section-stage-height);
  min-height: var(--section-stage-height);
}

.section-stage--locked .section-panel {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: auto;
  width: 100%;
  height: auto;
  min-height: 0;
}

.section-panel {
  width: 100%;
  position: relative;
  z-index: 2;
}

.section-panel[aria-hidden='true'] {
  z-index: 1;
  pointer-events: none;
}

.section-stage:not(.section-stage--locked) .section-panel[aria-hidden='true'] {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
}

.section-panel.reveal-on-active--visible:not([aria-hidden='true']) {
  z-index: 3;
}

.profile-column {
  width: min(760px, 100%);
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.profile-column > * {
  width: min(760px, 100%);
  flex-shrink: 0;
}

.placeholder-column {
  width: min(860px, 100%);
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.placeholder-column > * {
  flex-shrink: 0;
}

.user-card,
.action-card,
.status-card,
.error-card {
  border-radius: 2rem;
  background: rgba(255, 255, 255, 0.96);
  box-shadow: 0 22px 62px rgba(23, 42, 97, 0.14);
  color: black;
}

.user-card {
  display: flex;
  align-items: center;
  gap: 1.1rem;
  padding: 1.25rem;
}

.user-avatar {
  flex: 0 0 auto;
  background: linear-gradient(135deg, #f1f3f8 0%, #d9dee9 100%);
}

.user-avatar-icon {
  width: 68%;
  height: 68%;
  display: block;
  object-fit: contain;
  opacity: 0.38;
}

.user-copy {
  min-width: 0;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.user-copy span {
  display: block;
  color: rgba(0, 0, 0, 0.46);
  font-size: 0.78rem;
  font-weight: 600;
  line-height: 1;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.user-name {
  display: block;
  width: 100%;
  margin: 0.35rem 0 0;
  color: black;
  font-size: 1.65rem;
  font-weight: 600;
  line-height: 1.08;
  letter-spacing: -0.03em;
  overflow-wrap: anywhere;
}

.user-name :deep(*) {
  line-height: inherit;
}

.action-card,
.status-card,
.error-card {
  padding: 1.55rem;
  text-align: center;
}

.action-icon {
  margin-bottom: 1rem;
  background: #004eea;
  color: white;
}

.action-card h2,
.error-card h1 {
  display: block;
  margin: 0;
  color: black;
  font-size: 1.7rem;
  font-weight: 600;
  line-height: 1.08;
  letter-spacing: -0.03em;
}

.action-card p,
.error-card p,
.status-card p {
  max-width: 520px;
  margin: 0.75rem auto 0;
  color: rgba(0, 0, 0, 0.58);
  font-size: 0.98rem;
  font-weight: 400;
  line-height: 1.45;
}

.action-button {
  margin-top: 1.25rem;
  padding: 0 2rem;
  font-weight: 700;
}

.status-card,
.error-card {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.error-card .v-btn {
  margin-top: 1.2rem;
}

.error-icon {
  margin-bottom: 1rem;
  background: rgba(255, 73, 59, 0.12);
  color: #ff493b;
}

@media (max-width: 900px) {
  .profile-main {
    padding: 7rem 1rem 2rem;
  }
}
</style>
