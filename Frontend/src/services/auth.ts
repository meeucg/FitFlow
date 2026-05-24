import { ref } from 'vue'
import Keycloak from 'keycloak-js'
import { appSettings } from '@/services/appConfig'

const keycloak = new Keycloak({
  url: appSettings.keycloak.url,
  realm: appSettings.keycloak.realm,
  clientId: appSettings.keycloak.clientId,
})

let initialization: Promise<boolean> | null = null
const authInitTimeoutMs = 2500

export const authReady = ref(false)
export const isAuthenticated = ref(false)

const profileRedirectUri = () => `${window.location.origin}/profile#profile`
const landingRedirectUri = () => `${window.location.origin}/landing`

function syncAuthState(authenticated = Boolean(keycloak.authenticated)) {
  isAuthenticated.value = authenticated
}

export function initAuth() {
  if (!initialization) {
    let timeoutId = 0

    const keycloakInitialization = keycloak.init({
      onLoad: 'check-sso',
      pkceMethod: 'S256',
      checkLoginIframe: false,
      silentCheckSsoRedirectUri: `${window.location.origin}/silent-check-sso.html`,
      silentCheckSsoFallback: false,
    }).then((authenticated) => {
      window.clearTimeout(timeoutId)
      syncAuthState(authenticated)
      authReady.value = true
      return authenticated
    }).catch((error) => {
      window.clearTimeout(timeoutId)
      syncAuthState(false)
      authReady.value = true
      throw error
    })

    const timeoutFallback = new Promise<boolean>((resolve) => {
      timeoutId = window.setTimeout(() => {
        if (!authReady.value) {
          syncAuthState(false)
          authReady.value = true
        }

        resolve(isAuthenticated.value)
      }, authInitTimeoutMs)
    })

    initialization = Promise.race([keycloakInitialization, timeoutFallback])
  }

  return initialization
}

export async function login() {
  await initAuth()
  await keycloak.login({ redirectUri: profileRedirectUri() })
}

export async function logout() {
  await initAuth()
  await keycloak.logout({ redirectUri: landingRedirectUri() })
}

export async function getAccessToken() {
  await initAuth()

  if (!keycloak.authenticated) {
    return null
  }

  await keycloak.updateToken(30)
  syncAuthState()
  return keycloak.token ?? null
}

export { keycloak }
