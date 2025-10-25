<script setup lang="ts">
const settings = [
    { key: 'profile', label: 'Profile' },
    { key: 'billing', label: 'Billing' },
    { key: 'prefs', label: 'Preferences' },
];
</script>

<template>
    <div class="space-y-4">
        <h2 class="text-xl font-semibold mb-2">Settings</h2>

        <div class="grid gap-2">
            <button v-for="s in settings" :key="s.key"
                class="w-full text-left px-3 py-2 rounded-md hover:bg-subtle transition-colors">
                {{ s.label }}
            </button>
        </div>
    </div>
</template>

<style scoped></style>