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

const uploadFiles = async () => {
    if (!selectedFiles.value.length || uploading.value) return;

    uploading.value = true;

    try {
        const uploadPromises = selectedFiles.value.map(async (f) => {
            const buffer = await f.arrayBuffer();
            const { url } = await getSignedUrl(props.upload.id ?? -1, f.name).request;
            await putOnUrl(url, buffer);
        });

        await Promise.all(uploadPromises);
        emit('attachments-uploaded', selectedFiles.value);
    } finally {
        uploading.value = false;
    }
};
</script>

<template>
    <section class="rounded-2xl border border-subtle bg-surface p-5 shadow-sm">
        <div class="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div class="min-w-0">
                <h3 class="mt-1 truncate text-base font-semibold text-text">
                    {{ props.upload.product.title || $t('upload.inputs.upload') }}
                </h3>

                <div v-if="selectedFiles.length" class="mt-3">
                    <div class="mb-2 text-sm text-text/80">
                        {{ $t('upload.inputs.chosen') }}
                    </div>

                    <ul class="grid gap-2">
                        <li v-for="file in selectedFiles" :key="file.name"
                            class="truncate rounded-lg bg-subtle px-3 py-2 text-sm text-text">
                            {{ file.name }}
                        </li>
                    </ul>
                </div>

                <p v-else class="mt-3 text-sm text-text/60">
                    No attachments selected yet.
                </p>
            </div>

            <div class="shrink-0">
                <button type="button" :disabled="uploading || !selectedFiles.length"
                    class="inline-flex items-center gap-2 rounded-xl border border-text px-4 py-2.5 text-text transition-colors duration-200 hover:bg-text hover:text-surface disabled:cursor-not-allowed disabled:opacity-50"
                    @click="uploadFiles">
                    <TablerUploadIcon v-if="!uploading" class="h-5 w-5" />
                    <TablerLoaderBlockWave v-else class="h-5 w-5" />
                    <span>{{ $t('upload.inputs.upload') }}</span>
                </button>
            </div>
        </div>
    </section>
</template>