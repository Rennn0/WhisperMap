<script setup lang="ts">
import { ref, watch, computed, onBeforeUnmount } from 'vue';
import TablerChooseFileIcon from '../freestyle/TablerChooseFileIcon.vue';

const props = defineProps<{
    existingFiles?: File[];
    existingResources?: string[];
    existingResIds?: number[];
    isEditMode?: boolean;
}>();

const emit = defineEmits<{
    (e: 'attachments-selected', files: File[]): void;
    (e: 'resources-selected', ids: number[]): void;
}>();

const files = ref<File[]>(props.existingFiles ? [...props.existingFiles] : []);
const isDraggingOver = ref(false);
const selectedIndex = ref<number | null>(null);
const selectedResourceIds = ref<number[]>([]);

watch(() => props.existingFiles, (v) => {
    files.value = v ? [...v] : [];
});

watch(() => props.existingResIds, (v) => {
    selectedResourceIds.value = v ? [...v] : [];
}, { immediate: true });

const createStableFiles = async (inputFiles: File[]) => {
    return Promise.all(
        inputFiles.map(async (file) => {
            const buffer = await file.arrayBuffer();
            return new File([buffer], file.name, { type: file.type });
        })
    );
};

const appendFiles = async (incoming: File[]) => {
    const filtered = incoming.filter(file =>
        file.type.startsWith('image/') || file.type.startsWith('video/')
    );

    if (!filtered.length) return;

    const stableFiles = await createStableFiles(filtered);
    files.value = [...files.value, ...stableFiles];
    emit('attachments-selected', files.value);
};

const onFilesChange = async (e: Event) => {
    const input = e.target as HTMLInputElement;
    if (!input.files?.length) return;

    await appendFiles(Array.from(input.files));
    input.value = '';
};

const removeFile = (index: number) => {
    files.value.splice(index, 1);

    if (selectedIndex.value === index) selectedIndex.value = null;
    if (selectedIndex.value !== null && selectedIndex.value > index) selectedIndex.value--;

    emit('attachments-selected', files.value);
};

const handleSelect = (index: number) => {
    if (selectedIndex.value === null) {
        selectedIndex.value = index;
        return;
    }

    if (selectedIndex.value === index) {
        selectedIndex.value = null;
        return;
    }

    const temp = files.value[selectedIndex.value];
    files.value[selectedIndex.value] = files.value[index]!;
    files.value[index] = temp!;
    selectedIndex.value = null;

    emit('attachments-selected', files.value);
};

const handleDragOver = (e: DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    isDraggingOver.value = true;
};

const handleDragLeave = (e: DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    isDraggingOver.value = false;
};

const handleDrop = async (e: DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    isDraggingOver.value = false;

    const droppedFiles = e.dataTransfer?.files;
    if (!droppedFiles?.length) return;

    await appendFiles(Array.from(droppedFiles));
};

const toggleResourceSelection = (resId: number) => {
    const index = selectedResourceIds.value.indexOf(resId);
    if (index > -1) {
        selectedResourceIds.value.splice(index, 1);
    } else {
        selectedResourceIds.value.push(resId);
    }
    emit('resources-selected', selectedResourceIds.value);
};

const previews = computed(() =>
    files.value.map(file => ({
        file,
        url: URL.createObjectURL(file),
        isImage: file.type.startsWith('image/'),
        isVideo: file.type.startsWith('video/'),
    }))
);

watch(previews, (_, prev) => {
    prev?.forEach(p => URL.revokeObjectURL(p.url));
}, { flush: 'post' });

onBeforeUnmount(() => {
    previews.value.forEach(p => URL.revokeObjectURL(p.url));
});
</script>

<template>
    <section class="rounded-2xl border border-subtle bg-surface p-5 shadow-sm">
        <div v-if="isEditMode && existingResources?.length" class="mb-6">
            <div class="mb-4">
                <h3 class="text-base font-semibold text-text">
                    {{ $t('upload.inputs.existingResources') || 'Existing Resources' }}
                </h3>
                <p class="mt-1 text-sm text-text/70">
                    მონიშნე რაც დარჩეს
                </p>
            </div>

            <div class="grid grid-cols-1 gap-3 md:grid-cols-2 lg:grid-cols-3">
                <div v-for="(resource, index) in existingResources" :key="index"
                    class="group relative flex items-center gap-3 rounded-xl border p-3 transition-all duration-200 cursor-pointer"
                    :class="selectedResourceIds.includes(existingResIds?.[index] ?? -1)
                        ? 'border-primary bg-primary/10 shadow-sm'
                        : 'border-subtle bg-subtle/40 hover:border-primary/40 hover:bg-subtle/70'
                        " @click="toggleResourceSelection(existingResIds?.[index] ?? -1)">
                    <input type="checkbox" :checked="selectedResourceIds.includes(existingResIds?.[index] ?? -1)"
                        class="h-4 w-4 shrink-0 cursor-pointer rounded"
                        @change="toggleResourceSelection(existingResIds?.[index] ?? -1)" />

                    <div class="relative shrink-0 overflow-visible">
                        <img :src="resource" :alt="`Preview`"
                            class="h-16 w-16 rounded-lg border border-subtle object-cover bg-surface transition-transform duration-200 group-hover:scale-105" />

                        <div
                            class="pointer-events-none absolute left-full top-1/2 z-50 ml-4 -translate-y-1/2 opacity-0 transition-all duration-200 group-hover:opacity-100">
                            <img :src="resource" :alt="`Preview Large`"
                                class="max-h-[320px] max-w-[320px] rounded-2xl border border-subtle bg-surface object-contain shadow-2xl" />
                        </div>
                    </div>

                    <div class="min-w-0 flex-1">
                        <div class="truncate text-sm font-medium text-text">
                            {{ index + 1 }}
                        </div>

                        <div class="mt-1 truncate text-xs text-text/60">
                            {{ resource }}
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="mb-4">
            <h3 class="text-base font-semibold text-text">
                {{ $t('upload.inputs.chooseFile') }}
            </h3>
            <p class="mt-1 text-sm text-text/70">
                Add images or videos. You can drag files here, remove them, or tap two items to swap their order.
            </p>
        </div>

        <div class="rounded-2xl border-2 border-dashed bg-subtle/50 p-5 transition-colors"
            :class="isDraggingOver ? 'border-primary' : 'border-subtle'" @dragover="handleDragOver"
            @dragleave="handleDragLeave" @drop="handleDrop">
            <label class="flex w-full cursor-pointer flex-col items-center justify-center gap-3 text-center">
                <div class="flex h-12 w-12 items-center justify-center rounded-full bg-surface border border-subtle">
                    <TablerChooseFileIcon class="h-6 w-6 text-text" />
                </div>

                <div>
                    <div class="text-sm font-medium text-text">
                        {{ $t('upload.inputs.chooseFile') }}
                    </div>
                    <div class="mt-1 text-xs text-text/60">
                        Drag and drop or click to browse
                    </div>
                </div>

                <input type="file" multiple accept="image/*,video/*" class="hidden" @change="onFilesChange" />
            </label>
        </div>

        <div v-if="previews.length" class="mt-5">
            <div class="mb-3 flex items-center justify-between">
                <div class="text-sm font-medium text-text">
                    Selected files
                </div>
                <div class="text-xs text-text/60">
                    {{ previews.length }} item<span v-if="previews.length !== 1">s</span>
                </div>
            </div>

            <div class="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
                <div v-for="(p, index) in previews" :key="p.url"
                    class="group relative aspect-square overflow-hidden rounded-xl border border-subtle bg-subtle select-none cursor-pointer transition-all duration-200"
                    :class="selectedIndex === index ? 'scale-[1.02] ring-2 ring-primary' : 'hover:shadow-sm'"
                    @click="handleSelect(index)">
                    <img v-if="p.isImage" :src="p.url" class="h-full w-full object-cover" />

                    <video v-else-if="p.isVideo" :src="p.url" class="h-full w-full object-cover" muted controls />

                    <div class="absolute inset-x-0 bottom-0 bg-black/45 px-2 py-1.5 text-xs text-white">
                        <div class="truncate">
                            {{ p.file.name }}
                        </div>
                    </div>

                    <button type="button"
                        class="absolute right-2 top-2 rounded-md bg-black/60 px-2 py-1 text-xs text-white transition-colors hover:bg-black/80"
                        @click.stop="removeFile(index)">
                        ✕
                    </button>
                </div>
            </div>
        </div>
    </section>
</template>