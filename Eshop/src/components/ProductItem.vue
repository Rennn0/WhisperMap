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
    <article @click="onClick" role="button" :aria-label="`Open ${product.title}`"
        class="group bg-surface border border-transparent hover:border-gray-300 hover:bg-subtle rounded-lg shadow-sm hover:shadow-md transition-all duration-200 p-3 cursor-pointer">
        <div class="overflow-hidden rounded-md">
            <img :src="product.image" :alt="product.title"
                class="w-full h-40 object-cover group-hover:scale-[1.02] transition-transform duration-300" />
        </div>

        <div class="mt-3">
            <h3
                class="text-sm font-medium text-text line-clamp-2 group-hover:text-primary transition-colors duration-200">
                {{ product.title }}
            </h3>
            <p class="text-xs text-text/70 mt-1 line-clamp-2 group-hover:text-text transition-colors duration-200">
                {{ product.description }}
            </p>

            <div class="mt-3 flex items-center justify-between">
                <span v-if="product.price !== undefined" class="text-sm font-semibold text-primary">
                    ${{ product.price.toFixed(2) }}
                </span>
                <span v-else class="text-sm text-text/50">No price</span>

                <span class="text-xs text-text/60">
                    {{ product.seller ?? "Unknown" }}
                </span>
            </div>
        </div>
    </article>
</template>

<style scoped></style>