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
const canUpload = computed(() => (userInfo?.value?.can_upload) || false);
const submited = ref(false);
const uploadProps = reactive<UploadProps>({
    existingFiles: [],
    product: {
        description: "",
        price: -1,
        title: "",
    },
    id: null
});

watchEffect(() => {
    if (!canUpload.value) router.push({ name: "root" });
});

let waitingForSubmit = false;
const handleProductSubmit = (data: { title: string; price: number; description: string }) => {
    if (waitingForSubmit) return;

    waitingForSubmit = true;
    uploadProps.product = data;
    uploadProduct(uploadProps.product)
        .request.then(res => {
            if (!res?.product_id) return;

            uploadProps.id = res.product_id;
            submited.value = true;
            waitingForSubmit = false;
        })
};

const handleAttachmentsSelected = (files: File[]) => {
    uploadProps.existingFiles = [...files]
};

const onAttachmentsUploaded = () => {
    uploadProps.existingFiles = [];
}
// router.push({ name: "root" }).then(() => {
//     window.location.reload();
// });

</script>

<template>
    <div v-if="canUpload" class="max-w-3xl mx-auto px-4 py-6 space-y-8">

        <CreateProduct v-if="!submited" @submit-product="handleProductSubmit" />

        <div v-else class="p-4 rounded-lg border bg-subtle shadow-sm">
            <p class="text-text/80 text-sm">{{ uploadProps.product.title.slice(0, 16) }} — {{ uploadProps.product.price
                }}$</p>
            <p class="text-text/60 text-xs mt-1">{{ uploadProps.product.description.slice(0, 16) }}</p>
        </div>

        <CreateAttachment v-if="submited" :existingFiles="uploadProps.existingFiles"
            @attachments-selected="handleAttachmentsSelected" />
        <UploadProduct :upload="uploadProps" @attachments-uploaded="onAttachmentsUploaded" />
    </div>
</template>
