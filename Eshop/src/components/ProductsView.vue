<script setup lang="ts">
import { onActivated, onMounted, onUnmounted, onUpdated, reactive } from 'vue';
import type { Product } from '../types';
import ProductItem from './ProductItem.vue';
import { productData } from '../mock.data';
import { useRouter } from 'vue-router';

const emit = defineEmits<{ (e: 'select', product: Product): void, (e: 'close-detail'): void }>();

const products = reactive<Product[]>(productData);

const router = useRouter();

const onSelect = (product: Product) => {
    product.description = product.description.repeat(50);
    router.push({ name: "product", params: { id: product.id } });
    emit('select', product);
};

//#region lifecycle hooks
onActivated(() => { });
onUpdated(() => { });
onMounted(() => { });
onUnmounted(() => { })
//#endregion
</script>

<template>
    <div>
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            <ProductItem v-for="(item, index) in products" :key="index" :product="item" @select="onSelect" />
        </div>
    </div>
</template>

<style scoped></style>