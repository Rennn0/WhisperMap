<script setup lang="ts">
import { ref, computed } from 'vue';
import type { Product } from '../types';

type MediaItem = { type: 'image' | 'video'; src: string; alt?: string };

const props = defineProps<{ product: Product }>();
const emit = defineEmits<{ (e: 'close'): void }>();

const mediaList = computed<MediaItem[]>(() => {
    const base: MediaItem[] = [];

    if ((props.product as any).images && Array.isArray((props.product as any).images) && (props.product as any).images.length) {
        (props.product as any).images.forEach((s: string) => base.push({ type: 'image', src: s }));
    } else {
        if (props.product.image) base.push({ type: 'image', src: props.product.image, alt: props.product.title });
        for (let i = 1; i <= 2; i++) {
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

function selectMedia(i: number) {
    selectedIndex.value = i;
}
</script>

<template>
    <div class="max-w-5xl mx-auto bg-surface border border-subtle rounded-lg shadow-sm p-4">
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div class="lg:col-span-2">
                <div v-if="selectedMedia" class="w-full rounded-md overflow-hidden bg-black/5">
                    <template v-if="selectedMedia.type === 'image'">
                        <img :src="selectedMedia.src" :alt="selectedMedia.alt ?? 'product image'"
                            class="w-full h-[520px] object-cover" />
                    </template>
                    <template v-else>
                        <video :src="selectedMedia.src" controls class="w-full h-[520px] object-cover bg-black" />
                    </template>
                </div>
            </div>

            <div class="space-y-4">
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

                <div class="flex justify-between text-sm text-muted items-center">
                    <!-- Accessible Back Button -->
                    <button type="button" @click="$emit('close')" aria-label="Back to product list"
                        class="inline-flex items-center gap-2 px-3 py-2 rounded-md hover:bg-subtle focus:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 transition-colors text-sm">
                        <span aria-hidden="true" class="text-lg leading-none">←</span>
                        <span class="sr-only">Back to products</span>
                        <span class="hidden sm:inline">Back</span>
                    </button>

                    <div>Sold by <span class="font-medium">{{ props.product.seller }}</span></div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped></style>