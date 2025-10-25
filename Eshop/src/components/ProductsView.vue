<script setup lang="ts">
import { computed, onActivated, onMounted, onUnmounted, onUpdated, reactive, ref, toValue, type Ref } from 'vue';
import type { Product } from '../types';
import ProductItem from './ProductItem.vue';
import ProductDetail from './ProductDetail.vue';
import { productData } from '../mock.data';

const props = defineProps<{
    displayProduct?: Product
}>();
const emit = defineEmits<{ (e: 'select', product: Product): void, (e: 'close-detail'): void }>();

const products = reactive<Product[]>(productData);

const selectedProduct = ref<Product | null>(null);

const displayProductDetail = computed(() => {
    if (props.displayProduct) return toValue(props.displayProduct);

    if (selectedProduct) return selectedProduct.value;

    return null;
})

const onSelect = (product: Product) => {
    product.description = product.description.repeat(50);
    selectedProduct.value = product;
    emit('select', product);
};

const onCloseDetail = () => {
    selectedProduct.value = null;
    emit('close-detail');
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
        <div v-if="!displayProductDetail" class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            <ProductItem v-for="(item, index) in products" :key="index" :product="item" @select="onSelect" />
        </div>

        <div v-else>
            <ProductDetail :product="displayProductDetail" @close="onCloseDetail" />
        </div>
    </div>
</template>

<style scoped></style>