<template>
  <article
    class="job-card"
    :class="{ 'job-card--expanded': isExpanded }"
  >
    <div class="job-card-summary">
      <div
        class="job-source-logo"
        :class="{ 'job-source-logo--fallback': shouldUseSourceLogoFallback }"
      >
        <img
          v-if="!shouldUseSourceLogoFallback"
          :src="sourceInfo.logo"
          :alt="`${sourceInfo.name} logo`"
          @error="useFallbackLogo"
        >

        <span v-else aria-hidden="true" />
      </div>

      <div class="job-content">
        <div class="job-source-line">
          <span class="job-source-name">{{ sourceInfo.name }}</span>
          <span class="job-dot" />
          <span>{{ formatRelativeTime(job.postedAt) }}</span>
          <template v-if="job.author">
            <span class="job-dot" />
            <span>{{ job.author }}</span>
          </template>
        </div>

        <h2>{{ job.title || 'Заказ без названия' }}</h2>

        <p class="job-description">
          {{ job.description || 'Описание появится после открытия заказа.' }}
        </p>

        <div class="job-meta-row">
          <span v-if="job.cluster">{{ job.cluster }}</span>
          <span v-if="job.domains.length">{{ job.domains[0].name }}</span>
          <span v-if="job.attachedFiles.length">{{ formatAttachments(job.attachedFiles.length) }}</span>
        </div>

        <div class="job-chip-row job-chip-row--preview">
          <v-chip
            v-for="tag in previewTags"
            :key="tag"
            class="job-chip job-chip--summary"
            variant="flat"
            size="small"
          >
            {{ tag }}
          </v-chip>
        </div>
      </div>

      <aside class="job-side">
        <v-btn
          class="job-icon-button"
          :href="job.url"
          target="_blank"
          rel="noopener noreferrer"
          icon
          variant="flat"
          aria-label="Открыть заказ"
        >
          <v-icon icon="mdi-open-in-new" size="1.15rem" />
        </v-btn>

        <div
          class="job-budget-card"
          :class="{ 'job-budget-card--muted': !budget.isSpecified }"
        >
          <span>Бюджет</span>
          <strong>{{ budget.text }}</strong>
        </div>

        <v-btn
          class="job-expand-button"
          rounded="xl"
          variant="tonal"
          color="grey-darken-1"
          @click="toggleExpanded"
        >
          {{ isExpanded ? 'Свернуть' : 'Подробнее' }}
        </v-btn>
      </aside>
    </div>

    <v-expand-transition>
      <div
        v-show="isExpanded"
        class="job-details"
      >
        <section
          v-if="job.description"
          class="job-detail-section job-detail-section--description"
        >
          <h3>Описание</h3>
          <p>{{ job.description }}</p>
        </section>

        <section
          v-if="job.specializations.length"
          class="job-detail-section"
        >
          <h3>Специализации</h3>

          <div class="job-detail-card-list job-detail-card-list--compact">
            <div
              v-for="specialization in job.specializations"
              :key="`${job.id}-specialization-${specialization.name}`"
              class="job-detail-mini-card"
            >
              <strong>{{ specialization.name }}</strong>

              <div
                v-if="specialization.alternativeNames.length"
                class="job-chip-row"
              >
                <v-chip
                  v-for="alias in specialization.alternativeNames"
                  :key="alias"
                  class="job-chip job-chip--alias"
                  variant="flat"
                  size="small"
                >
                  {{ alias }}
                </v-chip>
              </div>
            </div>
          </div>
        </section>

        <section
          v-if="job.requiredSkills.length"
          class="job-detail-section"
        >
          <h3>Обязательные навыки</h3>

          <div class="job-detail-card-list">
            <div
              v-for="skill in job.requiredSkills"
              :key="`${job.id}-required-skill-${skill.displayName}`"
              class="job-detail-card"
            >
              <div class="job-detail-card-header">
                <strong>{{ skill.displayName }}</strong>
                <v-chip
                  class="job-level-chip job-level-chip--required"
                  variant="flat"
                  size="small"
                >
                  Обязательный
                </v-chip>
              </div>

              <p>{{ skill.description }}</p>

              <div
                v-if="skill.alternativeNames.length"
                class="job-chip-row"
              >
                <v-chip
                  v-for="alias in skill.alternativeNames"
                  :key="alias"
                  class="job-chip job-chip--alias"
                  variant="flat"
                  size="small"
                >
                  {{ alias }}
                </v-chip>
              </div>
            </div>
          </div>
        </section>

        <section
          v-if="job.bonusSkills.length"
          class="job-detail-section"
        >
          <h3>Будет плюсом</h3>

          <div class="job-detail-card-list">
            <div
              v-for="skill in job.bonusSkills"
              :key="`${job.id}-bonus-skill-${skill.displayName}`"
              class="job-detail-card"
            >
              <div class="job-detail-card-header">
                <strong>{{ skill.displayName }}</strong>
                <v-chip
                  class="job-level-chip job-level-chip--bonus"
                  variant="flat"
                  size="small"
                >
                  Бонус
                </v-chip>
              </div>

              <p>{{ skill.description }}</p>

              <div
                v-if="skill.alternativeNames.length"
                class="job-chip-row"
              >
                <v-chip
                  v-for="alias in skill.alternativeNames"
                  :key="alias"
                  class="job-chip job-chip--alias"
                  variant="flat"
                  size="small"
                >
                  {{ alias }}
                </v-chip>
              </div>
            </div>
          </div>
        </section>

        <section
          v-if="job.requiredTools.length || job.bonusTools.length"
          class="job-detail-section"
        >
          <h3>Инструменты</h3>

          <div class="job-tools-grid">
            <div
              v-for="tool in job.requiredTools"
              :key="`${job.id}-required-tool-${tool.toolStandardName}`"
              class="job-tool-card"
            >
              <div class="job-detail-card-header">
                <strong>{{ tool.toolStandardName }}</strong>
                <v-chip
                  class="job-level-chip job-level-chip--required"
                  variant="flat"
                  size="small"
                >
                  Обязательный
                </v-chip>
              </div>

              <div
                v-if="tool.toolAltNames.length"
                class="job-chip-row"
              >
                <v-chip
                  v-for="alias in tool.toolAltNames"
                  :key="alias"
                  class="job-chip job-chip--alias"
                  variant="flat"
                  size="small"
                >
                  {{ alias }}
                </v-chip>
              </div>
            </div>

            <div
              v-for="tool in job.bonusTools"
              :key="`${job.id}-bonus-tool-${tool.toolStandardName}`"
              class="job-tool-card"
            >
              <div class="job-detail-card-header">
                <strong>{{ tool.toolStandardName }}</strong>
                <v-chip
                  class="job-level-chip job-level-chip--bonus"
                  variant="flat"
                  size="small"
                >
                  Бонус
                </v-chip>
              </div>

              <div
                v-if="tool.toolAltNames.length"
                class="job-chip-row"
              >
                <v-chip
                  v-for="alias in tool.toolAltNames"
                  :key="alias"
                  class="job-chip job-chip--alias"
                  variant="flat"
                  size="small"
                >
                  {{ alias }}
                </v-chip>
              </div>
            </div>
          </div>
        </section>

        <section
          v-if="job.domains.length"
          class="job-detail-section"
        >
          <h3>Домены</h3>

          <div class="job-chip-row">
            <v-chip
              v-for="domain in job.domains"
              :key="`${job.id}-domain-${domain.name}`"
              class="job-chip job-chip--summary"
              variant="flat"
              size="small"
            >
              {{ domain.name }}
            </v-chip>
          </div>
        </section>

        <section
          v-if="job.attachedFiles.length"
          class="job-detail-section"
        >
          <h3>Вложения</h3>

          <div class="job-attachment-list">
            <a
              v-for="(file, index) in job.attachedFiles"
              :key="`${job.id}-file-${index}`"
              :href="file.url || undefined"
              target="_blank"
              rel="noopener noreferrer"
              class="job-attachment"
              :class="{ 'job-attachment--disabled': !file.url }"
            >
              <v-icon icon="mdi-paperclip" size="1rem" />
              <span>{{ file.extension.toUpperCase() }}</span>
            </a>
          </div>
        </section>
      </div>
    </v-expand-transition>
  </article>
</template>

<script setup lang="ts">
import { computed, nextTick, ref } from 'vue'
import type { JobPostingDto } from '@/services/recommendationsApi'

const props = defineProps<{
  job: JobPostingDto
}>()

const emit = defineEmits<{
  contentChange: []
}>()

const isExpanded = ref(false)
const logoFailed = ref(false)

const sourceMeta: Record<string, { name: string; logo: string; shortName: string }> = {
  tg: {
    name: 'Telegram',
    logo: 'https://cdn.simpleicons.org/telegram/229ED9',
    shortName: 'TG',
  },
  kwork: {
    name: 'Kwork',
    logo: 'https://kwork.ru/favicon.ico',
    shortName: 'KW',
  },
}

const sourceInfo = computed(() => {
  const source = props.job.source.trim().toLowerCase()
  const fallbackName = props.job.source || 'Источник'

  return sourceMeta[source] ?? {
    name: fallbackName,
    logo: '',
    shortName: fallbackName.slice(0, 2).toUpperCase(),
  }
})

const shouldUseSourceLogoFallback = computed(() => !sourceInfo.value.logo || logoFailed.value)

const budget = computed(() => formatBudget(props.job))

const previewTags = computed(() => getUniqueStrings([
  ...props.job.specializations.map((specialization) => specialization.name),
  ...props.job.requiredSkills.map((skill) => skill.displayName),
  ...props.job.bonusSkills.map((skill) => skill.displayName),
  ...props.job.requiredTools.map((tool) => tool.toolStandardName),
  ...props.job.bonusTools.map((tool) => tool.toolStandardName),
  ...props.job.domains.map((domain) => domain.name),
]).slice(0, 8))

function useFallbackLogo() {
  logoFailed.value = true
}

async function toggleExpanded() {
  isExpanded.value = !isExpanded.value
  await nextTick()
  emit('contentChange')
}

function getUniqueStrings(values: string[]) {
  return Array.from(new Set(values.map((value) => value.trim()).filter(Boolean)))
}

function formatBudget(job: JobPostingDto) {
  if (!job.priceMin && !job.priceMax) {
    return {
      text: 'Не указан',
      isSpecified: false,
    }
  }

  const minPrice = job.priceMin ? formatPrice(job.priceMin) : null
  const maxPrice = job.priceMax ? formatPrice(job.priceMax) : null
  const currency = job.currency.trim().toLowerCase()

  const withCurrency = (value: string) => {
    if (currency === 'rub') {
      return `${value} ₽`
    }

    if (currency === 'usd') {
      return `$${value}`
    }

    if (currency === 'eur') {
      return `€${value}`
    }

    return value
  }

  if (minPrice && maxPrice) {
    if (minPrice === maxPrice) {
      return {
        text: withCurrency(minPrice),
        isSpecified: true,
      }
    }

    if (currency === 'rub' || currency === 'unspecified') {
      return {
        text: withCurrency(`${minPrice}–${maxPrice}`),
        isSpecified: true,
      }
    }

    const symbol = currency === 'usd' ? '$' : currency === 'eur' ? '€' : ''
    return {
      text: `${symbol}${minPrice}–${maxPrice}`,
      isSpecified: true,
    }
  }

  if (minPrice) {
    return {
      text: `от ${withCurrency(minPrice)}`,
      isSpecified: true,
    }
  }

  return {
    text: `до ${withCurrency(maxPrice ?? '')}`,
    isSpecified: true,
  }
}

function formatPrice(value: string) {
  const normalizedValue = value.trim().replace(',', '.')
  const numericValue = Number(normalizedValue)

  if (!Number.isFinite(numericValue)) {
    return value
  }

  return new Intl.NumberFormat('ru-RU', {
    maximumFractionDigits: 0,
  }).format(numericValue)
}

function formatRelativeTime(value: string) {
  const postedAt = new Date(value).getTime()
  const diffMs = Date.now() - postedAt
  const diffMinutes = Math.max(Math.floor(diffMs / 60_000), 1)

  if (diffMinutes < 60) {
    return `${diffMinutes} мин назад`
  }

  const diffHours = Math.floor(diffMinutes / 60)

  if (diffHours < 24) {
    return `${diffHours} ч назад`
  }

  const diffDays = Math.floor(diffHours / 24)

  return `${diffDays} д назад`
}

function formatAttachments(count: number) {
  if (count === 1) {
    return '1 файл'
  }

  if (count > 1 && count < 5) {
    return `${count} файла`
  }

  return `${count} файлов`
}
</script>

<style scoped>
.job-card {
  overflow: hidden;
  border-radius: 2rem;
  background: rgba(255, 255, 255, 0.96);
  box-shadow: 0 22px 62px rgba(23, 42, 97, 0.14);
  color: black;
}

.job-card-summary {
  display: grid;
  grid-template-columns: 3.4rem minmax(0, 1fr) minmax(9.25rem, 11rem);
  align-items: stretch;
  gap: 1.05rem;
  padding: 1.1rem;
}

.job-source-logo {
  width: 3.35rem;
  height: 3.35rem;
  flex: 0 0 auto;
  border-radius: 999px;
  background: white;
  display: grid;
  place-items: center;
  overflow: hidden;
  box-shadow: 0 12px 30px rgba(0, 70, 212, 0.16);
}

.job-source-logo--fallback {
  background: #004eea;
}

.job-source-logo img {
  width: 68%;
  height: 68%;
  display: block;
  object-fit: contain;
}

.job-source-logo span {
  width: 100%;
  height: 100%;
}

.job-content {
  min-width: 0;
}

.job-source-line {
  min-width: 0;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.4rem;
  color: rgba(0, 0, 0, 0.46);
  font-size: 0.82rem;
  font-weight: 500;
  line-height: 1.25;
}

.job-source-name {
  color: black;
  font-weight: 600;
}

.job-dot {
  width: 0.25rem;
  height: 0.25rem;
  border-radius: 999px;
  background: rgba(0, 0, 0, 0.25);
}

.job-card h2 {
  margin: 0.45rem 0 0;
  color: black;
  font-size: 1.22rem;
  font-weight: 600;
  line-height: 1.16;
  letter-spacing: -0.03em;
  overflow-wrap: anywhere;
}

.job-description {
  display: -webkit-box;
  margin: 0.55rem 0 0;
  color: rgba(0, 0, 0, 0.58);
  font-size: 0.92rem;
  font-weight: 400;
  line-height: 1.43;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  overflow: hidden;
}

.job-card--expanded .job-description {
  -webkit-line-clamp: 4;
}

.job-meta-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem 0.75rem;
  margin-top: 0.8rem;
  color: rgba(0, 0, 0, 0.46);
  font-size: 0.82rem;
  font-weight: 500;
}

.job-meta-row span:not(:last-child)::after {
  content: "•";
  margin-left: 0.75rem;
  color: rgba(0, 0, 0, 0.26);
}

.job-side {
  min-width: 0;
  align-self: stretch;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.65rem;
}

.job-icon-button {
  width: 2.45rem;
  height: 2.45rem;
  background: rgba(0, 0, 0, 0.045);
  color: rgba(0, 0, 0, 0.65);
}

.job-budget-card {
  width: 100%;
  border-radius: 1.05rem;
  padding: 0.72rem 0.8rem;
  background: rgba(0, 78, 234, 0.075);
  color: #004eea;
  text-align: right;
}

.job-budget-card--muted {
  background: rgba(0, 0, 0, 0.045);
  color: rgba(0, 0, 0, 0.52);
}

.job-budget-card span,
.job-budget-card strong {
  display: block;
}

.job-budget-card span {
  font-size: 0.7rem;
  font-weight: 600;
  line-height: 1;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  opacity: 0.72;
}

.job-budget-card strong {
  margin-top: 0.35rem;
  font-size: 0.98rem;
  font-weight: 600;
  line-height: 1.15;
  letter-spacing: -0.02em;
  overflow-wrap: anywhere;
}

.job-expand-button {
  min-width: 7.2rem;
  margin-top: auto;
  font-weight: 500;
}

.job-chip-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.42rem;
  margin-top: 0.85rem;
}

.job-chip,
.job-level-chip {
  max-width: 100%;
  height: auto;
  border-radius: 0.65rem;
  line-height: 1.2;
}

.job-chip {
  min-height: 1.65rem;
  padding: 0.15rem 0.38rem;
  background: white;
  color: rgba(0, 0, 0, 0.76);
  font-size: 0.8rem;
  font-weight: 400;
}

.job-chip--summary {
  background: white;
}

.job-chip--alias {
  min-height: 1.55rem;
  padding: 0.12rem 0.36rem;
  background: rgba(0, 0, 0, 0.045);
  color: rgba(0, 0, 0, 0.58);
  font-size: 0.74rem;
}

.job-level-chip {
  flex: 0 0 auto;
  min-height: 1.7rem;
  border-radius: 999px;
  padding: 0.18rem 0.6rem;
  font-size: 0.76rem;
  font-weight: 500;
  line-height: 1.15;
}

.job-level-chip--required {
  background: rgba(0, 78, 234, 0.11);
  color: #004eea;
}

.job-level-chip--bonus {
  background: rgba(255, 61, 27, 0.11);
  color: rgb(255, 61, 27);
}

.job-chip :deep(.v-chip__content),
.job-level-chip :deep(.v-chip__content) {
  min-width: 0;
  max-width: 100%;
  white-space: normal;
  overflow: visible;
  overflow-wrap: anywhere;
}

.job-details {
  padding: 0 1.1rem 1.1rem;
}

.job-details::before {
  content: "";
  display: block;
  height: 1px;
  margin-bottom: 1rem;
  background: rgba(0, 0, 0, 0.06);
}

.job-detail-section {
  padding: 1rem;
  border-radius: 1.4rem;
  background: rgba(0, 78, 234, 0.045);
}

.job-detail-section + .job-detail-section {
  margin-top: 0.85rem;
}

.job-detail-section h3 {
  margin: 0 0 0.75rem;
  color: black;
  font-size: 1.05rem;
  font-weight: 600;
  line-height: 1.15;
  letter-spacing: -0.02em;
}

.job-detail-section--description p,
.job-detail-card p {
  margin: 0;
  color: rgba(0, 0, 0, 0.58);
  font-size: 0.92rem;
  line-height: 1.45;
}

.job-detail-card-list,
.job-tools-grid {
  display: grid;
  gap: 0.75rem;
}

.job-detail-card-list--compact,
.job-tools-grid {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.job-detail-card,
.job-detail-mini-card,
.job-tool-card {
  min-width: 0;
  padding: 0.95rem;
  border-radius: 1.15rem;
  background: white;
}

.job-detail-card-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.75rem;
  margin-bottom: 0.6rem;
}

.job-detail-card strong,
.job-detail-mini-card strong,
.job-tool-card strong {
  color: black;
  font-size: 0.98rem;
  font-weight: 500;
  line-height: 1.25;
  overflow-wrap: anywhere;
}

.job-attachment-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.job-attachment {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.45rem 0.65rem;
  border-radius: 999px;
  background: white;
  color: rgba(0, 0, 0, 0.72);
  font-size: 0.82rem;
  font-weight: 500;
  text-decoration: none;
}

.job-attachment--disabled {
  pointer-events: none;
  opacity: 0.5;
}

@media (max-width: 900px) {
  .job-card-summary {
    grid-template-columns: 3.4rem minmax(0, 1fr);
  }

  .job-side {
    grid-column: 1 / -1;
    width: 100%;
    display: grid;
    grid-template-columns: auto minmax(0, 1fr) auto;
    align-items: center;
  }

  .job-budget-card {
    text-align: left;
  }

  .job-expand-button {
    margin-top: 0;
  }

  .job-detail-card-list--compact,
  .job-tools-grid {
    grid-template-columns: minmax(0, 1fr);
  }
}
</style>
