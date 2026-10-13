/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        brand: {
          dark: '#0b0e14',
          surface: '#151a23',
          card: '#1c2331',
          accent: '#e53935',
          gold: '#f59e0b',
          cyan: '#06b6d4',
          muted: '#94a3b8'
        }
      }
    },
  },
  plugins: [],
};
