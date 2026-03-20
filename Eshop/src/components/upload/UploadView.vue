<script setup lang="ts">
import { computed, inject, watchEffect, type Ref, ref, reactive } from 'vue';
import { userInfoInjectionKey } from '../../injectionKeys';
import type { UploadProps, UserInfo } from '../../types';
import { useRouter } from 'vue-router';

import CreateProduct from './CreateProduct.vue';
import CreateAttachment from './CreateAttachment.vue';
import UploadProduct from './UploadProduct.vue';
import { uploadProduct } from '../../services/http';

const router = useRouter();
const userInfo = inject<Readonly<Ref<UserInfo>>>(userInfoInjectionKey);
const canUpload = computed(() => userInfo?.value?.can_upload || false);

const submited = ref(false);
const uploadProps = reactive<UploadProps>({
    existingFiles: [],
    product: {
        description: '',
        price: -1,
        title: '',
    },
    id: null,
});

watchEffect(() => {
    if (!canUpload.value) router.push({ name: 'root' });
});

let waitingForSubmit = false;

const handleProductSubmit = (data: { title: string; price: number; description: string }) => {
    if (waitingForSubmit) return;

    waitingForSubmit = true;
    uploadProps.product = data;

    uploadProduct(uploadProps.product).request.then(res => {
        if (!res?.product_id) {
            waitingForSubmit = false;
            return;
        }

        uploadProps.id = res.product_id;
        submited.value = true;
        waitingForSubmit = false;
    }).catch(() => {
        waitingForSubmit = false;
    });
};

const handleAttachmentsSelected = (files: File[]) => {
    uploadProps.existingFiles = [...files];
};

const onAttachmentsUploaded = () => {
    uploadProps.existingFiles = [];
};
</script>

<template>
    <div v-if="canUpload" class="mx-auto w-full max-w-4xl px-4 py-6">
        <div class="mb-6">
            <h1 class="text-xl sm:text-2xl font-semibold text-text">
                {{ $t('upload.add') }}
            </h1>
        </div>

        <div class="grid gap-6">
            <CreateProduct v-if="!submited" @submit-product="handleProductSubmit" />

            <div v-else class="rounded-2xl border border-subtle bg-surface p-4 shadow-sm">
                <div class="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                    <div class="min-w-0">
                        <div class="text-xs uppercase tracking-wide text-text/60">
                            Ready for attachments
                        </div>
                        <h2 class="mt-1 truncate text-base font-semibold text-text">
                            {{ uploadProps.product.title }}
                        </h2>
                        <p class="mt-1 text-sm text-text/70 line-clamp-2">
                            {{ uploadProps.product.description }}
                        </p>
                    </div>

                    <div class="shrink-0 rounded-full bg-subtle px-3 py-1.5 text-sm font-medium text-text">
                        ${{ uploadProps.product.price }}
                    </div>
                </div>
            </div>

            <CreateAttachment v-if="submited" :existingFiles="uploadProps.existingFiles"
                @attachments-selected="handleAttachmentsSelected" />

            <UploadProduct :upload="uploadProps" @attachments-uploaded="onAttachmentsUploaded" />
        </div>
    </div>
</template>