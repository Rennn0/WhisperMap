<script setup lang="ts">
import { ref, onMounted, computed, onActivated, onUnmounted, onUpdated, watch, toValue, inject, type Ref } from 'vue';
import type { Product, ThemeDropdown, UserInfo } from '../../types';
import { userInfoInjectionKey } from '../../injectionKeys';
import TablerMenuIcon from '../freestyle/TablerMenuIcon.vue';
import TablerPlusIcon from '../freestyle/TablerPlusIcon.vue';
import TablerSearchIcon from '../freestyle/TablerSearchIcon.vue';
import TablerMoonIcon from '../freestyle/TablerMoonIcon.vue';
import TablerSunIcon from '../freestyle/TablerSunIcon.vue';
import TablerContrastIcon from '../freestyle/TablerContrastIcon.vue';
import TablerCometIcon from '../freestyle/TablerCometIcon.vue';
import { getProducts } from '../../services/content.service';

/** Lightweight debounce implementation to avoid requiring lodash.debounce and its types */
function debounce<T extends (...args: any[]) => any>(fn: T, wait = 0) {
  let timeout: ReturnType<typeof setTimeout> | null = null;
  return function (this: any, ...args: Parameters<T>) {
    if (timeout) clearTimeout(timeout);
    timeout = setTimeout(() => {
      timeout = null;
      fn.apply(this, args);
    }, wait);
  };
}


const emit = defineEmits<{
  (e: 'menu-toggle'): void;
  (e: 'search', value: string): void;
  (e: 'product-chosen', value: Product): void;
  (e: 'input', value: string): void;
  (e: 'upload'): void;
}>();

const themes: ThemeDropdown[] = [
  { name: 'light', label: 'Light', icon: TablerSunIcon },
  { name: 'dark', label: 'Dark', icon: TablerMoonIcon },
  { name: 'solarized', label: 'Solarized', icon: TablerCometIcon },
  { name: 'highcontrast', label: 'High Contrast', icon: TablerContrastIcon },
];

const query = ref('');
const dropdownOpen = ref(false);
const searchPreviewOpen = ref(false);
const currentTheme = ref('light');
const searchInput = ref<HTMLInputElement | null>(null);
const userInfo = inject<Readonly<Ref<UserInfo>>>(userInfoInjectionKey);

const products = ref<Product[]>([]);

const fetchProducts = debounce(async (q: string) => {
  try {
    const ps = await getProducts(q);
    products.value = ps?.slice(0, 10) || [];
  } catch (error) {
    console.error('Failed to fetch products:', error);
    products.value = [];
  }
}, 400);

watch(query, async (newQuery) => {
  const q = newQuery.trim().toLowerCase();
  // if (q.length > 0)
  fetchProducts(q);
}, { immediate: true });

const filteredProducts = computed(() => toValue(products));

const currentThemeIcon = computed(() => {
  const t = themes.find((t) => t.name === currentTheme.value);
  return t ? t.icon : TablerSunIcon;
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
  <nav class="sticky top-0 z-50 w-full backdrop-blur-md bg-surface border-b border-subtle transition-all duration-300">
    <div class="max-w-5xl h-10 md:h-12 mx-auto px-3 py-2 flex items-center justify-between gap-3 relative">

      <!-- Left side buttons -->
      <div class="flex items-center gap-1">
        <!-- Menu -->
        <button @click="onMenu"
          class="flex items-center justify-center w-10 h-10 rounded-md hover:bg-subtle transition-colors duration-200"
          aria-label="Toggle menu">
          <TablerMenuIcon class="w-6 h-6 " />
        </button>

        <!-- Upload -->
        <button v-if="userInfo?.can_upload" @click="emit('upload')"
          class="flex items-center justify-center w-10 h-10 rounded-md hover:bg-subtle transition-colors duration-200"
          aria-label="Upload product">
          <TablerPlusIcon class="w-6 h-6" />
        </button>
      </div>

      <!-- Search (center) -->
      <div class="flex-1 flex justify-center">
        <div class="w-full max-w-2xl relative px-2 md:px-0">
          <span class="absolute inset-y-0 left-4 flex items-center pointer-events-none">
            <TablerSearchIcon class="w-5 h-5" />
          </span>

          <input id="nv1" v-model="query" ref="searchInput" @input="onInput" @keyup.enter="onEnter"
            @focus="searchPreviewOpen = true" @blur="closeSearchPreviewLater" type="search"
            :placeholder="$t('nav.search')"
            class="w-full pl-10 pr-4 py-1 md:py-2 rounded-full border   bg-surface text-text outline-none md:text-sm" />

          <!-- preview box -->
          <div v-if="searchPreviewOpen && filteredProducts.length"
            class="absolute left-0 right-0 mt-2 bg-surface border border-subtle shadow-lg rounded-xl z-50 overflow-y-auto max-h-[70vh] md:max-w-2xl mx-auto">
            <ul>
              <li v-for="p in filteredProducts" :key="p.id" @mousedown.prevent="onProductClick(p)"
                class="flex items-center justify-between gap-4 px-4 py-3 hover:bg-subtle cursor-pointer">
                <div class="flex items-center gap-4 min-w-0">
                  <img :src="p.preview_img || '/placeholder.png'"
                    class="w-14 h-14 rounded-lg object-cover flex-shrink-0" alt="Product image" />
                  <div class="flex flex-col overflow-hidden">
                    <span class="font-semibold text-text">{{ p.title }}</span>
                    <span class="text-sm text-text/70 truncate">{{ p.description }}</span>
                  </div>
                </div>
                <span class="text-sm font-semibold text-text whitespace-nowrap ml-2">
                  {{ p.price ? '$' + p.price.toFixed(2) : '' }}
                </span>
              </li>
            </ul>
          </div>
        </div>
      </div>


      <!-- Right side buttons -->
      <div class="relative flex items-center">
        <button @click="toggleDropdown()"
          class="flex items-center justify-center w-10 h-10 rounded-md hover:bg-subtle transition-colors duration-200"
          aria-label="Select theme">
          <component :is="currentThemeIcon" class="w-6 h-6" />
        </button>

        <transition enter-active-class="transition duration-150 ease-out" enter-from-class="opacity-0 scale-95"
          enter-to-class="opacity-100 scale-100" leave-active-class="transition duration-50 ease-in"
          leave-from-class="opacity-100 scale-100" leave-to-class="opacity-0 scale-95">
          <div v-if="dropdownOpen" @mouseleave="toggleDropdown(false)"
            class="absolute right-0 top-full mt-1 w-48 bg-surface border border-gray-300/40 shadow-lg rounded-lg py-1 z-50 pointer-events-auto">
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
