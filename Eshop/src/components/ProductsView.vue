<script setup lang="ts">
import { reactive, ref } from 'vue';
import type { Product } from '../types';
import ProductItem from './ProductItem.vue';
import ProductDetail from './ProductDetail.vue';
import { productData } from '../mock.data';

const emit = defineEmits<{ (e: 'select', product: Product): void }>();

const products = reactive<Product[]>(productData);

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