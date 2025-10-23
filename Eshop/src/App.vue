<script setup lang="ts" async>
import { ref, onMounted, reactive, provide, readonly, onActivated, onUpdated, onUnmounted } from 'vue';
import NavBar from './components/Navbar.vue';
import type { Product } from './types';
import { titleInjectionKey } from './injectionKeys';
import ProductItem from './components/ProductItem.vue';
//#region variables and providers
const products = reactive<Product[]>([]);
const title = ref("this is title");

provide(titleInjectionKey, { title: readonly(title), update: (t: string) => title.value = t });
//#endregion
//#region private methods

//#endregion
//#region event hooks
function onSearch(_value: string) { }

function onMenuToggle() { }

function onProfileClick() { }

function onSelectProduct(_product: Product) { }
//#endregion
//#region lifecycle hooks
onActivated(() => console.log("onactivated"));
onUpdated(() => console.log("update"));
onMounted(() =>
  products.splice(0, products.length, ...Array.from({ length: 12 }).map((_, i): Product => ({
    id: `product-${i + 1}`,
    title: `#${i + 1}`,
    description:
      'Short description of the item for sale',
    image: `https://picsum.photos/seed/product-${i + 1}/640/360`,
    price: Math.round((10 + Math.random() * 90) * 100) / 100,
    seller: `Seller ${Math.ceil(Math.random() * 10)}`,
  })))
);
onUnmounted(() => console.log("unmoun"))
//#endregion

</script>
<template>
  <div class="min-h-screen bg-surface text-text transition-colors duration-300">
    <title>{{ title }}</title>

    <NavBar @search="onSearch" @menu-toggle="onMenuToggle" @profile-click="onProfileClick" />

    <main class="max-w-6xl mx-auto p-4">
      <section>
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
          <ProductItem v-for="(item, index) in products" :key="index" :product="item" @select="onSelectProduct">
          </ProductItem>
        </div>
      </section>
    </main>
  </div>
</template>