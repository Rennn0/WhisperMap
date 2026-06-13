<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import type { TCartItem } from '../../types';
import CartItem from './CartItem.vue';
import { useRouter } from 'vue-router';
import { getProducts, getToken, removeProduct, requestPay } from '../../services/http';

const router = useRouter();
const cartItems = reactive<TCartItem[]>([]);
const loading = ref(true);

onMounted(() => {
    getToken().then(t => sessionStorage.setItem("t", t)).catch(console.error);
    const { request } = getProducts({ fromCookie: true, fromCart: true });
    request
        .then(data => {
            cartItems.splice(0, cartItems.length);

            data.products.forEach(p =>
                cartItems.push({
                    id: p.id,
                    price: p.price,
                    title: p.title,
                    preview_img: p.preview_img,
                    description: p.description,
                    orders: p.orders,
                  is_paid: p.is_paid,
                } as TCartItem & { preview_img?: string })
            );
        })
        .finally(() => {
            loading.value = false;
        });
});

const removeItem = (id: number) => {
    removeProduct(id).request.then(() => {
        const index = cartItems.findIndex(o => o.id === id);
        if (index !== -1) {
            cartItems.splice(index, 1);
        }
    });
};

const visitItem = (id: number) => {
    router.push({ name: 'product', params: { id } });
};

const payForItem = (id: number) => {
    const item = cartItems.find(c => c.id == id);
    const existingCheckout = item?.orders?.find(x => x.use_link);
    if (existingCheckout) {
        window.open(existingCheckout.checkout_url, '_blank')?.focus();
    }
    else if (item?.id && item.price) {
        const amountInCents = Math.round(item.price * 100);
        requestPay(item.id.toString(), amountInCents.toString(), item.title).request.then(d => {
            console.log(d);
            window.open(d.checkoutUrl, '_blank')?.focus();
        }).catch(err => {
            if (err?.response?.status === 401) {
                sessionStorage.removeItem("t");
                window.location.href = "/auth";
                return;
            }

            console.error(err);
        })
    }
};


const total = computed(() =>
    cartItems.reduce((sum, item) => sum + (item.price ?? 0), 0)
);
</script>

<template>
    <div class="mx-auto max-w-5xl">
        <div class="mb-5 flex items-center justify-between gap-3">
            <h2 class="text-xl font-semibold text-text">
                {{ $t('cart.title') }}
            </h2>

            <div v-if="cartItems.length" class="rounded-full bg-subtle px-3 py-1.5 text-sm font-medium text-text">
                ${{ total.toFixed(2) }}
            </div>
        </div>

        <div v-if="!loading && !cartItems.length" class="rounded-2xl border border-subtle bg-surface p-6">
            <div class="grid gap-3">
                <div class="h-5 w-32 rounded-lg bg-subtle" />
                <div class="h-4 w-48 rounded-lg bg-subtle" />
            </div>
        </div>

        <ul v-else class="grid grid-cols-1 gap-4 lg:grid-cols-2">
            <li v-for="o in cartItems" :key="o.id">
                <CartItem :id="o.id" :title="o.title" :price="o.price" :preview_img="(o as any).preview_img"
                    :orders="o.orders" :is_paid="o.is_paid" @remove="removeItem" @visit="visitItem" @pay="payForItem" />
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