/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#667eea',
          dark: '#5769cc',
          light: '#8ea1f5',
        },
        secondary: {
          DEFAULT: '#764ba2',
          dark: '#62378e',
          light: '#8a5fb6',
        },
        accent: '#ed64a6',
        success: '#22c55e',
        warning: '#fb923c',
        error: '#ef4444',
        info: '#3b82f6',
      },
      spacing: {
        '15': '3.75rem', // 60px for header height
      },
      width: {
        '16': '4rem',
      },
      height: {
        '16': '4rem',
      },
      transitionProperty: {
        'width': 'width',
      },
      backgroundImage: {
        'gradient-primary': 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        'gradient-success': 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
        'gradient-warning': 'linear-gradient(135deg, #fb923c 0%, #fcd34d 100%)',
        'gradient-info': 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
      },
    },
  },
  plugins: [],
}
