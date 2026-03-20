<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import type { TCartItem } from '../../types';
import CartItem from './CartItem.vue';
import { useRouter } from 'vue-router';
import { getProducts, removeProduct } from '../../services/http';

const router = useRouter();
const orders = reactive<TCartItem[]>([]);
const loading = ref(true);

onMounted(() => {
    const { request } = getProducts({ fromCookie: true, fromCart: true });

    request
        .then(data => {
            orders.splice(0, orders.length);

            data.products.forEach(p =>
                orders.push({
                    id: p.id,
                    price: p.price,
                    title: p.title,
                    preview_img: p.preview_img,
                } as TCartItem & { preview_img?: string })
            );
        })
        .finally(() => {
            loading.value = false;
        });
});

const removeItem = (id: number) => {
    removeProduct(id).request.then(() => {
        const index = orders.findIndex(o => o.id === id);
        if (index !== -1) {
            orders.splice(index, 1);
        }
    });
};

const visitItem = (id: number) => {
    router.push({ name: 'product', params: { id } });
};

const total = computed(() =>
    orders.reduce((sum, item) => sum + (item.price ?? 0), 0)
);
</script>

<template>
    <div class="mx-auto max-w-5xl">
        <div class="mb-5 flex items-center justify-between gap-3">
            <h2 class="text-xl font-semibold text-text">
                {{ $t('cart.title') }}
            </h2>

            <div v-if="orders.length" class="rounded-full bg-subtle px-3 py-1.5 text-sm font-medium text-text">
                ${{ total.toFixed(2) }}
            </div>
        </div>

        <div v-if="!loading && !orders.length" class="rounded-2xl border border-subtle bg-surface p-6">
            <div class="grid gap-3">
                <div class="h-5 w-32 rounded-lg bg-subtle" />
                <div class="h-4 w-48 rounded-lg bg-subtle" />
            </div>
        </div>

        <ul v-else class="grid grid-cols-1 gap-4 lg:grid-cols-2">
            <li v-for="o in orders" :key="o.id">
                <CartItem :id="o.id" :title="o.title" :price="o.price" :preview_img="(o as any).preview_img"
                    @remove="removeItem" @visit="visitItem" />
            </li>

            <li v-if="loading" v-for="i in 4" :key="'skeleton-' + i">
                <div class="rounded-2xl border border-subtle bg-surface p-4">
                    <div class="flex gap-4">
                        <div class="h-20 w-20 shrink-0 rounded-xl bg-subtle" />
                        <div class="min-w-0 flex-1">
                            <div class="h-4 w-2/3 rounded bg-subtle" />
                            <div class="mt-3 h-3 w-1/3 rounded bg-subtle" />
                            <div class="mt-4 h-9 w-24 rounded-xl bg-subtle" />
                        </div>
                    </div>
                </div>
            </li>
        </ul>
    </div>
</template>