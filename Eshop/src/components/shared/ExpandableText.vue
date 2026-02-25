<script setup lang="ts">
import { computed, ref } from 'vue';

interface Props {
    text: string;
    size?: "small" | "large",
    strLength?: number
}

const props = withDefaults(defineProps<Props>(), {
    size: 'small',
    strLength: 32
});

const expanded = ref(false);
const normalizedText = computed(() => props.text ?? "");
const isTruncated = computed(() => normalizedText.value.length > props.strLength);

const textSizeClass = computed(() => props.size === "small" ? "text-sm" : "text-lg");
const displayedText = computed(() => {
    if (!isTruncated.value || expanded.value) {
        return normalizedText.value;
    }
    return `${normalizedText.value.slice(0, props.strLength)}...`
})
const toggle = () => {
    expanded.value = !expanded.value;
}
</script>
<template>
    <div>
        <p :class="[
            textSizeClass,
            'leading-relaxed whitespace-pre-line break-words overflow-hidden'
        ]">
            {{ displayedText }}

            <span v-if="isTruncated" @click="toggle" class="ml-1 font-medium text-primary cursor-pointer">
                {{ expanded ? 'Show less' : 'Show more' }}
            </span>
        </p>
    </div>
</template>