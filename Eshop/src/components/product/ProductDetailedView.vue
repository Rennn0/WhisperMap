<script setup lang="ts">
import { ref, computed, watchEffect, inject, type Ref } from 'vue';
import type { AuditLog, MediaItem, Product, UserInfo } from '../../types';
import { useRouter } from 'vue-router';
import SkeletonProductDetail from '../skeletons/SkeletonProductDetail.vue';
import TablerLeftIcon from '../freestyle/TablerLeftIcon.vue';
import TablerAddToCartIcon from '../freestyle/TablerAddToCartIcon.vue';
import TablerPhoneCallIcon from '../freestyle/TablerPhoneCallIcon.vue';
import { deleteProduct, getProduct, includeProduct, sendAudit } from '../../services/http';
import TablerDeleteIcon from '../freestyle/TablerDeleteIcon.vue';
import ConfirmationModal from '../modals/ConfirmationModal.vue';
import InformationalModal from '../modals/InformationalModal.vue';
import { userInfoInjectionKey } from '../../injectionKeys';

const router = useRouter();
const props = defineProps<{ id: number }>();
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
    <div v-if="product" class="max-w-5xl mx-auto bg-surface border border-subtle rounded-lg shadow-sm p-4">
        <div class="flex justify-between items-center mb-4">
            <!-- Back -->
            <button type="button" @click="router.back()" aria-label="Back to product list"
                class="inline-flex items-center gap-2 px-4 py-2 rounded-md text-sm font-medium transition border border-subtle bg-surface text-text hover:bg-subtle hover:shadow-md">
                <TablerLeftIcon class="w-4 h-4" />
                <span>{{ $t('app.back') }}</span>
            </button>

            <!-- Delete -->
            <button v-if="userInfo?.can_delete" type="button" @click="deleteClicked" aria-label="Delete product"
                class="inline-flex items-center gap-2 px-4 py-2 rounded-md text-sm font-medium transition border border-subtle bg-surface text-text    hover:bg-subtle hover:shadow-md">
                <TablerDeleteIcon class="w-4 h-4" />
                <span class="font-medium">{{ $t('product.delete') }}</span>
            </button>
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
                        :class="selectedIndex === i ? 'ring ring-primary' : 'hover:scale-[1.01] border-transparent'">
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
                    <button
                        class="flex-1 flex items-center justify-center gap-2 px-4 py-2 rounded-md transition border border-subtle bg-surface text-text hover:bg-subtle hover:shadow-md"
                        @click="contactClicked">
                        <TablerPhoneCallIcon class="w-5 h-5" />
                        <span>{{ $t('product.contact') }}</span>
                    </button>

                    <button v-if="showAddButton"
                        class="flex-1 flex items-center justify-center gap-2 px-4 py-2 rounded-md transitionborder border-subtle bg-surface text-text hover:bg-subtle hover:shadow-md"
                        @click="addClicked">
                        <TablerAddToCartIcon class="w-5 h-5" />
                        <span>{{ $t('product.add') }}</span>
                    </button>
                </div>

            </div>
        </div>

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

<style scoped>
::-webkit-scrollbar {
    width: 6px;
}

::-webkit-scrollbar-thumb {
    background-color: rgba(0, 0, 0, 0.2);
    border-radius: 9999px;
}
</style>