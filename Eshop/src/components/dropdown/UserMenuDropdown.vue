<script setup lang="ts">
import BaseDropdown from './BaseDropdown.vue';
import TablerUserIcon from '../freestyle/TablerUserIcon.vue';

const _ = defineProps<{
    picture?: string | null;
}>(); _;

const emit = defineEmits<{
    (e: 'login'): void;
    (e: 'logout'): void;
    (e: 'profile'): void;
}>();
</script>

<template>
    <BaseDropdown :align="'right'">
        <!-- Trigger -->
        <template #trigger="{ toggle }">
            <button @click="toggle()"
                class="flex items-center justify-center w-10 h-10 rounded-md hover:bg-subtle transition-colors duration-200">
                <TablerUserIcon v-if="!picture" class="w-6 h-6" @click.stop="emit('login')" />
                <img v-else :src="picture" referrerpolicy="no-referrer"
                    class="p-1 w-10 h-10 rounded-full object-cover" />
            </button>
        </template>
        <!-- Content -->
        <template #content="{ close }">
            <div v-if="picture">
                <button @click="() => { emit('profile'); close(); }"
                    class="w-full text-left px-3 py-2 hover:bg-subtle text-sm transition-colors">
                    Profile
                </button>

                <button @click="() => { emit('logout'); close(); }"
                    class="w-full text-left px-3 py-2 hover:bg-subtle text-sm text-danger-text transition-colors">
                    Logout
                </button>
            </div>
        </template>
    </BaseDropdown>
</template>
