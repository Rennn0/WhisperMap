<script setup lang="ts">
import { ref, onMounted, computed, onActivated, onUnmounted, onUpdated } from 'vue';
import { Bars3Icon, MagnifyingGlassIcon, SunIcon, MoonIcon, AdjustmentsHorizontalIcon, BoltIcon } from '@heroicons/vue/24/outline';
import type { Product, ThemeDropdown } from '../types';
import { productData } from '../mock.data';

const emit = defineEmits<{
  (e: 'menu-toggle'): void;
  (e: 'search', value: string): void;
  (e: 'product-chosen', value: Product): void;
  (e: 'input', value: string): void;
}>();

const themes: ThemeDropdown[] = [
  { name: 'light', label: 'Light', icon: SunIcon },
  { name: 'dark', label: 'Dark', icon: MoonIcon },
  { name: 'solarized', label: 'Solarized', icon: AdjustmentsHorizontalIcon },
  { name: 'highcontrast', label: 'High Contrast', icon: BoltIcon },
];

const query = ref('');
const dropdownOpen = ref(false);
const searchPreviewOpen = ref(false);
const currentTheme = ref('light');
const searchInput = ref<HTMLInputElement | null>(null);


const filteredProducts = computed(() => {
  const q = query.value.trim().toLowerCase();
  if (!q) return productData.slice(0, 3);
  return productData.filter(
    (p) =>
      p.title.toLowerCase().includes(q) ||
      p.description.toLowerCase().includes(q)
  ).slice(0, 5);
});

const currentThemeIcon = computed(() => {
  const t = themes.find((t) => t.name === currentTheme.value);
  return t ? t.icon : SunIcon;
});

const toggleDropdown = (val: boolean | null = null) => {
  dropdownOpen.value = val ?? !dropdownOpen.value;
}

const selectTheme = (theme: ThemeDropdown) => {
  dropdownOpen.value = false;
  document.documentElement.classList.remove(...themes.map((t) => t.name));
  document.documentElement.classList.add(theme.name);
  localStorage.setItem('theme', theme.name);
  currentTheme.value = theme.name;
}

const onMenu = () => {
  emit('menu-toggle');
}

const onInput = () => {
  emit('input', query.value);
  searchPreviewOpen.value = !!query.value;
}

const onEnter = () => {
  emit('search', query.value);
  searchPreviewOpen.value = false;
}

const onProductClick = (product: Product) => {
  emit('product-chosen', product);
  query.value = '';
  searchPreviewOpen.value = false;
  searchInput.value?.blur();
}

const closeSearchPreviewLater = () => {
  setTimeout(() => {
    searchPreviewOpen.value = false;
  }, 100);
}

//#region lifecycle hooks
onActivated(() => { });
onUpdated(() => { });
onMounted(() => {
  const saved = localStorage.getItem('theme') || 'light';
  currentTheme.value = saved;
  document.documentElement.classList.add(saved);
});
onUnmounted(() => { })
//#endregion
</script>

<template>
  <nav
    class="sticky top-0 z-50 w-full backdrop-blur-md bg-surface border-b border-subtle transition-colors duration-300">
    <div class="max-w-5xl h-10 md:h-12 mx-auto px-3 py-2 flex items-center gap-3 relative">
      <!-- Menu Button -->
      <button @click="onMenu"
        class="flex items-center justify-center w-10 h-10 rounded-md hover:bg-subtle transition-colors duration-200"
        aria-label="Toggle menu">
        <Bars3Icon class="w-6 h-6 text-text" />
      </button>

      <!-- Search -->
      <div class="flex-1 flex justify-center relative">
        <div class="w-full max-w-xl relative">
          <span class="absolute inset-y-0 left-3 flex items-center pointer-events-none">
            <MagnifyingGlassIcon class="w-5 h-5 text-text/60" />
          </span>

          <input v-model="query" ref="searchInput" @input="onInput" @keyup.enter="onEnter"
            @focus="searchPreviewOpen = true" @blur="closeSearchPreviewLater" type="search" placeholder="Search..."
            class="w-full pl-10 pr-4 py-1 md:py-2 rounded-full border border-gray-300/40 bg-surface text-text shadow-sm focus:outline-none focus:ring-1 focus:ring-primary transition-colors duration-300 text-base md:text-sm"
            aria-label="Search" />

          <!-- Search Preview -->
          <transition enter-active-class="transition duration-150 ease-out" enter-from-class="opacity-0 translate-y-1"
            enter-to-class="opacity-100 translate-y-0" leave-active-class="transition duration-100 ease-in"
            leave-from-class="opacity-100" leave-to-class="opacity-0 translate-y-1">
            <div v-if="searchPreviewOpen && filteredProducts.length" class="absolute mt-3 left-1/2 transform -translate-x-1/2
         bg-surface border border-gray-300/40 shadow-xl rounded-2xl overflow-hidden z-50
         max-h-[70vh] overflow-y-auto
         w-[90vw] md:w-full md:max-w-xl
         p-2 md:p-0
         ring-1 ring-primary/10">
              <ul class="divide-y divide-gray-200/50">
                <li v-for="p in filteredProducts" :key="p.id" @mousedown.prevent="onProductClick(p)"
                  class="px-4 py-4 md:py-2 hover:bg-subtle cursor-pointer transition-colors duration-150 flex items-center gap-4 active:scale-[0.98]">
                  <img :src="p.image || '/placeholder.png'"
                    class="w-16 h-16 md:w-10 md:h-10 rounded-lg object-cover flex-shrink-0" alt="Product image" />
                  <div class="flex flex-col">
                    <span class="text-base md:text-sm font-semibold text-text">{{ p.title }}</span>
                    <span class="text-sm md:text-xs text-text/70 truncate">{{ p.description }}</span>
                  </div>
                </li>
              </ul>
            </div>

          </transition>
        </div>
      </div>

      <!-- Theme Dropdown -->
      <div class="relative ml-3">
        <button @click="toggleDropdown()"
          class="flex items-center justify-center w-10 h-10 rounded-md hover:bg-subtle transition-colors duration-200"
          aria-label="Select theme">
          <component :is="currentThemeIcon" class="w-6 h-6" />
        </button>

        <transition enter-active-class="transition duration-150 ease-out" enter-from-class="opacity-0 scale-95"
          enter-to-class="opacity-100 scale-100" leave-active-class="transition duration-50 ease-in"
          leave-from-class="opacity-100 scale-100" leave-to-class="opacity-0 scale-95">
          <div v-if="dropdownOpen" @mouseleave="toggleDropdown(false)" @blur="toggleDropdown(false)"
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


postgresql://luka:Xju4eqzdFKANB09dar4dgUv3iGezTiVv@dpg-d415cle3jp1c73ckmeu0-a.frankfurt-postgres.render.com/render_zyab