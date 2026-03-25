<script setup lang="ts">
import { ref, computed, watchEffect, inject, type Ref, watch } from 'vue';
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
import ExpandableText from '../shared/ExpandableText.vue';
import { component as Viewer } from 'v-viewer';

const router = useRouter();
const props = defineProps<{ id: number | string }>();

const productRef = ref<Product | null>(null);
const showContactModal = ref(false);
const showDeleteConfirmation = ref(false);
const showAddButton = ref<boolean | undefined>(false);
const selectedIndex = ref(0);

const contactInfo = {
    email: 'lukadanelia056@gmail.com',
    phone: '+995 599 288 177',
};

const userInfo = inject<Readonly<Ref<UserInfo>>>(userInfoInjectionKey);

watch(showContactModal, (value) => {
    document.body.style.overflow = value ? 'hidden' : '';
});

watchEffect(() => {
    productRef.value = null;

    getProduct(props.id).request.then(p => {
        productRef.value = p?.id ? p : null;
        showAddButton.value = !p?.in_cart;
        selectedIndex.value = 0;
    });
});

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
                alt: `${product.value?.title} ${idx + 1}`,
            });
        });
    }

    return list;
});

const imageList = computed(() =>
    mediaList.value.filter(m => m.type === 'image')
);

const selectedMedia = computed(() =>
    mediaList.value[selectedIndex.value] ?? mediaList.value[0]
);

const viewerOptions = {
    inline: false,
    button: true,
    navbar: false,
    title: false,
    toolbar: true,
    tooltip: true,
    movable: true,
    zoomable: true,
    rotatable: true,
    scalable: true,
    transition: true,
    fullscreen: true,
    keyboard: true,
};

const selectMedia = (i: number) => {
    selectedIndex.value = i;
};

const contactClicked = () => {
    showContactModal.value = true;
};

const addClicked = () => {
    includeProduct(props.id).request.then(() => {
        showAddButton.value = false;
    });
};

const closeContactModal = () => {
    showContactModal.value = false;
};

const handleDeleteConfirmed = async () => {
    showDeleteConfirmation.value = false;
    deleteProduct(props.id).request.then(() =>
        router.push({ name: 'root' }).then(() => {
            window.location.reload();
        })
    );
};

const handleDeleteCancelled = () => {
    showDeleteConfirmation.value = false;
};

const deleteClicked = () => {
    showDeleteConfirmation.value = true;
};
</script>

<template>
    <div v-if="product" class="mx-auto max-w-6xl px-4 py-6 sm:px-6 lg:px-8">
        <div class="mb-6 flex items-center justify-between gap-3">
            <button type="button" aria-label="Back to product list"
                class="inline-flex items-center gap-2 rounded-xl border border-subtle bg-surface px-4 py-2 text-sm font-medium text-text transition-colors hover:bg-subtle"
                @click="router.back()">
                <TablerLeftIcon class="h-4 w-4" />
                <span>{{ $t('app.back') }}</span>
            </button>

            <button v-if="userInfo?.can_delete" type="button" aria-label="Delete product"
                class="inline-flex items-center gap-2 rounded-xl border border-subtle bg-surface px-4 py-2 text-sm font-medium text-text transition-colors hover:bg-subtle"
                @click="deleteClicked">
                <TablerDeleteIcon class="h-4 w-4" />
                <span>{{ $t('product.delete') }}</span>
            </button>
        </div>

        <div class="grid grid-cols-1 gap-6 lg:grid-cols-12">
            <div class="lg:col-span-7">
                <Viewer :images="imageList.map(x => x.src)" :options="viewerOptions">
                    <div class="overflow-hidden rounded-2xl border border-subtle bg-surface">
                        <div v-if="selectedMedia" class="flex aspect-[4/3] items-center justify-center bg-subtle">
                            <template v-if="selectedMedia.type === 'image'">
                                <img :src="selectedMedia.src" :alt="selectedMedia.alt ?? product.title"
                                    class="h-full w-full cursor-zoom-in object-contain" />
                            </template>

                            <template v-else>
                                <video :src="selectedMedia.src" controls class="h-full w-full object-contain" />
                            </template>
                        </div>
                    </div>

                    <div v-if="mediaList.length > 1" class="mt-4 flex gap-3 overflow-x-auto pb-1">
                        <button v-for="(m, i) in mediaList" :key="m.src + i" type="button"
                            class="relative h-20 w-24 shrink-0 overflow-hidden rounded-xl border bg-subtle transition-all"
                            :class="selectedIndex === i ? 'border-primary ring-2 ring-primary/30' : 'border-subtle hover:opacity-85'"
                            @click="selectMedia(i)">
                            <img v-if="m.type === 'image'" :src="m.src" :alt="m.alt"
                                class="h-full w-full object-cover" />

                            <div v-else class="flex h-full w-full items-center justify-center bg-subtle">
                                <svg class="h-6 w-6 text-text" fill="currentColor" viewBox="0 0 24 24">
                                    <path d="M8 5v14l11-7z" />
                                </svg>
                            </div>
                        </button>
                    </div>
                </Viewer>
            </div>

            <div class="lg:col-span-5">
                <div class="space-y-4 lg:sticky lg:top-24">
                    <div class="rounded-2xl border border-subtle bg-surface p-5 shadow-sm">
                        <div class="space-y-4">
                            <ExpandableText :text="product.title" :size="'large'" />

                            <div class="rounded-xl bg-subtle p-4">
                                <div class="text-sm text-text/70">
                                    {{ $t('product.price') }}
                                </div>
                                <div class="mt-1 text-3xl font-bold text-primary">
                                    ${{ (product.price ?? 0).toFixed(2) }}
                                </div>
                            </div>

                            <div class="flex flex-col gap-3">
                                <button v-if="showAddButton" type="button"
                                    class="inline-flex w-full items-center justify-center gap-2 rounded-xl bg-primary px-5 py-3 font-medium text-surface transition-opacity hover:opacity-90"
                                    @click="addClicked">
                                    <TablerAddToCartIcon class="h-5 w-5" />
                                    <span>{{ $t('product.add') }}</span>
                                </button>

                                <button type="button"
                                    class="inline-flex w-full items-center justify-center gap-2 rounded-xl border border-subtle bg-surface px-5 py-3 font-medium text-text transition-colors hover:bg-subtle"
                                    @click="contactClicked">
                                    <TablerPhoneCallIcon class="h-5 w-5" />
                                    <span>{{ $t('product.contact') }}</span>
                                </button>
                            </div>
                        </div>
                    </div>

                    <div class="rounded-2xl border border-subtle bg-surface p-5 shadow-sm">
                        <ExpandableText :text="product.description" :size="'small'" />
                    </div>
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