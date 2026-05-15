<script setup lang="ts">
import type { Product } from '../../types';

const emits = defineEmits<{
    (e: 'select', product: Product): void;
}>();

const props = defineProps<{ product: Product }>();
</script>

<template>
    <article role="button" tabindex="0" :aria-label="`Open ${props.product.title}`"
        class="group flex h-full cursor-pointer flex-col overflow-hidden rounded-2xl border border-subtle bg-surface transition-all duration-200 hover:-translate-y-0.5 hover:shadow-md md:min-h-80"
        @click="emits('select', props.product)" @keydown.enter="emits('select', props.product)"
        @keydown.space.prevent="emits('select', props.product)">
        <div class="relative aspect-[4/3] overflow-hidden bg-subtle">
            <img :src="props.product.preview_img" :alt="props.product.title"
                class="h-full w-full object-cover transition-transform duration-300 group-hover:scale-[1.03]" />

            <div v-if="props.product.price !== undefined"
                class="absolute right-3 top-3 rounded-full bg-surface px-3 py-1 text-sm font-semibold text-primary shadow-sm">
                ${{ props.product.price.toFixed(2) }}
            </div>
        </div>

        <div class="flex flex-1 flex-col p-4">
            <h3
                class="line-clamp-2 text-base font-semibold text-text transition-colors duration-200 group-hover:text-primary">
                {{ props.product.title }}
            </h3>

            <p class="mt-2 line-clamp-3 flex-1 text-sm text-text/70">
                {{ props.product.description }}
            </p>

            <!-- <div class="mt-4 flex items-center justify-between">
                <span v-if="props.product.price !== undefined" class="text-sm font-semibold text-primary">
                    ${{ props.product.price.toFixed(2) }}
                </span>

                <span v-else class="text-sm text-text/50" />
            </div> -->
        </div>
    </article>
</template>