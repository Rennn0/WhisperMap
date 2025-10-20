<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import NavBar from './components/Navbar.vue';
import ProductItem from './components/ProductItem.vue';
import type { Product } from './types';

const lastSearch = ref('');
const menuOpen = ref(false);
const profileOpen = ref(false);
const selectedProduct = ref<string | null>(null);
const products = reactive<Product[]>([]);
const classRef1 = ref('bg-red-200');
const classRef2 = ref('bg-yellow-200');

function onSearch(value: string) {
  lastSearch.value = value;
  console.log('search:', value);
}

function onMenuToggle() {
  menuOpen.value = !menuOpen.value;
  console.log('menu toggled:', menuOpen.value);
}

function onProfileClick() {
  profileOpen.value = !profileOpen.value;
  console.log('profile clicked:', profileOpen.value);
}

function onSelectProduct(product: Product) {
  selectedProduct.value = product.id;
  // products.splice(products.findIndex(p => p.id == product.id), 1);
  console.log('product selected:', product.id);
  const tempVal = classRef1.value;
  classRef1.value = classRef2.value;
  classRef2.value = tempVal;
}


onMounted(async () => {
  await new Promise<void>((res) => {
    setTimeout(() => {
      res();
      console.log("MOUNTED");
      products.splice(0, products.length, ...Array.from({ length: 12 }).map((_, i): Product => ({
        id: `product-${i + 1}`,
        title: `#${i + 1}`,
        description:
          'Short description of the item for sale. This is mock data to demonstrate the item card layout in a responsive grid.',
        image: `https://picsum.photos/seed/product-${i + 1}/640/360`,

        price: Math.round((10 + Math.random() * 90) * 100) / 100,
        seller: `Seller ${Math.ceil(Math.random() * 10)}`,
      } as Product)))
    }, 2000);

  });
});

</script>

<template>
  <NavBar @search="onSearch" @menu-toggle="onMenuToggle" @profile-click="onProfileClick" />

  <main class="max-w-6xl mx-auto p-4">
    <section>
      <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
        <ProductItem v-for="(item, index) in products" :class="index % 2 == 0 ? classRef1 : classRef2" :key="item.id"
          :product="item" @select="onSelectProduct" />
      </div>
    </section>
  </main>
</template>