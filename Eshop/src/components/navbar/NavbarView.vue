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
import { eventStream } from '../../services/stream';
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
const onlineUsers = ref(0);
const isUsersStreamHealthy = ref(false);

const products = ref<Product[]>([]);

const fetchProducts = debounce(async (q: string) => {
  getProducts({ query: q }).request.then(v => {
    products.value = v?.products?.slice(0, 10) || [];
  });
}, 400);

let usersSource: EventSource | null = null;

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
};

const onInput = (value: string) => {
  query.value = value;
  searchPreviewOpen.value = !!value;
};

const onSearch = (value: string) => {
  emit('search', value);
  searchPreviewOpen.value = false;
};

const onProductChosen = (product: Product) => {
  emit('product-chosen', product);
  query.value = '';
  searchPreviewOpen.value = false;
};

const onSearchFocus = () => {
  searchPreviewOpen.value = true;
};

const onSearchBlur = () => {
  searchPreviewOpen.value = false;
};

const onProfile = () => { /* TODO */ };

const onLogout = () => {
  logout().request.then(() => window.location.reload());
};

//#region lifecycle hooks
onActivated(() => { });
onUpdated(() => { });

onMounted(() => {
  const saved = localStorage.getItem('theme') || 'light';
  currentTheme.value = saved;

  usersSource = eventStream({
    url: '/realtime/stream/u',
    callback: (e) => {
      const ev = e as MessageEvent<string>;
      const parts = ev.data.split(';');
      onlineUsers.value = Number(parts[0]);
      isUsersStreamHealthy.value = true;
    },
    onError: () => {
      isUsersStreamHealthy.value = false;
    },
    streamId: userInfo?.value.uid,
  });
});

onUnmounted(() => {
  usersSource?.close();
});
//#endregion
</script>

<template>
  <nav class="sticky top-0 z-50 w-full border-b border-subtle bg-surface/95 backdrop-blur-md">
    <div class="mx-auto flex min-h-[4rem] max-w-6xl items-center gap-3 px-3 py-3 sm:px-4">
      <div class="flex items-center gap-1 shrink-0">
        <button type="button" aria-label="Toggle menu"
          class="flex h-10 w-10 items-center justify-center rounded-xl text-text transition-colors duration-200 hover:bg-hover"
          @click="onMenu">
          <TablerMenuIcon class="h-6 w-6" />
        </button>

        <button v-if="userInfo?.can_upload" type="button" aria-label="Upload product"
          class="flex h-10 w-10 items-center justify-center rounded-xl text-text transition-colors duration-200 hover:bg-hover"
          @click="emit('upload')">
          <TablerPlusIcon class="h-6 w-6" />
        </button>
      </div>

      <div class="min-w-0 flex-1">
        <SearchBar :products="products" :isOpen="searchPreviewOpen" :query="query" @input="onInput" @search="onSearch"
          @product-chosen="onProductChosen" @focus="onSearchFocus" @blur="onSearchBlur" />
      </div>

      <div class="flex items-center gap-2 shrink-0">
        <div :title="'online'"
          class="flex items-center gap-2 rounded-full border px-2.5 py-1.5 text-[11px] md:text-xs whitespace-nowrap transition-colors duration-200"
          :class="isUsersStreamHealthy
            ? 'border-primary/20 bg-subtle text-text'
            : 'border-subtle bg-subtle text-text/60'">
          <span class="relative flex h-2.5 w-2.5 shrink-0">
            <span v-if="isUsersStreamHealthy"
              class="absolute inline-flex h-full w-full rounded-full bg-primary opacity-60 animate-ping" />
            <span class="relative inline-flex h-2.5 w-2.5 rounded-full"
              :class="isUsersStreamHealthy ? 'bg-primary' : 'bg-text/40'" />
          </span>

          <span class="font-semibold tabular-nums">
            {{ onlineUsers }}
          </span>
        </div>

        <ThemeDropdownMenu :themes="themes" v-model="currentTheme" />

        <UserMenuDropdown :picture="userInfo?.picture" @login="emit('auth')" @profile="onProfile" @logout="onLogout" />
      </div>
    </div>
  </nav>
</template>