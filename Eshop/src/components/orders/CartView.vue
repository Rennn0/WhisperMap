<script setup lang="ts">
import { onMounted, reactive } from 'vue';
import { getProducts, removeProduct } from '../../services/http';
import type { TCartItem } from '../../types';
import CartItem from './CartItem.vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const orders = reactive<TCartItem[]>([]);

onMounted(() => {
    const { request } = getProducts({ fromCookie: true });
    request.then(data =>
        data.products.forEach(p =>
            orders.push({ id: p.id, price: p.price, title: p.title })
        )
    );
});

const removeItem = (id: number) => {
    removeProduct(id).request.then((_) => {
        orders.splice(orders.findIndex(o => o.id == id), 1);
    });
};

const visitItem = (id: number) => { router.push({ name: "product", params: { id } }) };
</script>

<template>
    <div>
        <h2 class="text-xl font-semibold mb-4">{{ $t('cart.title') }}</h2>

        <ul class="grid gap-4 grid-cols-1 lg:grid-cols-2">
            <li v-for="o in orders" :key="o.id">
                <CartItem :id="o.id" :title="o.title" :price="o.price" @remove="removeItem" @visit="visitItem" />
            </li>
        </ul>
    </div>
</template>
