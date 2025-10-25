<script setup lang="ts" async>
import { inject, ref, type Ref } from 'vue';
import type { Product } from '../types';
import { titleInjectionKey } from '../injectionKeys';
import ErrorProductItem from './ErrorProductItem.vue';
import SkeletonProductItem from './SkeletonProductItem.vue';
import { useLoader } from '../main';
import { getUsername } from '../services/user.service';

const emit = defineEmits<{
    (e: 'select', product: Product): void;
}>();

const props = defineProps<{ product: Product }>();
const injected: { title: Readonly<Ref<string>>, update: (t: string) => void } | undefined = inject(titleInjectionKey);

function onClick() {
    emit('select', props.product);
    if (props.product.price) {
        props.product.price++;
        props.product.description = "x"
    }
    depRef.value = new Date().toISOString();
    injected?.update(props.product.title);

    getUsername()
        .then(data => console.log(data))
        .catch(err => console.error("Failed to fetch edge config:", err));
}

const depRef = ref("Luka");
const { data, error } = useLoader<number>(depRef, () => new Promise<number>((res) => {
    const min = 100;
    const max = 5000;
    const rand = Math.floor(Math.random() * (max - min)) + min;
    return res(rand);
}));

</script>

<template>
    <article @click="onClick" role="button" :aria-label="`Open ${product.title}`"
        class="group bg-surface border border-transparent hover:border-gray-300 hover:bg-subtle rounded-lg shadow-sm hover:shadow-md transition-all duration-200 p-4 cursor-pointer flex flex-col w-full sm:w-auto">

        <template v-if="data">
            <div class="overflow-hidden rounded-md">
                <img :src="product.image" :alt="product.title"
                    class="w-full h-48 sm:h-40 object-cover group-hover:scale-[1.02] transition-transform duration-300" />
            </div>

            <div class="mt-4 flex-1 flex flex-col">
                <h3
                    class="text-base sm:text-sm font-medium text-text line-clamp-2 group-hover:text-primary transition-colors duration-200">
                    {{ product.title }}
                </h3>

                <p
                    class="text-sm sm:text-xs text-text/70 mt-2 line-clamp-3 group-hover:text-text transition-colors duration-200 flex-1">
                    {{ product.description }}
                </p>

                <div class="mt-3 flex items-center justify-between text-sm">
                    <span v-if="product.price !== undefined" class="font-semibold text-primary">
                        ${{ product.price.toFixed(2) }}
                    </span>
                    <span v-else class="text-text/50">No price</span>

                    <span class="text-xs text-text/60">
                        {{ product.seller ?? "Unknown" }}
                    </span>
                </div>
            </div>
        </template>

        <ErrorProductItem v-else-if="error" :error="error.message" />
        <SkeletonProductItem v-else />
    </article>
</template>


<style scoped></style>