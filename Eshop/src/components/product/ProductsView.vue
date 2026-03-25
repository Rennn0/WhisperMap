<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import type { Product } from '../../types';
import { useRouter } from 'vue-router';
import ProductItem from './ProductItem.vue';
import SkeletonProductItem from '../skeletons/SkeletonProductItem.vue';
import { getProducts } from '../../services/http';
import BaseDropdown from '../dropdown/BaseDropdown.vue';

enum OrderBy {
    NewestFirst = 0,
    OldestFirst = 1,
    PriceIncreasing = 2,
    PriceDecreasing = 3
}
const router = useRouter();
const loading = ref(false);
const loadingMore = ref(false);
const emit = defineEmits<{ (e: 'select', product: Product): void }>();

const productsRef = ref<Product[] | null>(null);
const continuationToken = ref<string | null>(null);
const selectedOrder = ref<OrderBy>(OrderBy.NewestFirst);
const selectedBatch = ref<number>(5);
const products = computed(() => productsRef.value);

const orderOptions = [
    { value: OrderBy.NewestFirst, label: 'product.sort.newestFirst' },
    { value: OrderBy.OldestFirst, label: 'product.sort.oldestFirst' },
    { value: OrderBy.PriceIncreasing, label: 'product.sort.priceIncreasing' },
    { value: OrderBy.PriceDecreasing, label: 'product.sort.priceDecreasing' },
];

const batchOptions = [
    { value: 5, label: '5' },
    { value: 10, label: '10' },
    { value: 20, label: '20' },
    { value: 50, label: '50' }
];

const selectOrder = (value: OrderBy, close: () => void) => {
    if (selectedOrder.value !== value) {
        selectedOrder.value = value;
    }
    close();
};

const selectBatch = (value: number, close: () => void) => {
    if (selectedBatch.value !== value) {
        selectedBatch.value = value;
    }
    close();
};

const selectedOrderLabel = computed(() =>
    orderOptions.find(x => x.value === selectedOrder.value)?.label ?? orderOptions[0]!.label
);

const selectedBatchLabel = computed(() =>
    batchOptions.find(x => x.value === selectedBatch.value)?.label ?? '20'
);

const loadProducts = async () => {
    try {
        loadingMore.value = false;
        loading.value = true;
        const ps = await getProducts({ orderBy: selectedOrder.value, continuationToken: null, batch: selectedBatch.value }).request
        productsRef.value = ps.products ?? [];
        continuationToken.value = ps.continuation_token ?? null;
    } finally {
        loading.value = false;
    }
}

const loadMore = async () => {
    if (loadingMore.value || !continuationToken.value) return;

    loadingMore.value = true;
    try {
        const ps = await getProducts({ orderBy: selectedOrder.value, continuationToken: continuationToken.value, batch: selectedBatch.value }).request;
        productsRef.value = [...(productsRef.value as any), ...(ps.products ?? [])];
        continuationToken.value = ps.continuation_token ?? null;
    } finally {
        loadingMore.value = false;
    }

}

const onSelect = (product: Product) => {
    router.push({ name: 'product', params: { id: product.id } });
    emit('select', product);
};

watch(selectedOrder, () => {
    continuationToken.value = null;
    loadProducts();
});

onMounted(() => { loadProducts(); });
</script>
<template>
    <div class="space-y-4">
        <div class="flex justify-end">
            <BaseDropdown align="left" widthClass="w-56">
                <template #trigger="{ toggle, open, triggerRef }">
                    <button :ref="triggerRef as any" type="button" :aria-expanded="open" aria-haspopup="menu"
                        class="inline-flex min-w-[220px] items-center justify-between gap-3 rounded-xl border border-subtle bg-surface px-3 py-2.5 text-sm text-text transition-colors hover:bg-hover focus:outline-none"
                        @click="toggle()">
                        <span class="truncate font-medium">
                            {{ $t(selectedOrderLabel) }}
                        </span>

                        <svg class="h-4 w-4 shrink-0 transition-transform duration-200"
                            :class="open ? 'rotate-180' : ''" viewBox="0 0 20 20" fill="currentColor"
                            aria-hidden="true">
                            <path fill-rule="evenodd"
                                d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.51a.75.75 0 01-1.08 0l-4.25-4.51a.75.75 0 01.02-1.06z"
                                clip-rule="evenodd" />
                        </svg>
                    </button>
                </template>

                <template #content="{ close }">
                    <div class="flex flex-col gap-1">
                        <button v-for="option in orderOptions" :key="option.value" type="button" role="menuitem"
                            class="flex w-full items-center gap-3 rounded-md px-3 py-2 text-left text-sm transition-colors"
                            :class="selectedOrder === option.value ? 'bg-subtle font-medium text-text' : 'text-text hover:bg-hover'"
                            @click="selectOrder(option.value, close)">
                            <span class="truncate">{{ $t(option.label) }}</span>

                            <span v-if="selectedOrder === option.value"
                                class="ml-auto inline-block h-2 w-2 rounded-full bg-primary" />
                        </button>
                    </div>
                </template>
            </BaseDropdown>

            <BaseDropdown align="left" widthClass="w-36">
                <template #trigger="{ toggle, open, triggerRef }">
                    <button :ref="triggerRef as any" type="button" :aria-expanded="open" aria-haspopup="menu"
                        class="inline-flex min-w-[120px] items-center justify-between gap-3 rounded-xl border border-subtle bg-surface px-3 py-2.5 text-sm text-text transition-colors hover:bg-hover focus:outline-none"
                        @click="toggle()">
                        <span class="truncate font-medium">
                            {{ selectedBatchLabel }}
                        </span>

                        <svg class="h-4 w-4 shrink-0 transition-transform duration-200"
                            :class="open ? 'rotate-180' : ''" viewBox="0 0 20 20" fill="currentColor"
                            aria-hidden="true">
                            <path fill-rule="evenodd"
                                d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.51a.75.75 0 01-1.08 0l-4.25-4.51a.75.75 0 01.02-1.06z"
                                clip-rule="evenodd" />
                        </svg>
                    </button>
                </template>

                <template #content="{ close }">
                    <div class="flex flex-col gap-1">
                        <button v-for="option in batchOptions" :key="option.value" type="button" role="menuitem"
                            class="flex w-full items-center gap-3 rounded-md px-3 py-2 text-left text-sm transition-colors"
                            :class="selectedBatch === option.value ? 'bg-subtle font-medium text-text' : 'text-text hover:bg-hover'"
                            @click="selectBatch(option.value, close)">
                            <span class="truncate">{{ option.label }}</span>
                            <span v-if="selectedBatch === option.value"
                                class="ml-auto inline-block h-2 w-2 rounded-full bg-primary" />
                        </button>
                    </div>
                </template>
            </BaseDropdown>
        </div>

        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            <template v-if="products && !loading">
                <ProductItem v-for="item in products" :key="item.id" :product="item" @select="onSelect" />
            </template>

            <template v-else>
                <SkeletonProductItem v-for="(_, i) in Array(12)" :key="i" />
            </template>
        </div>

        <div class="flex justify-center pt-2" v-if="products?.length ?? 0 > 0">
            <button type="button"
                class="rounded-xl border border-subtle bg-surface px-4 py-2 text-sm text-text transition-colors hover:bg-hover disabled:opacity-50"
                :disabled="loadingMore" @click="loadMore">
                {{ loadingMore ? 'Loading...' : 'Load more' }}
            </button>
        </div>
    </div>
</template>