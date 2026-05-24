<template>
  <div
    v-show="loading"
    class="grid-loader"
    :style="loaderStyle"
    aria-hidden="true"
  >
    <span
      v-for="index in 9"
      :key="index"
      class="grid-loader-dot"
      :style="getDotStyle(index)"
    />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  loading?: boolean
  color?: string
  size?: string
  margin?: string
  radius?: string
}>(), {
  loading: true,
  color: '#ffffff',
  size: '13px',
  margin: '3px',
  radius: '100%',
})

const loaderStyle = computed(() => ({
  display: 'grid',
  gridTemplateColumns: `repeat(3, ${props.size})`,
  gridTemplateRows: `repeat(3, ${props.size})`,
  gap: `calc(${props.margin} * 2)`,
}))

const animationDelays = [
  '0.2s',
  '0.3s',
  '0.4s',
  '0.1s',
  '0.2s',
  '0.3s',
  '0s',
  '0.1s',
  '0.2s',
]

function getDotStyle(index: number) {
  return {
    width: props.size,
    height: props.size,
    backgroundColor: props.color,
    borderRadius: props.radius,
    animationDelay: animationDelays[index - 1],
  }
}
</script>

<style scoped>
.grid-loader {
  width: fit-content;
  height: fit-content;
  line-height: 0;
}

.grid-loader-dot {
  display: block;
  opacity: 1;
  animation: grid-loader-scale 1.3s infinite ease-in-out both;
}

@keyframes grid-loader-scale {
  0%,
  70%,
  100% {
    transform: scale3d(1, 1, 1);
  }

  35% {
    transform: scale3d(0, 0, 1);
  }
}
</style>