/// <reference types="vite/client" />
/// <reference types="vite-plugin-vue-layouts-next/client" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue'

  const component: DefineComponent<{}, {}, any>
  export default component
}

interface Window {
  __FITFLOW_APPSETTINGS__?: import('@/services/appConfig').FrontendAppSettings
}
