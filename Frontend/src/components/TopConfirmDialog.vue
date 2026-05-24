<template>
  <Teleport to="body">
    <Transition name="top-confirm">
      <div
        v-if="modelValue"
        class="top-confirm-overlay"
        @click.self="close"
      >
        <v-card class="top-confirm-card" elevation="0">
          <h2>{{ title }}</h2>
          <p>{{ text }}</p>

          <div class="top-confirm-actions">
            <v-btn
              class="text-none top-confirm-button"
              color="grey-darken-1"
              rounded="xl"
              variant="tonal"
              @click="close"
            >
              {{ cancelText }}
            </v-btn>

            <v-btn
              class="text-none top-confirm-button"
              color="black"
              rounded="xl"
              variant="flat"
              @click="confirm"
            >
              {{ confirmText }}
            </v-btn>
          </div>
        </v-card>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
withDefaults(defineProps<{
  modelValue: boolean
  title: string
  text: string
  cancelText?: string
  confirmText?: string
}>(), {
  cancelText: 'Отмена',
  confirmText: 'Выйти',
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirm: []
}>()

function close() {
  emit('update:modelValue', false)
}

function confirm() {
  emit('confirm')
}
</script>

<style scoped>
.top-confirm-overlay {
  position: fixed;
  inset: 0;
  z-index: 2000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  background: rgba(0, 0, 0, 0.18);
  backdrop-filter: blur(8px) brightness(60%);
  -webkit-backdrop-filter: blur(8px) brightness(60%);
}

.top-confirm-card {
  width: min(420px, calc(100vw - 2rem));
  border-radius: 2rem;
  padding: 1.35rem;
  background: rgba(255, 255, 255, 0.97);
  color: black;
  box-shadow: 0 24px 72px rgba(0, 0, 0, 0.22);
}

.top-confirm-card h2 {
  margin: 0;
  font-size: 1.35rem;
  font-weight: 600;
  line-height: 1.12;
  letter-spacing: -0.03em;
}

.top-confirm-card p {
  margin: 0.55rem 0 0;
  color: rgba(0, 0, 0, 0.58);
  font-size: 0.95rem;
  font-weight: 400;
  line-height: 1.4;
}

.top-confirm-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.65rem;
  margin-top: 1.2rem;
}

.top-confirm-button {
  min-width: 6.2rem;
  font-weight: 600;
}

.top-confirm-enter-active,
.top-confirm-leave-active {
  transition: opacity 0.16s ease;
}

.top-confirm-enter-active .top-confirm-card,
.top-confirm-leave-active .top-confirm-card {
  transition:
    opacity 0.16s ease,
    transform 0.16s ease;
}

.top-confirm-enter-from,
.top-confirm-leave-to {
  opacity: 0;
}

.top-confirm-enter-from .top-confirm-card,
.top-confirm-leave-to .top-confirm-card {
  opacity: 0;
  transform: translateY(0.35rem) scale(0.98);
}
</style>
