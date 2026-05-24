<template>
  <aside
    class="profile-sidebar"
    :class="{ 'profile-sidebar--expanded': isExpanded }"
    aria-label="Навигация профиля"
  >
    <button class="sidebar-logo" type="button" aria-label="Открыть лендинг" @click="goLanding">
      <img src="../assets/logo.svg" class="sidebar-logo-image" alt="FitFlow logo">
    </button>

    <v-btn
      icon
      class="sidebar-button sidebar-button--expand"
      :class="{ 'sidebar-button--expanded': isExpanded }"
      :aria-label="isExpanded ? 'Свернуть меню' : 'Развернуть меню'"
      :title="isExpanded ? 'Свернуть меню' : 'Развернуть меню'"
      rounded="circle"
      variant="text"
      @click="isExpanded = !isExpanded"
    >
      <img :src="arrowIcon" class="sidebar-icon sidebar-arrow-icon" alt="">
    </v-btn>

    <nav class="sidebar-nav">
      <v-btn
        v-for="item in navItems"
        :key="item.section"
        icon
        class="sidebar-button"
        :class="{ 'sidebar-button--active': activeSection === item.section }"
        :aria-label="item.label"
        :title="item.label"
        rounded="circle"
        variant="text"
        @click="goTo(item.section)"
      >
        <img :src="item.icon" class="sidebar-icon" alt="">
        <span class="sidebar-label">{{ item.label }}</span>
      </v-btn>
    </nav>

    <v-btn
      icon
      class="sidebar-button sidebar-button--logout"
      aria-label="Выйти"
      title="Выйти"
      color="black"
      rounded="circle"
      variant="flat"
      @click="$emit('logout')"
    >
      <img :src="logoutIcon" class="sidebar-icon sidebar-icon--white" alt="">
    </v-btn>
  </aside>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import profileIcon from '@/assets/icons/google-account-circle.svg'
import settingsIcon from '@/assets/icons/google-settings.svg'
import dashboardIcon from '@/assets/icons/google-dashboard.svg'
import logoutIcon from '@/assets/icons/google-logout.svg'
import arrowIcon from '@/assets/icons/google-keyboard-arrow-right.svg'

defineEmits<{
  logout: []
}>()

const router = useRouter()
const route = useRoute()
const isExpanded = ref(false)

const navItems = [
  {
    label: 'Профиль',
    section: 'profile',
    icon: profileIcon,
  },
  {
    label: 'Настройки',
    section: 'settings',
    icon: settingsIcon,
  },
  {
    label: 'Лента',
    section: 'feed',
    icon: dashboardIcon,
  },
]

const activeSection = computed(() => {
  const section = route.hash.replace('#', '')

  return section === 'settings' || section === 'feed'
    ? section
    : 'profile'
})

function goLanding() {
  void router.push({ name: 'landing' })
}

function goTo(section: string) {
  if (activeSection.value === section) {
    return
  }

  void router.push({
    name: 'profile',
    hash: `#${section}`,
  })
}
</script>

<style scoped>
.profile-sidebar {
  position: fixed;
  top: 40px;
  left: 2rem;
  z-index: 20;
  --sidebar-rail-width: calc(var(--header-container-height) - 80px);
  --sidebar-inner-size: calc(var(--sidebar-rail-width) - 1.6rem);
  width: var(--sidebar-rail-width);
  height: calc(100vh - 80px);
  padding: 0.8rem;
  box-sizing: border-box;
  border-radius: 999px;
  background: linear-gradient(
    to bottom in oklch,
    white,
    14%,
    rgba(255, 255, 255, 0.82),
    86%,
    white
  );
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  box-shadow: 0 18px 54px rgba(23, 42, 97, 0.1);
  overflow: hidden;
  transition:
    width 0.24s cubic-bezier(0.22, 1, 0.36, 1),
    box-shadow 0.24s ease;
}

.profile-sidebar--expanded {
  width: 15.2rem;
  border-radius: 2.2rem;
  box-shadow: 0 22px 62px rgba(23, 42, 97, 0.13);
}

.sidebar-logo {
  flex: 0 0 auto;
  width: var(--sidebar-inner-size);
  height: var(--sidebar-inner-size);
  border: 0;
  padding: 0;
  border-radius: 50%;
  background: transparent;
  cursor: pointer;
}

.sidebar-logo-image {
  width: 100%;
  height: 100%;
  display: block;
  border-radius: 50%;
}

.sidebar-nav {
  flex: 1 1 auto;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: flex-start;
  gap: 0.58rem;
  width: 100%;
  min-width: 0;
}

.sidebar-button {
  width: var(--sidebar-inner-size) !important;
  height: var(--sidebar-inner-size) !important;
  min-width: 0 !important;
  justify-content: flex-start;
  border-radius: 999px !important;
  padding: 0 !important;
  color: black;
  transition:
    width 0.24s cubic-bezier(0.22, 1, 0.36, 1),
    background-color 0.18s ease,
    transform 0.18s ease,
    box-shadow 0.18s ease;
}

.profile-sidebar--expanded .sidebar-button {
  width: calc(100% - 0.05rem) !important;
  padding: 0 0.85rem 0 0 !important;
}

.sidebar-button :deep(.v-btn__content) {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: flex-start;
}

.sidebar-button:hover {
  transform: translateY(-0.08rem);
}

.sidebar-button--active {
  background: rgba(0, 0, 0, 0.07) !important;
  box-shadow: none;
}

.sidebar-button--expand {
  flex: 0 0 auto;
  margin-top: 0.7rem;
  background: rgba(0, 0, 0, 0.035) !important;
}

.sidebar-icon {
  flex: 0 0 auto;
  width: 1.4rem;
  height: 1.4rem;
  display: block;
  object-fit: contain;
  opacity: 0.72;
  margin: 0 calc((var(--sidebar-inner-size) - 1.4rem) / 2);
}

.sidebar-button--active .sidebar-icon {
  opacity: 0.46;
  filter: none;
}

.sidebar-icon--white {
  filter: invert(1);
  opacity: 1;
}

.sidebar-arrow-icon {
  transition: transform 0.22s ease;
}

.sidebar-button--expanded .sidebar-arrow-icon {
  transform: rotate(180deg);
}

.sidebar-label {
  min-width: 0;
  margin-left: 0.15rem;
  color: rgba(0, 0, 0, 0.78);
  font-size: 0.95rem;
  font-weight: 400;
  line-height: 1;
  white-space: nowrap;
  opacity: 0;
  transform: translateX(-0.35rem);
  transition:
    opacity 0.16s ease,
    transform 0.16s ease;
}

.profile-sidebar--expanded .sidebar-label {
  opacity: 1;
  transform: translateX(0);
}

.sidebar-button--logout {
  flex: 0 0 auto;
}
</style>
