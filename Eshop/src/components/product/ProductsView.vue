<script setup lang="ts">
import { computed, onActivated, onMounted, onUnmounted, onUpdated, ref, watchEffect } from 'vue';
import type { Product } from '../../types';
import { getProducts } from '../../services/content.service';
import { useRouter } from 'vue-router';
import ProductItem from './ProductItem.vue';
import SkeletonProductItem from '../skeletons/SkeletonProductItem.vue';

const router = useRouter();
const emit = defineEmits<{ (e: 'select', product: Product): void }>();

const productsRef = ref<Product[] | null>(null);
// watchEffect(() => );
const products = computed(() => productsRef.value);

const onSelect = (product: Product) => {
    router.push({ name: "product", params: { id: product.id } });
    emit('select', product);
};

//#region lifecycle hooks
onUpdated(() => { });
onMounted(() => {
    getProducts().then(ps => productsRef.value = ps)
});
onUnmounted(() => { })
//#endregion
</script>

<template>
    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
        <template v-if="products">
            <ProductItem v-for="(item, index) in products" :key="index" :product="item" @select="onSelect" />
        </template>
        <template v-else>
            <SkeletonProductItem v-for="(_, i) in Array(12)" :key="i" />
        </template>
    </div>
</template>

<style scoped></style>