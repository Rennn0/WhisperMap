<script setup lang="ts">
import { computed, inject, watchEffect, type Ref, ref, reactive, onMounted } from 'vue';
import { userInfoInjectionKey } from '../../injectionKeys';
import { errorCode, type UploadProps, type UserInfo } from '../../types';
import { useRoute, useRouter } from 'vue-router';

import CreateProduct from './CreateProduct.vue';
import CreateAttachment from './CreateAttachment.vue';
import UploadProduct from './UploadProduct.vue';
import { getProduct, updateProduct, uploadProduct } from '../../services/http';

const router = useRouter();
const route = useRoute();

const userInfo = inject<Readonly<Ref<UserInfo>>>(userInfoInjectionKey);
const canUpload = computed(() => userInfo?.value?.can_upload || false);

const queryId = computed<number | null>(() => {
    const raw = route.query.id;
    if (raw == null) return null;

    const value = Array.isArray(raw) ? raw[0] : raw;
    const parsed = Number(value);

    return Number.isFinite(parsed) && parsed > 0 ? parsed : null;
});

const isEditMode = computed(() => queryId.value !== null);

const createProductSus = ref(false);
const loadingProduct = ref(false);
const existingResources = ref<string[]>([]);
const existingResIds = ref<number[]>([]);
const selectedResourceIds = ref<number[]>([]);

const uploadProps = reactive<UploadProps>({
    existingFiles: [],
    product: {
        description: '',
        price: 0,
        title: '',
        resIds: []
    },
    id: null,
});

watchEffect(() => {
    if (!canUpload.value) router.push({ name: 'root' });
});

const loadProductForEdit = async () => {
    if (!isEditMode.value || queryId.value === null) return;

    loadingProduct.value = true;

    try {
        const product = await getProduct(queryId.value).request;
        if (product.code == errorCode.argumentMissmatchInDatabase) throw new Error(errorCode.argumentMissmatchInDatabase.toString());

        existingResources.value = product.resources ?? [];
        existingResIds.value = product.res_ids ?? [];
        selectedResourceIds.value = [...(product.res_ids ?? [])];

        uploadProps.product = {
            title: product.title ?? '',
            description: product.description ?? '',
            price: product.price ?? 0,
            resIds: product.res_ids
        };

        uploadProps.id = queryId.value;
    } catch {
        router.push({ name: 'root' }).then(() => window.location.reload());
    } finally {
        loadingProduct.value = false;
    }
};

onMounted(() => {
    loadProductForEdit();
});


const handleProductSubmit = (data: { title: string; price: number; description: string }) => {
    createProductSus.value = true;
    uploadProps.product = {
        ...uploadProps.product,
        ...data
    };

    const request = isEditMode.value && uploadProps.id !== null
        ? updateProduct(uploadProps.id, uploadProps.product).request
        : uploadProduct(uploadProps.product).request;

    request
        .then((res: any) => {
            if (!isEditMode.value) {
                if (res?.product_id)
                    uploadProps.id = res.product_id;
            }
        })
        .catch(() => { }).finally(() => { createProductSus.value = false; });
};

const handleAttachmentsSelected = (files: File[]) => {
    uploadProps.existingFiles = [...files];
};

const onAttachmentsUploaded = () => {
    uploadProps.existingFiles = [];
};

const handleResourcesSelected = (ids: number[]) => {
    selectedResourceIds.value = ids;
    uploadProps.product.resIds = ids;
    uploadProps.product.resIdsDelete = existingResIds.value.filter(e => !ids.includes(e))
};
</script>

<template>
    <div v-if="canUpload" class="mx-auto w-full max-w-4xl px-4 py-6">
        <div class="mb-6">
            <h1 class="text-xl sm:text-2xl font-semibold text-text">
                {{ isEditMode ? $t('upload.edit') : $t('upload.add') }}
            </h1>
        </div>

        <div v-if="loadingProduct" class="rounded-2xl border border-subtle bg-surface p-5 shadow-sm">
            <div class="text-sm text-text/70">
                {{ $t('app.wait') }}
            </div>
        </div>

        <div v-else class="grid gap-6">
            <CreateProduct :initialProduct="uploadProps.product" :isEditMode="isEditMode" :suspend="createProductSus"
                @submit-product="handleProductSubmit" />

            <CreateAttachment :existingFiles="uploadProps.existingFiles" :existingResources="existingResources"
                :existingResIds="existingResIds" :isEditMode="isEditMode"
                @attachments-selected="handleAttachmentsSelected" @resources-selected="handleResourcesSelected" />

            <UploadProduct :upload="uploadProps" @attachments-uploaded="onAttachmentsUploaded" />
        </div>
    </div>
</template>