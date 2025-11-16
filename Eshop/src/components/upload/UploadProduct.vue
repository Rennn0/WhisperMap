<script setup lang="ts">
import { computed, ref } from 'vue';
import { type UploadProps } from '../../types';
import TablerLoaderBlockWave from '../freestyle/TablerLoaderBlockWave.vue';
import TablerUploadIcon from '../freestyle/TablerUploadIcon.vue';
import { getSignedUrl, putOnUrl } from '../../services/http';

const props = defineProps<{ upload: UploadProps }>();
const emit = defineEmits<{ (e: 'attachments-uploaded', files: File[]): void }>();

const selectedFiles = computed<File[]>(() => [...props.upload.existingFiles]);
const uploading = ref(false);
const uploadProgress = ref(0);

const uploadFiles = async () => {
    if (!selectedFiles.value.length) return;

    uploading.value = true;
    uploadProgress.value = 0;

    const uploadPromises = selectedFiles.value.map(async (f, i) => {
        const buffer = await f.arrayBuffer();
        const { url } = await getSignedUrl(props.upload.id ?? -1, f.name).request;
        await putOnUrl(url, buffer);
        uploadProgress.value = Math.round(((i + 1) / selectedFiles.value.length) * 100);
    });

    await Promise.all(uploadPromises);

    uploading.value = false;
    emit('attachments-uploaded', selectedFiles.value);
};

</script>

<template>
    <div class="p-4 rounded-lg bg-subtle border shadow-sm">
        <h3 class="font-semibold text-sm">
            {{ props.upload.product.title }}
        </h3>

        <div v-if="selectedFiles.length" class="text-xs text-text">
            {{ $t('upload.inputs.chosen') }}
            <ul>
                <li v-for="file in selectedFiles" :key="file.name" class="mt-1 mb-1">{{ file.name }}</li>
            </ul>
        </div>

        <button
            class="flex items-center gap-2 mt-2 px-4 py-2 border border-text text-text rounded-lg hover:text-subtle hover:bg-text transition-colors duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
            @click="uploadFiles" :disabled="uploading">
            <TablerUploadIcon v-if="!uploading" class="w-5 h-5" />
            <TablerLoaderBlockWave v-else class="w-5 h-5" />
            <span>{{ uploading ? `${uploadProgress}%` : $t('upload.inputs.upload') }}</span>
        </button>
    </div>
</template>