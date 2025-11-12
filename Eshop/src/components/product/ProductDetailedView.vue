<script setup lang="ts">
import { ref, computed, onMounted, onActivated, onUnmounted, onUpdated, watchEffect } from 'vue';
import type { MediaItem, Product } from '../../types';
import { getProduct } from '../../services/content.service';
import { useRouter } from 'vue-router';
import SkeletonProductDetail from '../skeletons/SkeletonProductDetail.vue';
import TablerLeftIcon from '../freestyle/TablerLeftIcon.vue';

const router = useRouter();
const props = defineProps<{ id: string }>();
const productRef = ref<Product | null>(null);
watchEffect(async () => {
    productRef.value = null;
    getProduct(props.id).then(p => productRef.value = p?.id ? p : null)
},)
const product = computed(() => productRef.value);

const mediaList = computed<MediaItem[]>(() => {
    if (!product.value) return [];

    const list: MediaItem[] = [];

    if (product.value.resources && product.value.resources.length > 0) {
        product.value.resources.forEach((res, idx) => {
            const isVideo = /\.(mp4|webm|mov)$/i.test(res);
            list.push({
                type: isVideo ? 'video' : 'image',
                src: res,
                alt: `${product.value?.title} ${idx + 1}`
            });
        });
    }

    return list;
});

const selectedIndex = ref(0);
const selectedMedia = computed(() => mediaList.value[selectedIndex.value] ?? mediaList.value[0]);

const showFullDescription = ref(false);
const displayedDescription = computed(() => {
    if (!product.value) return '';

    if (!product.value.description) return '';
    if (showFullDescription.value || product.value.description.length <= 256) {
        return product.value.description;
    }
    return product.value.description.slice(0, 256) + '...';
});


const selectMedia = (i: number) => {
    selectedIndex.value = i;
}

const toggleDescription = () => {
    showFullDescription.value = !showFullDescription.value;
};

//#region lifecycle hooks
onActivated(() => { });
onUpdated(() => { });
onMounted(() => { });
onUnmounted(() => { })
//#endregion
</script>

<template>
    <div v-if="product" class="max-w-5xl mx-auto bg-surface border border-subtle rounded-lg shadow-sm p-4">
        <div class="flex justify-between items-center mb-4">
            <button type="button" @click="router.back()" aria-label="Back to product list"
                class="inline-flex items-center gap-2 px-4 py-2 rounded-md bg-subtle hover:bg-subtle/60 focus:outline-none focus-visible:ring-2 focus-visible:ring-primary transition text-sm font-medium">
                <TablerLeftIcon class="w-4 h-4" />
                <span>{{ $t('app.back') }}</span>
            </button>

            <!-- <div class="text-sm  ">
                Sold by <span class="font-medium">{{ product.seller }}</span>
            </div> -->
        </div>
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div class="lg:col-span-2">
                <div v-if="selectedMedia" class="w-full rounded-md overflow-hidden bg-black/5">
                    <template v-if="selectedMedia.type === 'image'">
                        <img :src="selectedMedia.src" :alt="selectedMedia.alt ?? 'product image'"
                            class="w-full h-[700px] object-contain" />
                    </template>
                    <template v-else>
                        <video :src="selectedMedia.src" controls class="w-full h-[700px] object-contain bg-black" />
                    </template>
                </div>
            </div>

            <div class="space-y-4 max-h-[520px] overflow-y-auto pr-2">
                <div class="grid grid-cols-4 gap-2">
                    <button v-for="(m, i) in mediaList" :key="m.src + i" @click="selectMedia(i)"
                        :aria-pressed="selectedIndex === i"
                        class="relative rounded-md overflow-hidden border transition-transform duration-150"
                        :class="selectedIndex === i ? 'ring-2 ring-primary' : 'hover:scale-[1.01] border-transparent'">
                        <template v-if="m.type === 'image'">
                            <img :src="m.src" :alt="m.alt ?? 'thumb'" class="w-full h-20 object-cover" />
                        </template>
                        <template v-else>
                            <div class="w-full h-20 bg-black/20 flex items-center justify-center">
                                <video :src="m.src" muted playsinline loop class="w-full h-20 object-cover"></video>
                                <div class="absolute inset-0 flex items-center justify-center pointer-events-none">
                                    <svg class="w-6 h-6 text-white/90" fill="currentColor" viewBox="0 0 24 24">
                                        <path d="M8 5v14l11-7z" />
                                    </svg>
                                </div>
                            </div>
                        </template>
                    </button>
                </div>
            </div>
        </div>

        <div class="mt-6 border-t border-subtle pt-6 grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="md:col-span-2">
                <h1 class="text-2xl font-semibold mb-2">{{ product.title }}</h1>
                <p class="text-sm mb-4">
                    {{ displayedDescription }}
                    <span v-if="product?.description?.length > 256" @click="toggleDescription"
                        class="font-bold text-primary cursor-pointer ml-1">
                        {{ showFullDescription ? 'showLess' : 'showMore' }}
                    </span>
                </p>

            </div>

            <div class="md:col-span-1 space-y-4">
                <div class="text-lg">
                    <div class="text-sm">{{ $t('product.price') }}</div>
                    <div class="text-2xl font-bold">${{ (product.price ?? 0).toFixed(2) }}</div>
                </div>

                <div class="flex gap-2">
                    <button class="flex-1 px-4 py-2 bg-primary text-white rounded-md hover:opacity-95 transition">
                        <span>{{ $t('product.add') }}</span>
                    </button>
                    <button class="flex-1 px-4 py-2 border rounded-md hover:bg-subtle transition">
                        <span>{{ $t('product.contact') }}</span>
                    </button>
                </div>
            </div>
        </div>
    </div>
    <SkeletonProductDetail v-else />
</template>

<style scoped>
::-webkit-scrollbar {
    width: 6px;
}

::-webkit-scrollbar-thumb {
    background-color: rgba(0, 0, 0, 0.2);
    border-radius: 9999px;
}
</style>