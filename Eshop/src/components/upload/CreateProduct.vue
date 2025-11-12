<script setup lang="ts">
import { ref } from 'vue';
import TablerAddIcon from '../freestyle/TablerAddIcon.vue';
import TablerLoaderBlockWave from '../freestyle/TablerLoaderBlockWave.vue';

const title = ref('');
const price = ref<number | null>(null);
const description = ref('');

const submited = ref(false);

const emit = defineEmits<{
    (e: 'submit-product', data: { title: string; price: number; description: string }): void;
}>();

const onSubmit = () => {
    if (!title.value || !price.value || !description.value) return;
    submited.value = true;

    emit('submit-product', { title: title.value, price: price.value, description: description.value });
};
</script>

<template>
    <div class="p-4 rounded-lg bg-subtle border shadow-sm">
        <h2 class="text-text mb-4">{{ $t('upload.add') }}</h2>

        <div class="space-y-3">
            <input v-model="title" type="text" :placeholder="$t('upload.inputs.title')"
                class="w-full px-3 py-2 rounded-md border border-text bg-subtle text-text" />

            <input v-model.number="price" type="number" min="0" :placeholder="$t('upload.inputs.price')"
                class="w-full px-3 py-2 rounded-md border border-text bg-subtle text-text" />

            <textarea v-model="description" :placeholder="$t('upload.inputs.description')" rows="4"
                class="w-full px-3 py-2 rounded-md border border-text bg-subtle text-text"></textarea>

            <button @click="onSubmit"
                class="flex items-center gap-3 px-4 py-2 border border-text text-text rounded-lg  hover:text-subtle hover:bg-text transition-colors duration-200 ">
                <template v-if="!submited">
                    <TablerAddIcon class="w-5 h-5" />
                    <span>{{ $t('upload.inputs.create') }}</span>
                </template>
                <template v-else>
                    <TablerLoaderBlockWave class="w-5 h-5" />
                    <span>{{ $t('app.wait') }}</span>
                </template>
            </button>
        </div>
    </div>
</template>
<style scoped>
input[type=number]::-webkit-outer-spin-button,
input[type=number]::-webkit-inner-spin-button {
    -webkit-appearance: none;
    margin: 0;
}
</style>
