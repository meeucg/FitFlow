export type FrontendAppSettings = {
  apiBaseUrl?: string
  keycloak?: {
    url?: string
    realm?: string
    clientId?: string
  }
}

const configuredSettings = window.__FITFLOW_APPSETTINGS__ ?? {}

export const appSettings = {
  apiBaseUrl: configuredSettings.apiBaseUrl ?? 'http://localhost:5266',
  keycloak: {
    url: configuredSettings.keycloak?.url ?? 'http://localhost:8080',
    realm: configuredSettings.keycloak?.realm ?? 'fitflow',
    clientId: configuredSettings.keycloak?.clientId ?? 'fitflow-spa',
  },
}
