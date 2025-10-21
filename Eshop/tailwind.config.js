/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  darkMode: "class",
  // theme: {
  //   extend: {
  //     colors: {
  //       primary: {
  //         DEFAULT: '#0052CC',
  //         light: '#4C9AFF',
  //         dark: '#0747A6',
  //       },
  //       surface: {
  //         light: '#FFFFFF',
  //         dark: '#1D2125',
  //       },
  //       text: {
  //         light: '#172B4D',
  //         dark: '#B6C2CF',
  //       },
  //       subtle: {
  //         light: '#F4F5F7',
  //         dark: '#2C333A',
  //       },
  //     },
  //   },
  // },
  theme: {
    extend: {
      colors: {
        primary: 'var(--color-primary)',
        surface: 'var(--color-surface)',
        text: 'var(--color-text)',
        subtle: 'var(--color-subtle)',
      },
    },
  },
  plugins: [],
}