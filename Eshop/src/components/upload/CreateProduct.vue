<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import TablerAddIcon from '../freestyle/TablerAddIcon.vue';
import TablerLoaderBlockWave from '../freestyle/TablerLoaderBlockWave.vue';

type ProductForm = {
    title: string;
    price: number;
    description: string;
};

const props = defineProps<{
    initialProduct: ProductForm;
    isEditMode: boolean;
}>();

const emit = defineEmits<{
    (e: 'submit-product', data: { title: string; price: number; description: string }): void;
}>();

const title = ref('');
const price = ref<number | null>(null);
const description = ref('');
const submited = ref(false);

watch(
    () => props.initialProduct,
    (value) => {
        title.value = value.title ?? '';
        price.value = typeof value.price === 'number' ? value.price : null;
        description.value = value.description ?? '';
        submited.value = false;
    },
    { immediate: true, deep: true }
);

const canSubmit = computed(() =>
    !!title.value.trim() &&
    price.value !== null &&
    price.value >= 0 &&
    !!description.value.trim()
);

const onSubmit = () => {
    if (!canSubmit.value || submited.value) return;

    submited.value = true;

    emit('submit-product', {
        title: title.value.trim(),
        price: price.value!,
        description: description.value.trim(),
    });
};
</script>

<template>
    <section class="rounded-2xl border border-subtle bg-surface p-5 shadow-sm">
        <div class="grid gap-4">
            <div class="grid gap-2">
                <label class="text-sm font-medium text-text">
                    {{ $t('upload.inputs.title') }}
                </label>
                <input v-model="title" type="text"
                    class="w-full rounded-xl border border-subtle bg-subtle px-3 py-2.5 text-text outline-none transition-colors placeholder:text-text/50 focus:border-primary" />
            </div>

            <div class="grid gap-2">
                <label class="text-sm font-medium text-text">
                    {{ $t('upload.inputs.price') }}
                </label>
                <input v-model.number="price" type="number" min="0"
                    class="w-full rounded-xl border border-subtle bg-subtle px-3 py-2.5 text-text outline-none transition-colors placeholder:text-text/50 focus:border-primary" />
            </div>

            <div class="grid gap-2">
                <label class="text-sm font-medium text-text">
                    {{ $t('upload.inputs.description') }}
                </label>
                <textarea v-model="description" rows="3"
                    class="w-full resize-y rounded-xl border border-subtle bg-subtle px-3 py-2.5 text-text outline-none transition-colors placeholder:text-text/50 focus:border-primary" />
            </div>

            <div class="pt-2">
                <button type="button" :disabled="!canSubmit || submited"
                    class="inline-flex items-center gap-2 rounded-xl border border-text px-4 py-2.5 text-text transition-colors duration-200 hover:bg-text hover:text-surface disabled:cursor-not-allowed disabled:opacity-50"
                    @click="onSubmit">
                    <template v-if="!submited">
                        <TablerAddIcon class="h-5 w-5" />
                        <span>
                            {{ props.isEditMode ? $t('upload.inputs.update') : $t('upload.inputs.create') }}
                        </span>
                    </template>

                    <template v-else>
                        <TablerLoaderBlockWave class="h-5 w-5" />
                        <span>{{ $t('app.wait') }}</span>
                    </template>
                </button>
            </div>
        </div>
    </section>
</template>

<style scoped>
input[type=number]::-webkit-outer-spin-button,
input[type=number]::-webkit-inner-spin-button {
    -webkit-appearance: none;
    margin: 0;
}
</style>