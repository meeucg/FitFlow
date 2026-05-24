# fit-flow-frontend

Vue 3 + Vite frontend for FitFlow, an AI-assisted freelance-order aggregator for designers.

The app includes:

- landing page with animated three-screen presentation;
- Keycloak authentication flow;
- authenticated profile area with sidebar navigation;
- interview flow connected to the backend API;
- interview conclusion/profile summary UI;
- Vuetify-based components and custom Styrene font assets.

## Stack

- Vue 3
- Vite
- TypeScript
- Vuetify
- Vue Router
- Keycloak JS
- vue3-perfect-scrollbar
- GSAP

## Requirements

- Node.js
- npm
- running Keycloak instance for authentication
- running FitFlow API Gateway for profile/interview API calls

## Installation

```bash
npm install
```

## Development

```bash
npm run dev
```

The dev script starts Vite on:

```text
http://127.0.0.1:5173
```

`vite.config.mts` also sets `server.allowedHosts: true` so temporary localhost tunnel domains can reach the dev server.

## App Settings

Frontend runtime configuration lives in `public/appsettings.js` and is copied to the built image.

```js
window.__FITFLOW_APPSETTINGS__ = {
  apiBaseUrl: 'http://localhost:5266',
  keycloak: {
    url: 'http://localhost:8080',
    realm: 'fitflow',
    clientId: 'fitflow-spa',
  },
}
```

## Routes

- `/` - redirects to `/profile#profile` when authenticated, otherwise `/landing`
- `/landing` - public landing page
- `/profile#profile` - authenticated profile section
- `/profile#settings` - authenticated settings placeholder
- `/profile#feed` - authenticated feed section
- `/interview` - authenticated interview flow

## Auth Notes

Keycloak is configured in `src/services/auth.ts`.

Login redirects to:

```text
/profile#profile
```

Logout redirects to:

```text
/landing
```

Silent SSO uses:

```text
public/silent-check-sso.html
```

## Backend API

The interview API layer lives in `src/services/interviewApi.ts`.

Current endpoints:

- `GET /me`
- `GET /my-interview`
- `POST /my-interview/answers`

Requests use the current Keycloak access token as a Bearer token.

## Scripts

```bash
npm run dev
npm run build
npm run preview
npm run build-only
npm run type-check
```

## Build

```bash
npm run build
```

The production output is written to `dist/`.
