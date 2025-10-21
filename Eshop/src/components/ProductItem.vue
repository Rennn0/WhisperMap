<script setup lang="ts">
import { inject, type Ref } from 'vue';
import type { Product } from '../types';
import { titleInjectionKey } from '../injectionKeys';

const emit = defineEmits<{
    (e: 'select', product: Product): void;
}>();

const props = defineProps<{ product: Product }>();
const injected: { title: Readonly<Ref<string>>, update: (t: string) => void } | undefined = inject(titleInjectionKey);

function onClick() {
    emit('select', props.product);
    if (props.product.price) {
        props.product.price++;
        props.product.description = "x"
    }
    console.log(titleInjectionKey);
    injected?.update(props.product.title);
}
</script>

<template>
    <article class="bg-white rounded-lg shadow-sm hover:shadow-md transition p-3 cursor-pointer" @click="onClick"
        role="button" :aria-label="`Open ${product.title}`">
        <div class="overflow-hidden rounded-md">
            <img :src="product.image" :alt="product.title" class="w-full h-40 object-cover" />
        </div>

        <div class="mt-3">
            <h3 class="text-sm font-medium text-gray-900 line-clamp-2">{{ product.title }}</h3>
            <p class="text-xs text-gray-500 mt-1 line-clamp-2">{{ product.description }}</p>
            <div class="mt-3 flex items-center justify-between">
                <span v-if="product.price !== undefined" class="text-sm font-semibold text-indigo-600">
                    ${{ product.price.toFixed(2) }}
                </span>
                <span v-else class="text-sm text-gray-400">No price</span>

                <span class="text-xs text-gray-400">{{ product.seller ?? 'Unknown' }}</span>
            </div>
        </div>
    </article>
</template>


<style scoped></style>