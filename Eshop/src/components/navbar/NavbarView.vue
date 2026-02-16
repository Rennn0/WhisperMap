<script setup lang="ts">
import { ref, onMounted, onActivated, onUnmounted, onUpdated, watch, inject, type Ref } from 'vue';
import type { Product, ThemeDropdown, UserInfo } from '../../types';
import { userInfoInjectionKey } from '../../injectionKeys';
import TablerMenuIcon from '../freestyle/TablerMenuIcon.vue';
import TablerPlusIcon from '../freestyle/TablerPlusIcon.vue';
import TablerMoonIcon from '../freestyle/TablerMoonIcon.vue';
import TablerSunIcon from '../freestyle/TablerSunIcon.vue';
import TablerContrastIcon from '../freestyle/TablerContrastIcon.vue';
import TablerCometIcon from '../freestyle/TablerCometIcon.vue';
import SearchBar from './SearchBar.vue';
import { getProducts, logout } from '../../services/http';
import UserMenuDropdown from '../dropdown/UserMenuDropdown.vue';
import ThemeDropdownMenu from '../dropdown/ThemeDropdownMenu.vue';

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
  (e: 'auth'): void;
}>();

const themes: ThemeDropdown[] = [
  { name: 'light', label: 'Light', icon: TablerSunIcon },
  { name: 'dark', label: 'Dark', icon: TablerMoonIcon },
  { name: 'solarized', label: 'Solarized', icon: TablerCometIcon },
  { name: 'highcontrast', label: 'High Contrast', icon: TablerContrastIcon },
];

const query = ref('');
const searchPreviewOpen = ref(false);
const currentTheme = ref('light');
const userInfo = inject<Readonly<Ref<UserInfo>>>(userInfoInjectionKey);

const products = ref<Product[]>([]);

const fetchProducts = debounce(async (q: string) => {
  getProducts({ query: q }).request.then(v => {
    products.value = v?.products?.slice(0, 10) || [];
  })
}, 400);

watch(query, async (newQuery) => {
  const q = newQuery.trim().toLowerCase();
  fetchProducts(q);
});

watch(currentTheme, (theme) => {
  document.documentElement.classList.remove(...themes.map(t => t.name));
  document.documentElement.classList.add(theme);
  localStorage.setItem('theme', theme);
});


const onMenu = () => {
  emit('menu-toggle');
}

const onInput = (value: string) => {
  query.value = value;
  searchPreviewOpen.value = !!value;
}

const onSearch = (value: string) => {
  emit('search', value);
  searchPreviewOpen.value = false;
}

const onProductChosen = (product: Product) => {
  emit('product-chosen', product);
  query.value = '';
  searchPreviewOpen.value = false;
}

const onSearchFocus = () => {
  searchPreviewOpen.value = true;
}

const onSearchBlur = () => {
  searchPreviewOpen.value = false;
}

const onProfile = () => { /*TODO*/ }
const onLogout = () => {
  logout().request.then(() => window.location.reload())
}

//#region lifecycle hooks
onActivated(() => { });
onUpdated(() => { });
onMounted(() => {
  const saved = localStorage.getItem('theme') || 'light';
  currentTheme.value = saved;
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
      <SearchBar :products="products" :isOpen="searchPreviewOpen" :query="query" @input="onInput" @search="onSearch"
        @product-chosen="onProductChosen" @focus="onSearchFocus" @blur="onSearchBlur" />

      <!-- Right side buttons -->
      <div class="relative flex items-center gap-1">
        <!-- Theme -->
        <ThemeDropdownMenu :themes="themes" v-model="currentTheme" />
        <!-- Auth -->
        <UserMenuDropdown :picture="userInfo?.picture" @login="emit('auth')" @profile="onProfile" @logout="onLogout" />
      </div>
    </div>
  </nav>
</template>
