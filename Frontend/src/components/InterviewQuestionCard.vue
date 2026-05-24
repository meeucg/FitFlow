<template>
  <div
    class="form-container"
    :class="{ 'form-container--wide': needsWideOptionLayout }"
  >
    <div class="question-container">
      <h1 class="question-text">
        {{ question.questionText }}
      </h1>
    </div>

    <div
      v-if="question.answerOptions.length > 0 && question.isSingleChoice"
      class="options-container"
      :class="{ 'options-container--columns': useOptionColumns }"
    >
      <div
        v-for="(option, optionIndex) in question.answerOptions"
        :key="option"
        class="option"
        :class="{ 'option--levels': hasLevels }"
        @click="selectSingleOption(optionIndex)"
      >
        <div
          class="option-row"
          :class="{ 'option-row--levels': hasLevels }"
        >
          <p class="option-number">{{ optionIndex + 1 }}.</p>
          <p class="option-text" :title="option">{{ option }}</p>

          <div
            v-if="hasLevels"
            class="level-slider"
            @click.stop
          >
            <p class="level-hint">{{ getLevelHint(optionIndex) }}</p>
            <v-slider
              v-model="selectedLevels[optionIndex]"
              color="#004EEA"
              density="compact"
              hide-details
              :max="question.answerLevels.length"
              :min="0"
              :step="1"
              show-ticks="always"
              @update:model-value="(value) => handleLevelChange(optionIndex, value)"
            />
          </div>

          <div v-if="!hasLevels" class="option-chooser">
            <v-radio
              :model-value="isOptionSelected(optionIndex)"
              color="rgb(255, 61, 27)"
              hide-details
              @click.stop="selectSingleOption(optionIndex)"
            />
          </div>
        </div>

      </div>
    </div>

    <div
      v-else-if="question.answerOptions.length > 0"
      class="options-container"
      :class="{ 'options-container--columns': useOptionColumns }"
    >
      <div
        v-for="(option, optionIndex) in question.answerOptions"
        :key="option"
        class="option"
        :class="{ 'option--levels': hasLevels }"
        @click="toggleOption(optionIndex)"
      >
        <div
          class="option-row"
          :class="{ 'option-row--levels': hasLevels }"
        >
          <p class="option-number">{{ optionIndex + 1 }}.</p>
          <p class="option-text" :title="option">{{ option }}</p>

          <div
            v-if="hasLevels"
            class="level-slider"
            @click.stop
          >
            <p class="level-hint">{{ getLevelHint(optionIndex) }}</p>
            <v-slider
              v-model="selectedLevels[optionIndex]"
              color="#004EEA"
              density="compact"
              hide-details
              :max="question.answerLevels.length"
              :min="0"
              :step="1"
              show-ticks="always"
              @update:model-value="(value) => handleLevelChange(optionIndex, value)"
            />
          </div>

          <div v-if="!hasLevels" class="option-chooser">
            <v-checkbox
              :model-value="isOptionSelected(optionIndex)"
              color="rgb(255, 61, 27)"
              hide-details
              @click.stop="toggleOption(optionIndex)"
            />
          </div>
        </div>

      </div>
    </div>

    <v-textarea
      v-if="question.plainTextOptionPresent"
      v-model="textAnswer"
      class="text-answer"
      auto-grow
      bg-color="#f2f4f8"
      color="#004EEA"
      placeholder="Ваш ответ"
      rows="3"
      variant="solo"
      flat
    />

    <div class="buttons-container">
      <v-btn
        :disabled="!question.isOptional || loading"
        color="#004EEA"
        rounded="xl"
        variant="tonal"
        class="text-none form-button"
        @click="submitSkip"
      >
        Пропустить
      </v-btn>

      <v-btn
        color="black"
        rounded="xl"
        variant="flat"
        class="text-none form-button"
        :disabled="!canSubmit"
        :loading="loading"
        @click="submitAnswer"
      >
        Далее
      </v-btn>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { AnswerDto, QuestionDto } from '@/services/interviewApi'

const props = defineProps<{
  question: QuestionDto
  loading?: boolean
}>()

const emit = defineEmits<{
  submit: [answer: AnswerDto]
}>()

const singleChoice = ref<number | null>(null)
const selectedOptions = ref<number[]>([])
const selectedLevels = ref<Record<number, number>>({})
const textAnswer = ref('')

const hasLevels = computed(() => props.question.answerLevels.length > 0)

const useOptionColumns = computed(() => props.question.answerOptions.length > 4)

const needsWideOptionLayout = computed(() =>
  hasLevels.value ||
  props.question.answerOptions.some((option) => option.length > 48) ||
  (useOptionColumns.value && props.question.answerOptions.some((option) => option.length > 28)),
)

const selectedOptionIds = computed(() => {
  if (props.question.answerOptions.length === 0) {
    return []
  }

  if (hasLevels.value) {
    return props.question.answerOptions
      .map((_, optionIndex) => optionIndex)
      .filter((optionIndex) => (selectedLevels.value[optionIndex] ?? 0) > 0)
  }

  if (props.question.isSingleChoice) {
    return singleChoice.value === null ? [] : [singleChoice.value]
  }

  return selectedOptions.value
})

const hasTextAnswer = computed(() => textAnswer.value.trim().length > 0)

const selectedLevelsAreValid = computed(() => {
  if (!hasLevels.value) {
    return true
  }

  return selectedOptionIds.value.every((optionId) => (selectedLevels.value[optionId] ?? 0) > 0)
})

const canSubmit = computed(() => {
  if (props.loading) {
    return false
  }

  if (selectedOptionIds.value.length > 0) {
    return selectedLevelsAreValid.value
  }

  return props.question.plainTextOptionPresent && hasTextAnswer.value
})

function resetAnswer() {
  singleChoice.value = null
  selectedOptions.value = []
  selectedLevels.value = hasLevels.value
    ? Object.fromEntries(props.question.answerOptions.map((_, optionIndex) => [optionIndex, 0]))
    : {}
  textAnswer.value = ''
}

function ensureLevelDefault(optionIndex: number) {
  if (hasLevels.value && selectedLevels.value[optionIndex] === undefined) {
    selectedLevels.value[optionIndex] = 0
  }
}

function selectSingleOption(optionIndex: number) {
  if (hasLevels.value) {
    const nextLevels = Object.fromEntries(props.question.answerOptions.map((_, currentIndex) => [
      currentIndex,
      currentIndex === optionIndex ? Math.max(selectedLevels.value[optionIndex] ?? 0, 1) : 0,
    ]))

    selectedLevels.value = nextLevels
  }

  singleChoice.value = optionIndex
  ensureLevelDefault(optionIndex)
}

function toggleOption(optionIndex: number) {
  if (hasLevels.value) {
    const wasSelected = (selectedLevels.value[optionIndex] ?? 0) > 0

    handleLevelChange(optionIndex, wasSelected ? 0 : Math.max(selectedLevels.value[optionIndex] ?? 0, 1))
    return
  }

  if (selectedOptions.value.includes(optionIndex)) {
    selectedOptions.value = selectedOptions.value.filter((selectedOption) => selectedOption !== optionIndex)
    delete selectedLevels.value[optionIndex]
    return
  }

  selectedOptions.value = [...selectedOptions.value, optionIndex]
  ensureLevelDefault(optionIndex)
}

function isOptionSelected(optionIndex: number) {
  if (hasLevels.value) {
    return (selectedLevels.value[optionIndex] ?? 0) > 0
  }

  if (props.question.isSingleChoice) {
    return singleChoice.value === optionIndex
  }

  return selectedOptions.value.includes(optionIndex)
}

function handleLevelChange(optionIndex: number, value: number | number[]) {
  const normalizedLevel = Array.isArray(value)
    ? Number(value[0] ?? 0)
    : Number(value ?? 0)

  if (props.question.isSingleChoice) {
    selectedLevels.value = Object.fromEntries(props.question.answerOptions.map((_, currentIndex) => [
      currentIndex,
      currentIndex === optionIndex ? normalizedLevel : 0,
    ]))

    singleChoice.value = normalizedLevel > 0 ? optionIndex : null
    return
  }

  selectedLevels.value = {
    ...selectedLevels.value,
    [optionIndex]: normalizedLevel,
  }

  const nextSelectedOptions = selectedOptions.value.filter((selectedOption) => selectedOption !== optionIndex)

  selectedOptions.value = normalizedLevel > 0
    ? [...nextSelectedOptions, optionIndex]
    : nextSelectedOptions
}

function getLevelHint(optionIndex: number) {
  const sliderValue = selectedLevels.value[optionIndex] ?? 0

  if (sliderValue <= 0) {
    return 'Не выбрано'
  }

  return props.question.answerLevels[getSubmittedLevel(sliderValue)] ?? 'Не выбрано'
}

function getSubmittedLevel(sliderValue: number) {
  return props.question.answerLevels.length - sliderValue
}

function buildSelectedOptions() {
  return selectedOptionIds.value.map((optionId) => ({
    optionId,
    selectedLevel: hasLevels.value ? getSubmittedLevel(selectedLevels.value[optionId] ?? 0) : null,
  }))
}

function submitAnswer() {
  if (!canSubmit.value) {
    return
  }

  emit('submit', {
    selectedOptions: buildSelectedOptions(),
    textAnswer: hasTextAnswer.value ? textAnswer.value.trim() : null,
    isSkipped: false,
  })
}

function submitSkip() {
  if (!props.question.isOptional || props.loading) {
    return
  }

  emit('submit', {
    selectedOptions: [],
    textAnswer: null,
    isSkipped: true,
  })
}

watch(
  () => props.question,
  resetAnswer,
  { immediate: true },
)
</script>

<style scoped>
.form-container {
  display: flex;
  flex-direction: column;
  align-items: stretch;
  justify-content: flex-start;
  padding: 2rem;
  box-sizing: border-box;
  width: min(600px, calc(100vw - 2rem));
  max-width: calc(100vw - 2rem);
  gap: 1rem;
  border-radius: 2rem;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(32px);
  box-shadow: 0 18px 54px rgba(23, 42, 97, 0.13);
  color: black;
  font-family: 'Styrene A Web', Roboto, Arial, sans-serif;
  transition: width 0.2s ease-in-out;
}

.form-container--wide {
  width: min(1120px, calc(100vw - 2rem));
}

.question-container {
  width: 100%;
}

.question-text {
  font-size: 1.5rem;
  font-weight: 500;
  line-height: 1.18;
  color: black;
}

.options-container {
  width: 100%;
  max-width: 100%;
  min-width: 0;
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  justify-content: flex-start;
  gap: 0.65rem;
}

.options-container :deep(.v-input__control),
.options-container :deep(.v-selection-control-group) {
  width: 100%;
  max-width: 100%;
  min-width: 0;
}

.options-container :deep(.v-input__control) {
  display: block;
}

.options-container :deep(.v-selection-control-group) {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  align-items: stretch;
  justify-content: flex-start;
  justify-items: stretch;
  gap: 0.65rem;
}

.options-container--columns,
.options-container--columns :deep(.v-selection-control-group) {
  grid-template-columns: repeat(2, minmax(0, 1fr));
  column-gap: 2.5rem;
  row-gap: 0.65rem;
}

.option {
  position: relative;
  width: 100%;
  max-width: 100%;
  min-width: 0;
  cursor: pointer;
}

.option-row {
  width: 100%;
  max-width: 100%;
  min-width: 0;
  min-height: var(--v-input-control-height);
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: center;
  column-gap: 0.85rem;
}

.option-row--levels {
  grid-template-columns: auto minmax(0, 3fr) minmax(9rem, 2fr);
  column-gap: 1.4rem;
}

.option-number {
  color: darkgray;
  font-weight: 500;
  white-space: nowrap;
}

.option-text {
  display: -webkit-box;
  min-width: 0;
  max-width: 100%;
  overflow: hidden;
  color: black;
  font-weight: 400;
  line-height: 1.25;
  overflow-wrap: anywhere;
  word-break: normal;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  line-clamp: 2;
}

.option-chooser {
  min-width: 0;
  display: flex;
  justify-content: flex-start;
  align-items: center;
}

.option-chooser :deep(.v-selection-control) {
  min-width: 0;
  justify-content: flex-start;
}

.option-chooser :deep(.v-selection-control__wrapper) {
  margin-inline: 0;
}

.level-slider {
  width: 100%;
  min-width: 0;
  max-width: 100%;
  padding: 0;
}

.level-slider :deep(.v-slider-track__tick-label) {
  display: none;
}

.level-hint {
  min-height: 1rem;
  margin: 0 0 0.1rem;
  color: rgba(0, 0, 0, 0.58);
  font-size: 0.78rem;
  line-height: 1.2;
  text-align: center;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.text-answer {
  width: 100%;
  margin-top: 0.25rem;
}

.text-answer :deep(.v-field) {
  border-radius: 1rem;
  background: #f2f4f8 !important;
  color: black;
  box-shadow: none;
}

.text-answer :deep(.v-field__field) {
  background: #f2f4f8 !important;
}

.text-answer :deep(.v-field__overlay) {
  background: transparent !important;
  opacity: 0 !important;
}

.text-answer :deep(.v-field__input) {
  padding-inline: 1.15rem;
  padding-top: 1.35rem;
  color: black !important;
  caret-color: #004eea;
}

.text-answer :deep(textarea) {
  color: black !important;
}

.text-answer :deep(textarea::placeholder) {
  color: rgba(0, 0, 0, 0.58) !important;
  opacity: 1;
}

.text-answer :deep(.v-field__outline),
.text-answer :deep(.v-field__underlay) {
  display: none;
}

.buttons-container {
  margin-top: 0.25rem;
  width: 100%;
  display: flex;
  flex-direction: row;
  justify-content: flex-start;
  align-items: center;
  gap: 1rem;
}

.form-button {
  padding: 0 1.2rem 0 1.2rem;
  transition: padding 0.2s ease-in-out;
}

.form-button:hover {
  padding: 0 1.5rem 0 1.5rem;
}

@media (max-width: 840px) {
  .options-container--columns,
  .options-container--columns :deep(.v-selection-control-group) {
    grid-template-columns: minmax(0, 1fr);
  }
}
</style>
