<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import type { MediaItem, Product } from '../types';
import { ArrowLeftIcon } from '@heroicons/vue/24/solid';

const props = defineProps<{ product: Product }>();
const emit = defineEmits<{ (e: 'close'): void }>();

const mediaList = computed<MediaItem[]>(() => {
    const base: MediaItem[] = [];

    if ((props.product as any).images && Array.isArray((props.product as any).images) && (props.product as any).images.length) {
        (props.product as any).images.forEach((s: string) => base.push({ type: 'image', src: s }));
    } else {
        if (props.product.image) base.push({ type: 'image', src: props.product.image, alt: props.product.title });
        for (let i = 1; i <= 25; i++) {
            base.push({
                type: 'image',
                src: `https://picsum.photos/seed/${props.product.id}-extra-${i}/800/600`,
                alt: `${props.product.title} ${i + 1}`
            });
        }
        base.push({ type: 'video', src: 'https://samplelib.com/lib/preview/mp4/sample-5s.mp4' });
    }

    return base;
});

const selectedIndex = ref(0);
const selectedMedia = computed(() => mediaList.value[selectedIndex.value] ?? mediaList.value[0]);

const selectMedia = (i: number) => {
    selectedIndex.value = i;
}

let startX = 0;
let endX = 0;

const handleTouchStart = (e: TouchEvent) => {
    if (!e || !e.touches) return;
    startX = e.touches[0]?.clientX || 0;
};

const handleTouchEnd = (e: TouchEvent) => {
    if (!e || !e.touches) return;
    endX = e.changedTouches[0]?.clientX || 0;
    const diff = endX - startX;

    const threshold = 50;

    if (Math.abs(diff) > threshold) {
        if (diff < 0 && selectedIndex.value < mediaList.value.length - 1) {
            selectedIndex.value++;
        } else if (diff > 0 && selectedIndex.value > 0) {
            selectedIndex.value--;
        }
    }
};

onMounted(() => {
    window.scrollTo({ top: 0, behavior: 'smooth' })
})

</script>

<template>
    <div class="max-w-5xl mx-auto bg-surface border border-subtle rounded-lg shadow-sm p-4">
        <div class="flex justify-between items-center mb-4">
            <button type="button" @click="$emit('close')" aria-label="Back to product list"
                class="inline-flex items-center gap-2 px-4 py-2 rounded-md bg-subtle hover:bg-subtle/60 focus:outline-none focus-visible:ring-2 focus-visible:ring-primary transition text-sm font-medium">
                <ArrowLeftIcon class="w-4 h-4" />
                <span>Back</span>
            </button>

            <div class="text-sm text-muted">
                Sold by <span class="font-medium">{{ props.product.seller }}</span>
            </div>
        </div>
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div class="lg:col-span-2">
                <div v-if="selectedMedia" class="w-full rounded-md overflow-hidden bg-black/5"
                    @touchstart="handleTouchStart" @touchend="handleTouchEnd">
                    <template v-if="selectedMedia.type === 'image'">
                        <img :src="selectedMedia.src" :alt="selectedMedia.alt ?? 'product image'"
                            class="w-full h-[520px] object-cover" />
                    </template>
                    <template v-else>
                        <video :src="selectedMedia.src" controls class="w-full h-[520px] object-cover bg-black" />
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

                <div class="space-y-2">
                    <div class="text-sm text-muted">Media</div>
                    <div class="text-xs text-muted">Choose an image or video to preview in large view</div>
                </div>
            </div>
        </div>

        <div class="mt-6 border-t border-subtle pt-6 grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="md:col-span-2">
                <h1 class="text-2xl font-semibold mb-2">{{ props.product.title }}</h1>
                <p class="text-sm text-muted mb-4">{{ props.product.description }}</p>
            </div>

            <div class="md:col-span-1 space-y-4">
                <div class="text-lg">
                    <div class="text-sm text-muted">Price</div>
                    <div class="text-2xl font-bold">${{ (props.product.price ?? 0).toFixed(2) }}</div>
                </div>

                <div class="flex gap-2">
                    <button class="flex-1 px-4 py-2 bg-primary text-white rounded-md hover:opacity-95 transition">Add to
                        cart</button>
                    <button class="flex-1 px-4 py-2 border rounded-md hover:bg-subtle transition">Contact
                        seller</button>
                </div>
            </div>
        </div>
    </div>
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