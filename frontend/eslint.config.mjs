import antfu from '@antfu/eslint-config'

export default antfu({
  typescript: true,
  vue: true,
  markdown: false,

  ignores: [
    '**/*.md',
  ],

  rules: {
    'no-console': 'warn',
    'vue/multi-word-component-names': 'off',
    '@typescript-eslint/no-explicit-any': 'warn',
    'vue/attribute-hyphenation': 'off', // Allow camelCase props (required for PrimeVue v4)
  },
})
