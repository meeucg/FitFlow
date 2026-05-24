<template>
  <component
    :is="tag"
    ref="elementRef"
    class="reveal-on-active"
    :class="{ 'reveal-on-active--visible': isVisible }"
    :style="rootStyle"
    :aria-hidden="active ? undefined : 'true'"
    :inert="active ? undefined : true"
  >
    <slot />
  </component>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { gsap } from 'gsap'

type CssLength = string | number

const props = withDefaults(defineProps<{
  active?: boolean
  tag?: string
  y?: CssLength
  enterY?: CssLength
  exitY?: CssLength
  delay?: number
  duration?: number
  exitDuration?: number
  ease?: string
  exitEase?: string
  blur?: CssLength
  appear?: boolean
  collapseWhenHidden?: boolean
}>(), {
  active: true,
  tag: 'div',
  y: 34,
  delay: 0,
  duration: 0.8,
  exitDuration: 0.42,
  ease: 'power3.out',
  exitEase: 'power3.in',
  blur: 4,
  appear: true,
  collapseWhenHidden: false,
})

const elementRef = ref<HTMLElement | null>(null)
const hasMounted = ref(false)
const isVisible = ref(false)
const renderVisible = ref(false)

let currentTween: gsap.core.Tween | null = null
let animationToken = 0

const toCssLength = (value: CssLength) => (
  typeof value === 'number' ? `${value}px` : value
)

const enterOffset = computed(() => props.enterY ?? props.y)
const exitOffset = computed(() => props.exitY ?? props.y)

const hiddenEnterVars = computed(() => ({
  opacity: 0,
  y: enterOffset.value,
  filter: `blur(${toCssLength(props.blur)})`,
}))

const hiddenExitVars = computed(() => ({
  opacity: 0,
  y: exitOffset.value,
  filter: `blur(${toCssLength(props.blur)})`,
}))

const visibleVars = {
  opacity: 1,
  y: 0,
  filter: 'blur(0px)',
}

const rootStyle = computed(() => ({
  pointerEvents: props.active ? undefined : 'none',
  display: props.collapseWhenHidden && !renderVisible.value ? 'none' : undefined,
  visibility: isVisible.value ? 'visible' : 'hidden',
  ...(!hasMounted.value && (props.appear || !props.active)
    ? {
        opacity: 0,
        transform: `translate3d(0, ${toCssLength(enterOffset.value)}, 0)`,
        filter: `blur(${toCssLength(props.blur)})`,
      }
    : {}),
}))

function killTween() {
  currentTween?.kill()
  currentTween = null
}

async function animateIn() {
  const token = ++animationToken

  killTween()
  renderVisible.value = true

  await nextTick()

  if (token !== animationToken) {
    return
  }

  const element = elementRef.value

  if (!element) {
    return
  }

  gsap.set(element, {
    ...hiddenEnterVars.value,
    visibility: 'visible',
  })

  element.style.visibility = 'visible'
  isVisible.value = true

  currentTween = gsap.fromTo(
    element,
    hiddenEnterVars.value,
    {
      ...visibleVars,
      delay: props.delay,
      duration: props.duration,
      ease: props.ease,
      force3D: true,
      willChange: 'transform, opacity, filter',
      onComplete: () => {
        if (token !== animationToken) {
          return
        }

        currentTween = null
      },
    },
  )
}

function animateOut() {
  const token = ++animationToken
  const element = elementRef.value

  if (!element) {
    return
  }

  killTween()
  renderVisible.value = true
  element.style.visibility = 'visible'
  isVisible.value = true

  currentTween = gsap.to(element, {
    ...hiddenExitVars.value,
    duration: props.exitDuration,
    ease: props.exitEase,
    force3D: true,
    willChange: 'transform, opacity, filter',
    onComplete: () => {
      if (token !== animationToken) {
        return
      }

      element.style.visibility = 'hidden'
      isVisible.value = false

      if (props.collapseWhenHidden) {
        renderVisible.value = false
      }

      currentTween = null
    },
  })
}

onMounted(() => {
  const element = elementRef.value

  if (!element) {
    return
  }

  if (!props.active) {
    renderVisible.value = !props.collapseWhenHidden
    gsap.set(element, hiddenEnterVars.value)
    element.style.visibility = 'hidden'
    isVisible.value = false
  } else if (props.appear) {
    renderVisible.value = true
    gsap.set(element, hiddenEnterVars.value)
    element.style.visibility = 'hidden'
    isVisible.value = false
  } else {
    renderVisible.value = true
    gsap.set(element, visibleVars)
    element.style.visibility = 'visible'
    isVisible.value = true
  }

  hasMounted.value = true

  if (props.active && props.appear) {
    void animateIn()
  }
})

watch(
  () => props.active,
  (isActive) => {
    if (!hasMounted.value) {
      return
    }

    if (isActive) {
      void animateIn()
      return
    }

    animateOut()
  },
)

onBeforeUnmount(killTween)
</script>

<style scoped>
.reveal-on-active {
  display: block;
  visibility: hidden;
  will-change: transform, opacity, filter;
}

.reveal-on-active--visible {
  visibility: visible;
}
</style>