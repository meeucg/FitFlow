<template>
  <v-card class="feed-hero-card" elevation="0">
    <div class="feed-hero-copy">
      <span>Лента</span>
      <SplitText
        class="placeholder-title"
        tag="h1"
        text="Новые заказы"
        :active="active"
        split-type="words"
        :delay="28"
        :duration="0.78"
        :exit-duration="0.28"
        :blur="4"
      />
      <p>
        Заказы приходят через SSE и появляются сверху. При обновлении ленты текущая позиция скролла сохраняется.
      </p>
    </div>

    <div class="feed-status-pill" :class="`feed-status-pill--${streamState}`">
      <span class="feed-status-dot" />
      {{ streamStateLabel }}
    </div>
  </v-card>

  <div
    v-if="feedState === 'preparing'"
    class="feed-list"
  >
    <v-card class="feed-state-card" elevation="0">
      <v-progress-circular
        color="#004EEA"
        indeterminate
        size="2.6rem"
        width="4"
      />
      <h2>Готовим рекомендации...</h2>
      <p>Как только backend закончит подбор, заказы появятся здесь автоматически.</p>
    </v-card>
  </div>

  <div
    v-else-if="feedState === 'empty'"
    class="feed-list"
  >
    <v-card class="feed-state-card" elevation="0">
      <v-avatar class="feed-state-icon" size="3rem">
        <v-icon icon="mdi-briefcase-search-outline" />
      </v-avatar>
      <h2>Пока нет подходящих заказов</h2>
      <p>Оставьте эту страницу открытой — новые рекомендации появятся сверху без перезагрузки.</p>
    </v-card>
  </div>

  <div
    v-else-if="feedState === 'error'"
    class="feed-list"
  >
    <v-card class="feed-state-card" elevation="0">
      <v-avatar class="error-icon" size="3rem">
        <v-icon icon="mdi-alert-circle-outline" />
      </v-avatar>
      <h2>Не удалось загрузить рекомендации</h2>
      <p>{{ error }}</p>
      <v-btn
        color="black"
        rounded="xl"
        variant="flat"
        @click="$emit('retry')"
      >
        Повторить
      </v-btn>
    </v-card>
  </div>

  <div v-else class="feed-list-wrap">
    <v-card
      v-if="loading && jobs.length === 0"
      class="feed-state-card"
      elevation="0"
    >
      <v-progress-circular
        color="#004EEA"
        indeterminate
        size="2.2rem"
        width="4"
      />
      <h2>Загружаем заказы...</h2>
    </v-card>

    <TransitionGroup
      name="feed-job"
      tag="div"
      class="feed-list"
    >
      <ProfileJobCard
        v-for="job in jobs"
        :key="job.id"
        :job="job"
        @content-change="$emit('contentChange')"
      />
    </TransitionGroup>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import SplitText from './SplitText.vue'
import ProfileJobCard from './ProfileJobCard.vue'
import type {
  JobPostingDto,
  RecommendationStreamState,
} from '@/services/recommendationsApi'

const props = defineProps<{
  active: boolean
  jobs: JobPostingDto[]
  feedState: 'preparing' | 'empty' | 'loaded' | 'error'
  loading: boolean
  error: string
  streamState: RecommendationStreamState
}>()

defineEmits<{
  retry: []
  contentChange: []
}>()

const streamStateLabel = computed(() => {
  switch (props.streamState) {
    case 'connected':
      return 'Подключено'
    case 'connecting':
      return 'Подключаемся'
    case 'reconnecting':
      return 'Переподключаемся'
    case 'error':
      return 'Ошибка SSE'
    case 'closed':
    default:
      return 'Поток закрыт'
  }
})
</script>

<style scoped>
.feed-hero-card,
.feed-state-card {
  border-radius: 2rem;
  background: rgba(255, 255, 255, 0.96);
  box-shadow: 0 22px 62px rgba(23, 42, 97, 0.14);
  color: black;
}

.feed-hero-card {
  display: flex;
  align-items: stretch;
  justify-content: space-between;
  gap: 1.25rem;
  padding: 1.55rem;
}

.feed-hero-copy {
  min-width: 0;
}

.feed-hero-copy span {
  display: block;
  color: rgba(0, 0, 0, 0.46);
  font-size: 0.78rem;
  font-weight: 600;
  line-height: 1;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.placeholder-title {
  display: block;
  width: 100%;
  margin: 0.35rem 0 0;
  color: black;
  font-size: 2.1rem;
  font-weight: 600;
  line-height: 1.02;
  letter-spacing: -0.04em;
}

.placeholder-title :deep(*) {
  line-height: inherit;
}

.feed-hero-card p {
  max-width: 560px;
  margin: 0.65rem 0 0;
  color: rgba(0, 0, 0, 0.58);
  font-size: 0.98rem;
  line-height: 1.45;
}

.feed-status-pill {
  align-self: flex-start;
  flex: 0 0 auto;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 0.95rem;
  border-radius: 999px;
  background: rgba(0, 78, 234, 0.08);
  color: #004eea;
  font-size: 0.86rem;
  font-weight: 600;
}

.feed-status-dot {
  width: 0.48rem;
  height: 0.48rem;
  border-radius: 999px;
  background: currentColor;
}

.feed-status-pill--error,
.feed-status-pill--closed {
  background: rgba(255, 73, 59, 0.1);
  color: #ff493b;
}

.feed-status-pill--connecting,
.feed-status-pill--reconnecting {
  background: rgba(0, 0, 0, 0.06);
  color: rgba(0, 0, 0, 0.58);
}

.feed-list-wrap,
.feed-list {
  display: grid;
  gap: 1rem;
}

.feed-state-card {
  min-height: 11rem;
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.85rem;
  text-align: center;
}

.feed-state-card h2 {
  margin: 0;
  font-size: 1.28rem;
  font-weight: 600;
  line-height: 1.15;
}

.feed-state-card p {
  max-width: 520px;
  margin: 0;
  color: rgba(0, 0, 0, 0.58);
  font-size: 0.95rem;
  line-height: 1.45;
}

.feed-state-card .v-btn {
  margin-top: 0.25rem;
}

.feed-state-icon {
  background: rgba(0, 78, 234, 0.1);
  color: #004eea;
}

.error-icon {
  margin-bottom: 0.15rem;
  background: rgba(255, 73, 59, 0.12);
  color: #ff493b;
}

.feed-job-enter-active,
.feed-job-leave-active,
.feed-job-move {
  transition:
    opacity 360ms ease,
    transform 360ms cubic-bezier(0.22, 1, 0.36, 1);
}

.feed-job-enter-from {
  opacity: 0;
  transform: translateY(-20px) scale(0.985);
}

.feed-job-leave-to {
  opacity: 0;
  transform: translateY(14px) scale(0.985);
}

.feed-job-leave-active {
  position: absolute;
  width: 100%;
}

@media (max-width: 900px) {
  .feed-hero-card {
    flex-direction: column;
    align-items: flex-start;
  }

  .feed-status-pill {
    align-self: flex-start;
  }
}
</style>
