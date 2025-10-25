<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { Bars3Icon, MagnifyingGlassIcon, SunIcon, MoonIcon, AdjustmentsHorizontalIcon, BoltIcon } from '@heroicons/vue/24/outline';

const emit = defineEmits<{
  (e: 'menu-toggle'): void;
  (e: 'search', value: string): void;
  (e: 'input', value: string): void;
}>();

type ThemeDropdown = { name: string, label: string, icon: any };
const themes: ThemeDropdown[] = [
  { name: "light", label: "Light", icon: SunIcon },
  { name: "dark", label: "Dark", icon: MoonIcon },
  { name: "solarized", label: "Solarized", icon: AdjustmentsHorizontalIcon },
  { name: "highcontrast", label: "High Contrast", icon: BoltIcon },
];

const query = ref("");
const dropdownOpen = ref(false);
const currentTheme = ref("light");

onMounted(() => {
  const saved = localStorage.getItem("theme") || "light";
  currentTheme.value = saved;
  document.documentElement.classList.add(saved);
});

function toggleDropdown(val: boolean | null = null) {
  dropdownOpen.value = val ?? !dropdownOpen.value;
}

function selectTheme(theme: ThemeDropdown) {
  dropdownOpen.value = false;
  document.documentElement.classList.remove(
    ...themes.map((t) => t.name)
  );
  document.documentElement.classList.add(theme.name);
  localStorage.setItem("theme", theme.name);
  currentTheme.value = theme.name;
}

const currentThemeIcon = computed(() => {
  const t = themes.find((t) => t.name === currentTheme.value);
  return t ? t.icon : SunIcon;
});

const onMenu = () => emit('menu-toggle')
const onInput = () => emit('input', query.value)
const onEnter = () => emit('search', query.value)

</script>

<template>
  <!-- add stacking context so navbar and its dropdown stay above the page/detail -->
  <nav class="relative z-50 w-full backdrop-blur-md bg-surface border-b border-subtle transition-colors duration-300">
    <div class="max-w-5xl mx-auto px-3 py-2 flex items-center gap-3 relative">
      <!-- Menu Button -->
      <button @click="onMenu()"
        class="flex items-center justify-center w-10 h-10 rounded-md hover:bg-subtle transition-colors duration-200"
        aria-label="Toggle menu">
        <Bars3Icon class="w-6 h-6 text-text" />
      </button>

      <!-- Search -->
      <div class="flex-1 flex justify-center">
        <div class="w-full max-w-xl">
          <div class="relative">
            <span class="absolute inset-y-0 left-3 flex items-center pointer-events-none">
              <MagnifyingGlassIcon class="w-5 h-5 text-text/60" />
            </span>
            <input v-model="query" @input="onInput" @keyup.enter="onEnter" type="search" placeholder="Search..."
              class="w-full pl-10 pr-4 py-2 rounded-full border border-gray-300/40 bg-surface text-text shadow-sm focus:outline-none focus:ring-1 focus:ring-primary transition-colors duration-300"
              aria-label="Search" />
          </div>
        </div>
      </div>

      <!-- Theme Dropdown -->
      <div class="relative ml-3">
        <button @click="toggleDropdown()"
          class="flex items-center justify-center w-10 h-10 rounded-md hover:bg-subtle transition-colors duration-200"
          aria-label="Select theme">
          <component :is="currentThemeIcon" class="w-6 h-6" />
        </button>

        <!-- Dropdown Menu -->
        <transition enter-active-class="transition duration-150 ease-out" enter-from-class="opacity-0 scale-95"
          enter-to-class="opacity-100 scale-100" leave-active-class="transition duration-50 ease-in"
          leave-from-class="opacity-100 scale-100" leave-to-class="opacity-0 scale-95">
          <div v-if="dropdownOpen" @mouseleave="toggleDropdown(false)"
            class="absolute right-0 mt-2 w-48 bg-surface border border-gray-300/40 shadow-lg rounded-lg py-1 z-60 pointer-events-auto">
            <div v-for="theme in themes" :key="theme.name" @click="selectTheme(theme)"
              class="flex items-center gap-3 px-3 py-2 hover:bg-subtle cursor-pointer transition-colors duration-150">
              <component :is="theme.icon" class="w-5 h-5" />
              <span class="text-sm">{{ theme.label }}</span>
            </div>
          </div>
        </transition>
      </div>
    </div>
  </nav>
</template>
<style scoped></style>