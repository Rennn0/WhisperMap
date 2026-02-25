<script setup lang="ts">
import { ref, computed, watchEffect, inject, type Ref } from 'vue';
import type { MediaItem, Product, UserInfo } from '../../types';
import { useRouter } from 'vue-router';
import SkeletonProductDetail from '../skeletons/SkeletonProductDetail.vue';
import TablerLeftIcon from '../freestyle/TablerLeftIcon.vue';
import TablerAddToCartIcon from '../freestyle/TablerAddToCartIcon.vue';
import TablerPhoneCallIcon from '../freestyle/TablerPhoneCallIcon.vue';
import { deleteProduct, getProduct, includeProduct } from '../../services/http';
import TablerDeleteIcon from '../freestyle/TablerDeleteIcon.vue';
import ConfirmationModal from '../modals/ConfirmationModal.vue';
import InformationalModal from '../modals/InformationalModal.vue';
import { userInfoInjectionKey } from '../../injectionKeys';
import { ZoomImg } from 'vue3-zoomer';
import ExpandableText from '../shared/ExpandableText.vue';

const router = useRouter();
const props = defineProps<{ id: number | string }>();
const productRef = ref<Product | null>(null);
const showContactModal = ref(false);
const showDeleteConfirmation = ref(false);
const showAddButton = ref<boolean | undefined>(false);
const contactInfo = {
    email: "lukadanelia056@gmail.com",
    phone: "+995 599 288 177"
}
const userInfo = inject<Readonly<Ref<UserInfo>>>(userInfoInjectionKey);
watchEffect(() => {
    if (showContactModal)
        document.body.style.overflow = 'hidden';
    else
        document.body.style.overflow = '';
});
watchEffect(async () => {
    productRef.value = null;
    getProduct(props.id).request
        .then(p => {
            productRef.value = p?.id ? p : null;
        })
        .then(() => {
            showAddButton.value = !productRef.value?.in_cart;
        })
})
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

const selectMedia = (i: number) => {
    selectedIndex.value = i;
}


const contactClicked = () => showContactModal.value = true;
const addClicked = () => {
    includeProduct(props.id).request.then((_) => showAddButton.value = false);
};
const closeContactModal = () => showContactModal.value = false;

const handleDeleteConfirmed = async () => {
    showDeleteConfirmation.value = false;
    deleteProduct(props.id).request.then(() =>
        router.push({ name: "root" }).then(() => {
            window.location.reload();
        }));
};

const handleDeleteCancelled = () => {
    showDeleteConfirmation.value = false;
};

const deleteClicked = () => {
    showDeleteConfirmation.value = true;
};


</script>

<template>
    <div v-if="product" class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <!-- Top Bar -->
        <div class="flex justify-between items-center mb-6">
            <!-- Back (UNCHANGED VISUAL STYLE) -->
            <button type="button" @click="router.back()" aria-label="Back to product list" class="inline-flex items-center gap-2 px-4 py-2 rounded-md text-sm font-medium transition
               border border-subtle bg-surface text-text
               hover:bg-subtle hover:shadow-md">
                <TablerLeftIcon class="w-4 h-4" />
                <span>{{ $t('app.back') }}</span>
            </button>

            <!-- Delete (UNCHANGED VISUAL STYLE) -->
            <button v-if="userInfo?.can_delete" type="button" @click="deleteClicked" aria-label="Delete product" class="inline-flex items-center gap-2 px-4 py-2 rounded-md text-sm font-medium transition
               border border-subtle bg-surface text-text
               hover:bg-subtle hover:shadow-md">
                <TablerDeleteIcon class="w-4 h-4" />
                <span class="font-medium">{{ $t('product.delete') }}</span>
            </button>
        </div>

        <!-- Main Layout -->
        <div class="grid grid-cols-1 lg:grid-cols-5 gap-10">
            <!-- Media Section -->
            <div class="lg:col-span-3 space-y-4">
                <!-- Main Media -->
                <div v-if="selectedMedia" class="w-full rounded-xl overflow-hidden bg-subtle aspect-[4/3]
                 flex items-center justify-center">
                    <template v-if="selectedMedia.type === 'image'">
                        <ZoomImg :src="selectedMedia.src" :alt="selectedMedia.alt ?? 'product image'"
                            class="w-full h-full object-contain transition duration-300" zoom-type="drag"
                            :zoom-scale="5" :step="1" :show-img-map="true" />
                    </template>

                    <template v-else>
                        <video :src="selectedMedia.src" controls class="w-full h-full object-contain bg-black" />
                    </template>
                </div>

                <!-- Thumbnails -->
                <div class="flex gap-3 overflow-x-auto pb-1">
                    <button v-for="(m, i) in mediaList" :key="m.src + i" @click="selectMedia(i)" class="relative rounded-lg overflow-hidden shrink-0 w-24 h-20
                   border transition" :class="selectedIndex === i
                    ? 'border-primary ring-2 ring-primary/30'
                    : 'border-subtle hover:opacity-80'">
                        <img v-if="m.type === 'image'" :src="m.src" class="w-full h-full object-cover" />

                        <div v-else class="w-full h-full bg-black/20 flex items-center justify-center">
                            <svg class="w-6 h-6 text-white/90" fill="currentColor" viewBox="0 0 24 24">
                                <path d="M8 5v14l11-7z" />
                            </svg>
                        </div>
                    </button>
                </div>
            </div>

            <!-- Info Section -->
            <div class="lg:col-span-2 space-y-6 lg:sticky lg:top-24 h-fit">
                <!-- Purchase Card -->
                <div class="bg-surface rounded-xl p-6 border border-subtle shadow-sm space-y-4">
                    <ExpandableText :text="product.title" :size="'large'" />

                    <div>
                        <div class="text-sm opacity-70">
                            {{ $t('product.price') }}
                        </div>
                        <div class="text-3xl font-bold text-primary">
                            ${{ (product.price ?? 0).toFixed(2) }}
                        </div>
                    </div>

                    <div class="flex flex-col gap-3 pt-2">
                        <button v-if="showAddButton" @click="addClicked" class="w-full flex items-center justify-center gap-2
                     px-5 py-3 rounded-lg font-medium transition
                     bg-primary text-white hover:opacity-90">
                            <TablerAddToCartIcon class="w-5 h-5" />
                            <span>{{ $t('product.add') }}</span>
                        </button>

                        <button @click="contactClicked" class="w-full flex items-center justify-center gap-2
                     px-5 py-3 rounded-lg font-medium transition
                     border border-subtle bg-surface text-text
                     hover:bg-subtle hover:shadow-md">
                            <TablerPhoneCallIcon class="w-5 h-5" />
                            <span>{{ $t('product.contact') }}</span>
                        </button>
                    </div>
                </div>

                <!-- Description Card -->
                <div class="bg-surface rounded-xl p-6 border border-subtle shadow-sm">
                    <h2 class="text-lg font-semibold mb-3">
                        {{ $t('product.description') }}
                    </h2>
                    <ExpandableText :text="product.description" :size="'small'" />
                </div>
            </div>
        </div>

        <!-- Modals -->
        <ConfirmationModal :isOpen="showDeleteConfirmation" :title="$t('product.modals.delete.title')"
            :description="$t('product.modals.delete.desc')" :cancel-text="$t('product.modals.delete.cancel')"
            :confirm-text="$t('product.modals.delete.confirm')" @confirmed="handleDeleteConfirmed"
            @cancelled="handleDeleteCancelled" />

        <InformationalModal :isOpen="showContactModal" :title="$t('product.info')"
            :text="`${$t('product.email')}: ${contactInfo.email}\n${$t('product.phone')}: ${contactInfo.phone}`"
            @closed="closeContactModal" />
    </div>

    <SkeletonProductDetail v-else />
</template>