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
    <div class="flex-1 flex justify-center">
        <div class="w-full max-w-2xl relative px-2 md:px-0">
            <span class="absolute inset-y-0 left-4 flex items-center pointer-events-none">
                <TablerSearchIcon class="w-5 h-5" />
            </span>

            <input ref="searchInput" :value="query" @input="onInput" @keyup.enter="onEnter" @focus="onFocus"
                @blur="closeSearchPreviewLater" type="search" :placeholder="$t('nav.search')"
                class="w-full pl-10 pr-4 py-1 md:py-2 rounded-full border bg-surface text-text outline-none md:text-sm" />

            <!-- preview box -->
            <div v-if="isOpen && products.length"
                class="absolute left-0 right-0 mt-2 bg-surface border border-subtle shadow-lg rounded-xl z-50 overflow-y-auto max-h-[70vh] md:max-w-2xl mx-auto">
                <ul>
                    <li v-for="p in products" :key="p.id" @mousedown.prevent="onProductClick(p)"
                        class="flex items-center justify-between gap-4 px-4 py-3 hover:bg-subtle cursor-pointer">
                        <div class="flex items-center gap-4 min-w-0">
                            <img :src="p.preview_img" class="w-14 h-14 rounded-lg object-cover flex-shrink-0"
                                alt="Product image" />
                            <div class="flex flex-col overflow-hidden">
                                <span class="font-semibold text-text">{{ p.title }}</span>
                                <span class="text-sm text-text/70 truncate">{{ p.description }}</span>
                            </div>
                        </div>
                        <span class="text-sm font-semibold text-text whitespace-nowrap ml-2">
                            {{ p.price ? '$' + p.price.toFixed(2) : '' }}
                        </span>
                    </li>
                </ul>
            </div>
        </div>
    </div>
</template>
