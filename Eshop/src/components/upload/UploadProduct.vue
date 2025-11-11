<script setup lang="ts">
import { PaperAirplaneIcon } from '@heroicons/vue/24/outline';
import { computed, ref } from 'vue';
import { type UploadProps } from '../../types';
import { uploadProductFile } from '../../services/content.service';

const props = defineProps<{ upload: UploadProps }>();
const emit = defineEmits<{ (e: 'attachments-uploaded', files: File[]): void }>();

const selectedFiles = computed<File[]>(() => [...props.upload.existingFiles]);
const uploading = ref(false);
const uploadProgress = ref(0);

const uploadFiles = async () => {
    if (!selectedFiles.value.length) return;
    uploading.value = true;
    uploadProgress.value = 0;

    for (let i = 0; i < selectedFiles.value.length; i++) {
        const file = selectedFiles.value[i];
        await fileUpload(file!);
        uploadProgress.value = Math.round(((i + 1) / selectedFiles.value.length) * 100);
    }

    uploading.value = false;
    emit('attachments-uploaded', selectedFiles.value);
};

const fileUpload = (file: File) => {
    return new Promise(async (ok, rej) => {
        const id = await uploadProductFile(props.upload.id ?? -1, file);
        if (id) ok(id)
        else rej()
    })
}

</script>

<template>
    <div class="p-4 rounded-lg border border-gray-300/40 bg-subtle shadow-sm space-y-4">
        <h3 class="font-semibold text-sm">
            Product: {{ props.upload.product.title }}
        </h3>

        <div v-if="selectedFiles.length" class="text-xs text-gray-600">
            Selected files:
            <ul>
                <li v-for="file in selectedFiles" :key="file.name">{{ file.name }}</li>
            </ul>
        </div>

        <button
            class="flex items-center gap-2 px-4 py-2 border-2 border-blue-500 text-blue-500 rounded-lg hover:bg-blue-50 transition-colors duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
            @click="uploadFiles" :disabled="uploading">
            <PaperAirplaneIcon v-if="!uploading" class="w-5 h-5 transition-transform duration-300" />
            <div v-else class="w-5 h-5 border-4 border-blue-500 border-t-transparent rounded-full animate-spin"></div>
            <span>
                {{ uploading ? `Uploading ${uploadProgress}%` : 'Upload' }}
            </span>
        </button>
    </div>
</template>