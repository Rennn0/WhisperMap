<script setup lang="ts">
import { ref } from 'vue';

const title = ref('');
const price = ref<number | null>(null);
const description = ref('');

const emit = defineEmits<{
    (e: 'submit-product', data: { title: string; price: number; description: string }): void;
}>();

const onSubmit = () => {
    if (!title.value || !price.value || !description.value) return;
    emit('submit-product', { title: title.value, price: price.value, description: description.value });
};
</script>

<template>
    <div class="p-4 rounded-lg shadow-sm border border-gray-300/40 bg-surface">
        <h2 class="text-xl font-semibold text-text mb-4">Create Product</h2>

        <div class="space-y-3">
            <input v-model="title" type="text" placeholder="Title"
                class="w-full px-3 py-2 rounded-md border border-gray-300/40 bg-subtle text-text focus:ring-2 focus:ring-primary" />

            <input v-model.number="price" type="number" min="0" placeholder="Price"
                class="w-full px-3 py-2 rounded-md border border-gray-300/40 bg-subtle text-text focus:ring-2 focus:ring-primary" />

            <textarea v-model="description" placeholder="Description" rows="4"
                class="w-full px-3 py-2 rounded-md border border-gray-300/40 bg-subtle text-text focus:ring-2 focus:ring-primary"></textarea>

            <button @click="onSubmit"
                class="w-full py-2 rounded-md bg-primary text-white font-medium hover:opacity-90 transition">
                Create
            </button>
        </div>
    </div>
</template>
