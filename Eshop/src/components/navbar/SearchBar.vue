<script setup lang="ts">
import { ref } from 'vue';
import type { Product } from '../../types';
import TablerSearchIcon from '../freestyle/TablerSearchIcon.vue';

defineProps<{
    products: Product[];
    isOpen: boolean;
    query: string;
}>();

const emit = defineEmits<{
    (e: 'input', value: string): void;
    (e: 'search', value: string): void;
    (e: 'product-chosen', product: Product): void;
    (e: 'focus'): void;
    (e: 'blur'): void;
}>();

const searchInput = ref<HTMLInputElement | null>(null);

const onInput = (e: Event) => {
    const target = e.target as HTMLInputElement;
    emit('input', target.value);
};

const onEnter = (e: Event) => {
    const target = e.target as HTMLInputElement;
    emit('search', target.value);
};

const onProductClick = (product: Product) => {
    emit('product-chosen', product);
};

const closeSearchPreviewLater = () => {
    setTimeout(() => {
        emit('blur');
    }, 100);
};

const onFocus = () => {
    emit('focus');
};
</script>

<template>
    <div class="relative w-full">
        <div class="relative">
            <span class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-4">
                <TablerSearchIcon class="h-5 w-5 text-text/60" />
            </span>

            <input ref="searchInput" :value="query" type="search" :placeholder="$t('nav.search')"
                class="w-full rounded-2xl border border-subtle bg-subtle pl-11 pr-4 py-2.5 text-text outline-none transition-colors placeholder:text-text/50 focus:border-primary"
                @input="onInput" @keyup.enter="onEnter" @focus="onFocus" @blur="closeSearchPreviewLater" />
        </div>

        <div v-if="isOpen && products.length"
            class="absolute left-0 right-0 mt-2 max-h-[70vh] overflow-y-auto rounded-2xl border border-subtle bg-surface shadow-lg">
            <ul class="p-1">
                <li v-for="p in products" :key="p.id"
                    class="cursor-pointer rounded-xl transition-colors hover:bg-subtle"
                    @mousedown.prevent="onProductClick(p)">
                    <div class="flex items-center gap-3 px-3 py-3">
                        <img :src="p.preview_img" alt="Product image"
                            class="h-14 w-14 shrink-0 rounded-xl object-cover bg-subtle" />

                        <div class="min-w-0 flex-1">
                            <div class="truncate font-medium text-text">
                                {{ p.title }}
                            </div>
                            <div class="truncate text-sm text-text/70">
                                {{ p.description }}
                            </div>
                        </div>

                        <div class="shrink-0 text-sm font-semibold text-primary">
                            {{ p.price ? '$' + p.price.toFixed(2) : '' }}
                        </div>
                    </div>
                </li>
            </ul>
        </div>
    </div>
</template>