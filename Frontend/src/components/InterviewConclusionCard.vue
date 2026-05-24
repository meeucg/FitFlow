<template>
  <v-card class="conclusion-card" elevation="0">
    <div class="conclusion-header">
      <v-avatar class="conclusion-icon" size="3rem">
        <v-icon icon="mdi-check-bold" />
      </v-avatar>

      <div class="conclusion-heading">
        <h1>Интервью завершено</h1>
        <p>Скоро здесь появится подбор заказов по вашему профилю</p>
      </div>
    </div>

    <Transition name="conclusion-view" mode="out-in">
      <div v-if="!isDetailed" key="summary" class="summary-view">
        <div class="cluster-card">
          <span>Направление</span>
          <strong>{{ profileView.cluster || 'Пока нет данных' }}</strong>
        </div>

        <div class="summary-sections">
          <section
            v-for="section in summarySections"
            :key="section.title"
            class="summary-section"
          >
            <h2>{{ section.title }}</h2>

            <div v-if="section.items.length > 0" class="chip-list">
              <v-chip
                v-for="item in section.items"
                :key="item"
                class="profile-chip"
                variant="flat"
                size="small"
              >
                {{ item }}
              </v-chip>
            </div>

            <p v-else class="empty-text">
              Пока нет данных
            </p>
          </section>
        </div>
      </div>

      <div v-else key="details" class="details-view">
        <section class="detail-section">
          <h2 class="detail-section-title">Направление</h2>

          <div class="detail-main-card">
            <p>{{ profileView.cluster || 'Пока нет данных' }}</p>
          </div>
        </section>

        <section class="detail-section">
          <h2 class="detail-section-title">Специализации</h2>

          <div v-if="specializations.length > 0" class="detail-list">
            <article
              v-for="(specialization, index) in specializations"
              :key="specialization.name ?? index"
              class="detail-item"
            >
              <h3>{{ specialization.name }}</h3>

              <div
                v-if="specialization.alternativeNames?.length"
                class="alias-list"
              >
                <v-chip
                  v-for="alias in specialization.alternativeNames"
                  :key="alias"
                  class="alias-chip"
                  variant="flat"
                  size="small"
                >
                  {{ alias }}
                </v-chip>
              </div>
            </article>
          </div>

          <p v-else class="empty-text">
            Пока нет данных
          </p>
        </section>

        <section class="detail-section">
          <h2 class="detail-section-title">Навыки</h2>

          <div v-if="skills.length > 0" class="detail-list">
            <article
              v-for="(skill, index) in skills"
              :key="skill.displayName ?? index"
              class="detail-item"
            >
              <div class="detail-item-header">
                <h3>{{ skill.displayName }}</h3>

                <v-chip
                  class="level-chip"
                  :class="`level-chip--${getNormalizedLevel(skill.dominanceLevel)}`"
                  variant="flat"
                  size="small"
                >
                  {{ getSkillLevelLabel(skill.dominanceLevel) }}
                </v-chip>
              </div>

              <p class="detail-description">
                {{ skill.description || 'Описание пока не указано' }}
              </p>

              <div
                v-if="skill.alternativeNames?.length"
                class="alias-list"
              >
                <v-chip
                  v-for="alias in skill.alternativeNames"
                  :key="alias"
                  class="alias-chip"
                  variant="flat"
                  size="small"
                >
                  {{ alias }}
                </v-chip>
              </div>
            </article>
          </div>

          <p v-else class="empty-text">
            Пока нет данных
          </p>
        </section>

        <section class="detail-section">
          <h2 class="detail-section-title">Инструменты</h2>

          <div v-if="tools.length > 0" class="tool-grid">
            <article
              v-for="(tool, index) in tools"
              :key="tool.toolStandardName ?? index"
              class="tool-item"
            >
              <div class="tool-title-row">
                <h3>{{ tool.toolStandardName }}</h3>

                <v-chip
                  class="level-chip"
                  :class="`level-chip--${getNormalizedLevel(tool.usageFrequency)}`"
                  variant="flat"
                  size="small"
                >
                  {{ getToolLevelLabel(tool.usageFrequency) }}
                </v-chip>
              </div>

              <div
                v-if="tool.toolAltNames?.length"
                class="alias-list"
              >
                <v-chip
                  v-for="alias in tool.toolAltNames"
                  :key="alias"
                  class="alias-chip"
                  variant="flat"
                  size="small"
                >
                  {{ alias }}
                </v-chip>
              </div>

              <p v-else class="detail-description">
                Альтернативные названия инструмента пока не указаны
              </p>
            </article>
          </div>

          <p v-else class="empty-text">
            Пока нет данных
          </p>
        </section>

        <section class="detail-section">
          <h2 class="detail-section-title">Домены</h2>

          <div v-if="domains.length > 0" class="detail-list">
            <article
              v-for="(domain, index) in domains"
              :key="domain.name ?? index"
              class="detail-item"
            >
              <h3>{{ domain.name }}</h3>

              <div
                v-if="domain.alternativeNames?.length"
                class="alias-list"
              >
                <v-chip
                  v-for="alias in domain.alternativeNames"
                  :key="alias"
                  class="alias-chip"
                  variant="flat"
                  size="small"
                >
                  {{ alias }}
                </v-chip>
              </div>
            </article>
          </div>

          <p v-else class="empty-text">
            Пока нет данных
          </p>
        </section>
      </div>
    </Transition>

    <div class="actions-row">
      <v-btn
        color="grey-darken-1"
        rounded="xl"
        variant="tonal"
        class="text-none details-button"
        @click="toggleDetailed"
      >
        {{ isDetailed ? 'Кратко' : 'Подробнее' }}
      </v-btn>
    </div>
  </v-card>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import type { UserProfileDto } from '@/services/interviewApi'

type SpecializationViewModel = {
  name?: string | null
  alternativeNames?: string[] | null
}

type SkillViewModel = {
  displayName?: string | null
  description?: string | null
  dominanceLevel?: string | null
  alternativeNames?: string[] | null
}

type ToolViewModel = {
  toolStandardName?: string | null
  usageFrequency?: string | null
  toolAltNames?: string[] | null
}

type DomainViewModel = {
  name?: string | null
  alternativeNames?: string[] | null
}

type UserProfileViewModel = {
  cluster?: string | null
  specializations?: SpecializationViewModel[] | null
  skills?: SkillViewModel[] | null
  tools?: ToolViewModel[] | null
  preferredDomains?: DomainViewModel[] | null
}

const props = defineProps<{
  profile: UserProfileDto
}>()

const emit = defineEmits<{
  viewChange: [isDetailed: boolean]
}>()

const isDetailed = ref(false)

const profileView = computed(() => props.profile as unknown as UserProfileViewModel)

const specializations = computed(() => profileView.value.specializations ?? [])
const skills = computed(() => profileView.value.skills ?? [])
const tools = computed(() => profileView.value.tools ?? [])
const domains = computed(() => profileView.value.preferredDomains ?? [])

const summarySections = computed(() => [
  {
    title: 'Специализации',
    items: specializations.value.map((item) => item.name).filter(Boolean) as string[],
  },
  {
    title: 'Навыки',
    items: skills.value.map((item) => item.displayName).filter(Boolean) as string[],
  },
  {
    title: 'Инструменты',
    items: tools.value.map((item) => item.toolStandardName).filter(Boolean) as string[],
  },
  {
    title: 'Домены',
    items: domains.value.map((item) => item.name).filter(Boolean) as string[],
  },
])

const skillLevelLabels: Record<string, string> = {
  core: 'Ключевой',
  important: 'Важный',
  secondary: 'Дополнительный',
}

const toolLevelLabels: Record<string, string> = {
  core: 'Ключевой',
  regular: 'Регулярный',
  occasional: 'Иногда',
  secondary: 'Дополнительный',
}

function toggleDetailed() {
  isDetailed.value = !isDetailed.value
  emit('viewChange', isDetailed.value)
}

function getNormalizedLevel(level?: string | null) {
  return level?.trim().toLowerCase() || 'unknown'
}

function getSkillLevelLabel(level?: string | null) {
  const normalizedLevel = getNormalizedLevel(level)
  return skillLevelLabels[normalizedLevel] ?? level ?? 'Уровень не указан'
}

function getToolLevelLabel(level?: string | null) {
  const normalizedLevel = getNormalizedLevel(level)
  return toolLevelLabels[normalizedLevel] ?? level ?? 'Уровень не указан'
}
</script>

<style scoped>
.conclusion-card {
  width: min(760px, 100%);
  border-radius: 2rem;
  padding: 1.55rem;
  background: rgba(255, 255, 255, 0.96);
  box-shadow: 0 22px 62px rgba(23, 42, 97, 0.16);
  color: black;
  font-family: 'Styrene A Web', Roboto, Arial, sans-serif;
}

.conclusion-header {
  display: flex;
  align-items: center;
  gap: 0.9rem;
}

.conclusion-icon {
  flex: 0 0 auto;
  background: rgba(255, 61, 27, 0.12);
  color: rgb(255, 61, 27);
}

.conclusion-heading {
  min-width: 0;
}

.conclusion-heading h1 {
  margin: 0;
  color: black;
  font-size: 1.45rem;
  font-weight: 600;
  line-height: 1.08;
  letter-spacing: -0.03em;
}

.conclusion-heading p {
  margin: 0.3rem 0 0;
  color: rgba(0, 0, 0, 0.56);
  font-size: 0.92rem;
  font-weight: 400;
  line-height: 1.35;
}

.summary-view,
.details-view {
  margin-top: 1.15rem;
}

.cluster-card {
  border-radius: 1.25rem;
  padding: 1rem;
  background: linear-gradient(135deg, #004eea 0%, #4030f2 100%);
  color: white;
}

.cluster-card span {
  display: block;
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.72rem;
  font-weight: 600;
  line-height: 1;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.cluster-card strong {
  display: block;
  margin-top: 0.45rem;
  font-size: 1.28rem;
  font-weight: 600;
  line-height: 1.1;
  letter-spacing: -0.03em;
}

.summary-sections {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.85rem;
  margin-top: 0.9rem;
}

.summary-section {
  min-width: 0;
  border-radius: 1.2rem;
  padding: 0.95rem;
  background: rgba(0, 78, 234, 0.045);
}

.summary-section h2 {
  margin: 0;
  color: black;
  font-size: 0.95rem;
  font-weight: 600;
  line-height: 1.2;
}

.chip-list,
.alias-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.42rem;
}

.chip-list {
  margin-top: 0.7rem;
}

.profile-chip,
.alias-chip {
  max-width: 100%;
  border-radius: 0.65rem;
  font-weight: 400;
  line-height: 1.2;
}

.profile-chip {
  min-height: 1.65rem;
  padding: 0.15rem 0.38rem;
  background: white;
  color: rgba(0, 0, 0, 0.78);
  font-size: 0.8rem;
}

.alias-chip {
  min-height: 1.55rem;
  padding: 0.12rem 0.36rem;
  background: rgba(0, 0, 0, 0.045);
  color: rgba(0, 0, 0, 0.58);
  font-size: 0.74rem;
}

.profile-chip :deep(.v-chip__content),
.alias-chip :deep(.v-chip__content) {
  min-width: 0;
  max-width: 100%;
  white-space: normal;
  overflow-wrap: anywhere;
}

.empty-text {
  margin: 0.65rem 0 0;
  color: rgba(0, 0, 0, 0.48);
  font-size: 0.84rem;
  font-weight: 400;
  line-height: 1.35;
}

.details-view {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.detail-section {
  border-radius: 1.25rem;
  padding: 1rem;
  background: rgba(0, 78, 234, 0.045);
}

.detail-section-title {
  margin: 0 0 0.85rem;
  color: black;
  font-size: 1.28rem;
  font-weight: 600;
  line-height: 1.1;
  letter-spacing: -0.03em;
}

.detail-main-card {
  border-radius: 1rem;
  padding: 0.85rem 0.95rem;
  background: white;
}

.detail-main-card p {
  margin: 0;
  color: #004eea;
  font-size: 1.05rem;
  font-weight: 500;
  line-height: 1.25;
}

.detail-list,
.tool-grid {
  display: grid;
  gap: 0.65rem;
}

.tool-grid {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.detail-item,
.tool-item {
  min-width: 0;
  overflow: hidden;
  border-radius: 1rem;
  padding: 0.9rem;
  background: white;
}

.detail-item-header,
.tool-title-row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.75rem;
}

.detail-item h3,
.tool-item h3 {
  min-width: 0;
  margin: 0;
  color: black;
  font-size: 0.98rem;
  font-weight: 500;
  line-height: 1.25;
  letter-spacing: -0.02em;
  overflow-wrap: anywhere;
}

.detail-description {
  margin: 0.5rem 0 0;
  color: rgba(0, 0, 0, 0.58);
  font-size: 0.84rem;
  font-weight: 400;
  line-height: 1.42;
  overflow-wrap: anywhere;
}

.alias-list {
  margin-top: 0.65rem;
}

.level-chip {
  flex: 0 0 auto;
  min-height: 1.7rem;
  height: auto;
  max-width: 100%;
  border-radius: 999px;
  padding: 0.18rem 0.6rem;
  font-size: 0.76rem;
  font-weight: 500;
  line-height: 1.15;
}

.level-chip :deep(.v-chip__content) {
  min-width: 0;
  white-space: normal;
  overflow: visible;
  overflow-wrap: anywhere;
}

.level-chip--core {
  background: rgba(0, 78, 234, 0.11);
  color: #004eea;
}

.level-chip--important,
.level-chip--regular {
  background: rgba(255, 61, 27, 0.11);
  color: rgb(255, 61, 27);
}

.level-chip--secondary,
.level-chip--occasional,
.level-chip--unknown {
  background: rgba(0, 0, 0, 0.06);
  color: rgba(0, 0, 0, 0.58);
}

.actions-row {
  display: flex;
  justify-content: flex-start;
  margin-top: 1rem;
}

.details-button {
  padding: 0 1.15rem;
  font-weight: 500;
}

.conclusion-view-enter-active,
.conclusion-view-leave-active {
  transition:
    opacity 0.18s ease,
    transform 0.18s ease;
}

.conclusion-view-enter-from,
.conclusion-view-leave-to {
  opacity: 0;
  transform: translateY(0.35rem);
}

@media (max-width: 760px) {
  .summary-sections,
  .tool-grid {
    grid-template-columns: minmax(0, 1fr);
  }

  .conclusion-card {
    padding: 1.2rem;
  }

  .detail-item-header,
  .tool-title-row {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>