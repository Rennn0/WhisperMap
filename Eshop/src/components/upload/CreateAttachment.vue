<script setup lang="ts">
import { ref, watch, computed } from 'vue';

const props = defineProps<{
    existingFiles?: File[];
}>();

const emit = defineEmits<{
    (e: 'attachments-selected', files: File[]): void;
}>();

// Local state
const files = ref<File[]>(props.existingFiles ? [...props.existingFiles] : []);

// Sync if parent updates
watch(() => props.existingFiles, (v) => {
    files.value = v ? [...v] : [];
});

// Add / append new files
const onFilesChange = (e: Event) => {
    const input = e.target as HTMLInputElement;
    const newFiles = input.files ? Array.from(input.files) : [];
    files.value = [...files.value, ...newFiles];
    emit('attachments-selected', files.value);
};

// Remove file
const removeFile = (index: number) => {
    files.value.splice(index, 1);
    emit('attachments-selected', files.value);
};

// Tap-to-swap logic (works on mobile)
const selectedIndex = ref<number | null>(null);

const handleSelect = (index: number) => {
    if (selectedIndex.value === null) {
        selectedIndex.value = index; // first tap selects
    } else if (selectedIndex.value === index) {
        selectedIndex.value = null; // deselect if same
    } else {
        // swap files
        const temp = files.value[selectedIndex.value];
        files.value[selectedIndex.value] = files.value[index]!;
        files.value[index] = temp!;
        selectedIndex.value = null;
        emit('attachments-selected', files.value);
    }
};

const previews = computed(() =>
    files.value.map(file => ({
        file,
        url: URL.createObjectURL(file),
        isImage: file.type.startsWith('image/'),
        isVideo: file.type.startsWith('video/')
    }))
);
</script>

<template>
    <div class="p-4 rounded-lg shadow-sm border border-gray-300/40 bg-surface">
        <label
            class="flex flex-col items-center justify-center border-2 border-dashed border-gray-300/60 rounded-lg p-6 cursor-pointer hover:border-primary transition text-center">
            <span class="text-text/70 mb-2">Select images or videos</span>
            <input type="file" multiple accept="image/*,video/*" class="hidden" @change="onFilesChange" />
        </label>

        <div v-if="previews.length" class="mt-4 grid grid-cols-2 sm:grid-cols-3 gap-3">
            <div v-for="(p, index) in previews" :key="p.url"
                class="relative aspect-square overflow-hidden rounded-lg bg-subtle select-none cursor-pointer transition-transform duration-200"
                :class="{ 'scale-105 ring-4 ring-primary': selectedIndex === index }" @click="handleSelect(index)">

                <img v-if="p.isImage" :src="p.url" class="object-cover w-full h-full" />
                <video v-else :src="p.url" class="object-cover w-full h-full" muted controls />

                <!-- Remove button -->
                <button
                    class="absolute top-1 right-1 bg-black/60 text-white text-xs px-2 py-1 rounded hover:bg-black/80"
                    @click.stop="removeFile(index)">
                    ✕
                </button>
            </div>
        </div>
    </div>
</template>
