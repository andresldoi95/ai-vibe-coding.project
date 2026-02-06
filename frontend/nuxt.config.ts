// https://nuxt.com/docs/api/configuration/nuxt-config
import Aura from '@primeuix/themes/aura'

export default defineNuxtConfig({
  devtools: { enabled: true },

  // Disable app manifest to avoid build warnings
  experimental: {
    appManifest: false,
  },

  modules: [
    '@nuxtjs/tailwindcss',
    '@primevue/nuxt-module',
    '@pinia/nuxt',
    '@pinia-plugin-persistedstate/nuxt',
    '@nuxtjs/color-mode',
    '@nuxtjs/i18n',
  ],

  // Components configuration
  components: [
    {
      path: '~/components',
      pathPrefix: false,
    },
  ],

  // TypeScript configuration
  typescript: {
    strict: true,
    typeCheck: false, // Disabled to avoid Volar dependency issues - use 'npm run typecheck' for manual checking
  },

  // Vite configuration
  vite: {
    vue: {
      script: {
        propsDestructure: true,
      },
    },
  },

  // PrimeVue configuration
  primevue: {
    options: {
      ripple: true,
      inputVariant: 'outlined',
      theme: {
        preset: Aura,
        options: {
          prefix: 'p',
          darkModeSelector: '.dark',
          cssLayer: false,
        },
      },
    },
    autoImport: true,
    components: {
      include: '*',
    },
    directives: {
      include: ['Tooltip', 'Ripple', 'StyleClass'],
    },
  },

  // Tailwind CSS configuration
  tailwindcss: {
    cssPath: '~/assets/styles/main.css',
    configPath: 'tailwind.config.ts',
    exposeConfig: false,
    viewer: true,
  },

  // Color mode configuration for dark/light theme
  colorMode: {
    classSuffix: '',
    preference: 'system',
    fallback: 'light',
    storageKey: 'nuxt-color-mode',
  },

  // i18n configuration
  i18n: {
    locales: [
      { code: 'en', language: 'en-US', files: ['en.json'], name: 'English' },
      { code: 'es', language: 'es-ES', files: ['es.json'], name: 'Español' },
      { code: 'fr', language: 'fr-FR', files: ['fr.json'], name: 'Français' },
      { code: 'de', language: 'de-DE', files: ['de.json'], name: 'Deutsch' },
    ],
    defaultLocale: 'en',
    strategy: 'no_prefix',
    detectBrowserLanguage: {
      useCookie: true,
      cookieKey: 'i18n_redirected',
      redirectOn: 'root',
      alwaysRedirect: false,
    },
  },

  // Runtime config
  runtimeConfig: {
    public: {
      apiBase:
        // eslint-disable-next-line node/prefer-global/process
        process.env.NUXT_PUBLIC_API_BASE || 'http://localhost:5000/api/v1',
    },
  },

  // App configuration
  app: {
    head: {
      title: 'SaaS Billing & Inventory',
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        {
          name: 'description',
          content: 'Multi-tenant SaaS Billing and Inventory Management System',
        },
      ],
      link: [{ rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' }],
    },
  },

  css: [
    'primeicons/primeicons.css',
  ],

  compatibilityDate: '2024-11-01',
})
