<script setup lang="ts">
import { reactive, ref } from 'vue';
import type { Product } from '../types';
import ProductItem from './ProductItem.vue';
import ProductDetail from './ProductDetail.vue';

const emit = defineEmits<{ (e: 'select', product: Product): void }>();

const products = reactive<Product[]>(Array.from({ length: 12 }).map((_, i): Product => ({
    id: `${i + 1}`,
    title: `#${i + 1}`,
    description:
        'Short description of the item for sale',
    image: `https://picsum.photos/seed/product-${i + 1}/640/360`,
    price: Math.round((10 + Math.random() * 90) * 100) / 100,
    seller: `Seller ${Math.ceil(Math.random() * 10)}`,
})));

const selectedProduct = ref<Product | null>(null);

const onSelect = (product: Product) => {
    product.description = product.description.repeat(50);
    selectedProduct.value = product;
    emit('select', product);
};

const onCloseDetail = () => {
    selectedProduct.value = null;
};
</script>

<template>
    <div>
        <!-- Grid -->
        <div v-if="!selectedProduct" class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            <ProductItem v-for="(item, index) in products" :key="index" :product="item" @select="onSelect" />
        </div>

        <!-- Detail -->
        <div v-else>
            <ProductDetail :product="selectedProduct" @close="onCloseDetail" />
        </div>
    </div>
</template>

<style scoped></style>