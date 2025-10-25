<script setup lang="ts" async>
import { ref, onMounted, provide, readonly, onActivated, onUpdated, onUnmounted } from 'vue';
import NavBar from './components/Navbar.vue';
import Sidebar from './components/Sidebar.vue';
import ProductsView from './components/ProductsView.vue';
import OrdersView from './components/OrdersView.vue';
import SettingsView from './components/SettingsView.vue';
import { CurrentViewSelection, type Product } from './types';
import { titleInjectionKey } from './injectionKeys';
import { productData } from './mock.data';

//#region variables and providers

const title = ref("საჩუქრების ზარდახშა");
const sidebarOpen = ref(false);
const viewKey = ref(0);
const selectedView = ref<CurrentViewSelection>(CurrentViewSelection.Product);
const displayProduct = ref<Product>();

provide(titleInjectionKey, { title: readonly(title), update: (t: string) => title.value = t });
//#endregion

//#region computed / watchEffect

//#endregion

//#region event hooks
const onSearch = (_value: string): void => {
  const item = productData.find(p => p.description.includes(_value));
  displayProduct.value = item;
}

const onMenuToggle = () => sidebarOpen.value = true

const onProfileClick = (): void => { }

const onSelectProduct = (_product: Product): void => { }

const onOptionSelect = (key: CurrentViewSelection) => {
  viewKey.value++;
  displayProduct.value = undefined;
  selectedView.value = key;
  sidebarOpen.value = false;
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const onCloseDetail = () => {
  displayProduct.value = undefined;
}

const onProductChosen = (product: Product) => {
  displayProduct.value = product;
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

    <NavBar @search="onSearch" @menu-toggle="onMenuToggle" @profile-click="onProfileClick"
      @product-chosen="onProductChosen" />

    <Sidebar :open="sidebarOpen" @close="sidebarOpen = false" @select="onOptionSelect" />

    <main class="max-w-6xl mx-auto p-4">
      <template v-if="selectedView == CurrentViewSelection.Product">
        <ProductsView :key="viewKey" :display-product="displayProduct" @select="onSelectProduct"
          @close-detail="onCloseDetail" />
      </template>
      <template v-else-if="selectedView == CurrentViewSelection.Order">
        <OrdersView />
      </template>
      <template v-else-if="selectedView == CurrentViewSelection.Setting">
        <SettingsView />
      </template>
    </main>
  </div>
</template>