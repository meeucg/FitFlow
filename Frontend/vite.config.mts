import { fileURLToPath, URL } from 'node:url'
import Vue from '@vitejs/plugin-vue'
import Fonts from 'unplugin-fonts/vite'
import { defineConfig } from 'vite'
import Vuetify, { transformAssetUrls } from 'vite-plugin-vuetify'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    Vue({
      template: { transformAssetUrls },
    }),
    // https://github.com/vuetifyjs/vuetify-loader/tree/master/packages/vite-plugin#readme
    Vuetify({
      autoImport: true,
      styles: {
        configFile: 'src/styles/settings.scss',
      },
    }),
    Fonts({
      custom: {
        families: [
          {
            name: 'Styrene A Web',
            local: 'Styrene A Web',
            src: './src/assets/fonts/styrene-a/StyreneAWeb-*.woff2',
            transform(font) {
              const weights: Record<string, number> = {
                Thin: 100,
                Light: 300,
                Regular: 400,
                Medium: 500,
                Bold: 700,
                Black: 900,
              }

              const weightName = Object.keys(weights).find((weight) =>
                font.basename.includes(weight),
              )

              font.weight = weightName ? weights[weightName] : 400
              font.style = font.basename.includes('Italic') ? 'italic' : 'normal'

              return font
            },
          },
        ],
        display: 'swap',
      },
      fontsource: {
        families: [
          {
            name: 'Roboto',
            weights: [100, 300, 400, 500, 700, 900],
            styles: ['normal', 'italic'],
          },
        ],
      },
    }),
  ],
  define: { 'process.env': {} },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('src', import.meta.url)),
    },
    extensions: [
      '.js',
      '.json',
      '.jsx',
      '.mjs',
      '.ts',
      '.tsx',
      '.vue',
    ],
  },
  server: {
    port: 3000,
    allowedHosts: true,
  },
})
