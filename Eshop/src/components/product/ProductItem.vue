<script setup lang="ts">
import { onActivated, onMounted, onUnmounted, onUpdated } from 'vue';
import type { Product } from '../../types';

const emits = defineEmits<{
    (e: 'select', product: Product): void;
}>();

defineProps<{ product: Product }>();

//#region lifecycle hooks
onActivated(() => { });
onUpdated(() => { });
onMounted(() => { });
onUnmounted(() => { })
//#endregion
</script>

<template>
    <article @click="emits('select', product)" role="button" :aria-label="`Open ${product.title}`"
        class="group bg-surface border border-transparent hover:border-gray-300 hover:bg-subtle rounded-lg shadow-sm hover:shadow-md transition-all duration-200 p-4 cursor-pointer flex flex-col w-full sm:w-auto">

        <div class="overflow-hidden rounded-md">
            <img :src="product.image" :alt="product.title"
                class="w-full h-48 sm:h-40 object-cover group-hover:scale-[1.02] transition-transform duration-300" />
        </div>

        <div class="mt-4 flex-1 flex flex-col">
            <h3
                class="text-base sm:text-sm font-medium text-text line-clamp-2 group-hover:text-primary transition-colors duration-200">
                {{ product.title }}
            </h3>

            <p
                class="text-sm sm:text-xs text-text/70 mt-2 line-clamp-3 group-hover:text-text transition-colors duration-200 flex-1">
                {{ product.description }}
            </p>

            <div class="mt-3 flex items-center justify-between text-sm">
                <span v-if="product.price !== undefined" class="font-semibold text-primary">
                    ${{ product.price.toFixed(2) }}
                </span>
                <span v-else class="text-text/50">No price</span>

                <span class="text-xs text-text/60">
                    {{ product.seller ?? "Unknown" }}
                </span>
            </div>
        </div>
    </article>
</template>
