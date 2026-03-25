<script setup lang="ts">
import BaseDropdown from './BaseDropdown.vue';
import TablerUserIcon from '../freestyle/TablerUserIcon.vue';

defineProps<{
    picture?: string | null;
}>();

const emit = defineEmits<{
    (e: 'login'): void;
    (e: 'logout'): void;
    (e: 'profile'): void;
}>();
</script>

<template>
    <BaseDropdown align="left" widthClass="w-52">
        <template #trigger="{ toggle, open, triggerRef }">
            <button :ref="triggerRef as any" type="button" :aria-expanded="open" aria-haspopup="menu"
                class="flex items-center justify-center w-10 h-10 rounded-md hover:bg-hover transition-colors duration-200 focus:outline-none"
                @click="picture ? toggle() : emit('login')">
                <TablerUserIcon v-if="!picture" class="w-6 h-6" />
                <img v-else :src="picture" referrerpolicy="no-referrer" class="p-1 w-10 h-10 rounded-full object-cover"
                    alt="User avatar" />
            </button>
        </template>

        <template #content="{ close }">
            <div v-if="picture" class="flex flex-col gap-1">
                <button type="button"
                    class="w-full text-left px-3 py-2 rounded-md hover:bg-hover text-sm transition-colors"
                    @click="emit('profile'); close()">
                    Profile
                </button>

                <button type="button"
                    class="w-full text-left px-3 py-2 rounded-md hover:bg-danger-bg text-sm text-danger-text transition-colors"
                    @click="emit('logout'); close()">
                    Logout
                </button>
            </div>
        </template>
    </BaseDropdown>
</template>