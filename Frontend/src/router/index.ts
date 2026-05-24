import { createRouter, createWebHistory } from 'vue-router'

import LandingScreen from '@/components/LandingScreen.vue'
import InterviewScreen from '@/components/InterviewScreen.vue'
import ProfileScreen from '@/components/ProfileScreen.vue'
import { initAuth, isAuthenticated } from '@/services/auth'

const EmptyRoute = {
  template: '<div />',
}

async function ensureAuthInitialized() {
  try {
    await initAuth()
  } catch (error) {
    console.error('Keycloak initialization failed', error)
  }
}

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: EmptyRoute,
      beforeEnter: async () => {
        await ensureAuthInitialized()

        return isAuthenticated.value
          ? { name: 'profile', hash: '#profile' }
          : { name: 'landing' }
      },
    },
    {
      path: '/landing',
      name: 'landing',
      component: LandingScreen,
    },
    {
      path: '/profile',
      name: 'profile',
      component: ProfileScreen,
      meta: {
        requiresAuth: true,
      },
    },
    {
      path: '/profile/settings',
      redirect: {
        name: 'profile',
        hash: '#settings',
      },
      meta: {
        requiresAuth: true,
      },
    },
    {
      path: '/profile/feed',
      redirect: {
        name: 'profile',
        hash: '#feed',
      },
      meta: {
        requiresAuth: true,
      },
    },
    {
      path: '/interview',
      name: 'interview',
      component: InterviewScreen,
      meta: {
        requiresAuth: true,
      },
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/',
    },
  ],
})

router.beforeEach(async (to) => {
  await ensureAuthInitialized()

  if (to.meta.requiresAuth && !isAuthenticated.value) {
    return { name: 'landing' }
  }
})

export default router
