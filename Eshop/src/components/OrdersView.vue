<script setup lang="ts">
const orders = [
    { id: 'o1', title: 'Order #1', status: 'Processing' },
    { id: 'o2', title: 'Order #2', status: 'Shipped' },
    { id: 'o3', title: 'Order #3', status: 'Delivered' },
];
</script>

<template>
    <div class="space-y-4">
        <h2 class="text-xl font-semibold mb-2">Orders</h2>
        <ul class="space-y-2">
            <li v-for="o in orders" :key="o.id" class="p-3 bg-surface border border-subtle rounded-md">
                <div class="flex items-center justify-between">
                    <div>
                        <div class="font-medium">{{ o.title }}</div>
                        <div class="text-sm text-muted">{{ o.id }}</div>
                    </div>
                    <div class="text-sm px-2 py-1 rounded-full bg-gray-100 text-gray-700">{{ o.status }}</div>
                </div>
            </li>
        </ul>
    </div>
</template>

<style scoped></style>