<script setup lang="ts">
import { computed, onActivated, onMounted, onUnmounted, onUpdated, reactive, ref, watchEffect } from 'vue';
import type { Product } from '../types';
import ProductItem from './ProductItem.vue';
import { getProducts } from '../mock.data';
import { useRouter } from 'vue-router';
import SkeletonProductItem from './SkeletonProductItem.vue';

const router = useRouter();
const emit = defineEmits<{ (e: 'select', product: Product): void }>();

const productsRef = ref<Product[] | null>(null);
watchEffect(() => getProducts().then(ps => productsRef.value = ps));
const products = computed(() => productsRef ? productsRef.value : []);

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
    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
        <template v-if="products">
            <ProductItem v-for="(item, index) in products" :key="index" :product="item" @select="onSelect" />
        </template>
        <template v-else>
            <SkeletonProductItem v-for="_ in Array(12)" />
        </template>
    </div>
</template>

<style scoped></style>