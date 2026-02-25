/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        primary: 'var(--color-primary)',
        surface: 'var(--color-surface)',
        text: 'var(--color-text)',
        subtle: 'var(--color-subtle)',
        hover: 'var(--color-hover)',
        danger: {
          bg: 'var(--color-danger-bg)',
          text: 'var(--color-danger-text)'
        }
      },
    },
  },
  plugins: [],
}