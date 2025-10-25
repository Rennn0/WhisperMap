<script setup lang="ts" async>
import { ref, onMounted, provide, readonly, onActivated, onUpdated, onUnmounted, computed, watchEffect } from 'vue';
import NavBar from './components/Navbar.vue';
import Sidebar from './components/Sidebar.vue';
import ProductsView from './components/ProductsView.vue';
import OrdersView from './components/OrdersView.vue';
import SettingsView from './components/SettingsView.vue';
import type { Product } from './types';
import { titleInjectionKey } from './injectionKeys';

//#region variables and providers

const title = ref("საჩუქრების ზარდახშა");
const sidebarOpen = ref(false);

const selectedView = ref<'products' | 'orders' | 'settings'>('products');

provide(titleInjectionKey, { title: readonly(title), update: (t: string) => title.value = t });
//#endregion

//#region computed / watchEffect
const viewMap = {
  products: ProductsView,
  orders: OrdersView,
  settings: SettingsView,
} as const;

const currentView = computed(() => viewMap[selectedView.value]);

const currentProps = computed(() => {
  if (selectedView.value === 'products') return {};
  return {};
});

watchEffect(() => {
  switch (selectedView.value) {
    case 'products':
      break;
    case 'orders':
      break;
    case 'settings':
      break;
  }
});
//#endregion

//#region event hooks
const onSearch = (_value: string): void => { }

const onMenuToggle = () => sidebarOpen.value = true

const onProfileClick = (): void => { }

const onSelectProduct = (_product: Product): void => { }

const onOptionSelect = (key: string) => {
  if (key === 'products' || key === 'orders' || key === 'settings') {
    selectedView.value = key;
  }
  sidebarOpen.value = false;
}
//#endregion

//#region lifecycle hooks
onActivated(() => { });
onUpdated(() => { });
onMounted(() => { });
onUnmounted(() => { })
//#endregion

</script>

<template>
  <div class="min-h-screen bg-surface text-text transition-colors duration-300">
    <title>{{ title }}</title>

    <NavBar @search="onSearch" @menu-toggle="onMenuToggle" @profile-click="onProfileClick" />

    <Sidebar :open="sidebarOpen" @close="sidebarOpen = false" @select="onOptionSelect" />

    <main class="max-w-6xl mx-auto p-4">
      <component :is="currentView" v-bind="currentProps" @select="onSelectProduct" />
    </main>
  </div>
</template>