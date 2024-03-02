/** @type {import('tailwindcss').Config} */
const defaultTheme = require('tailwindcss/defaultTheme')
const { colors: defaultColors } = require('tailwindcss/defaultTheme')

export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Lexend', 'Inter', ...defaultTheme.fontFamily.sans],
        display: ['Lexend', ...defaultTheme.fontFamily.sans],
      },
      colors: {
        green: defaultColors.emerald,
        purple: defaultColors.violet,
        yellow: defaultColors.amber,
        pink: defaultColors.fuchsia,
        slate: defaultColors.slate,
        ...defaultColors
      },
    },
  },
  plugins: [],
}

