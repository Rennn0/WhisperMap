<script setup lang="ts">
import { ExclamationTriangleIcon } from '@heroicons/vue/24/outline';
defineProps<{ error: string }>();
</script>

<template>
    <div
        class="flex-1 w-full min-h-0 flex flex-col items-center justify-center bg-red-50 border border-red-200 rounded-md p-4">
        <ExclamationTriangleIcon class="h-8 w-8 text-red-600 mb-2" />

        <p
            class="w-full text-xs text-gray-600 mt-1 text-center overflow-hidden break-words whitespace-normal line-clamp-3">
            {{ error }}
        </p>
    </div>
</template>
