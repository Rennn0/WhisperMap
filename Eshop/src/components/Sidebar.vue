<script setup lang="ts">
import { XCircleIcon, GiftIcon, ShoppingBagIcon, Cog6ToothIcon } from '@heroicons/vue/24/solid';
import type { SidebarOptions } from '../types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ (e: 'close'): void; (e: 'select', option: string): void }>();

const options: SidebarOptions[] = [
    { title: "Products", key: "products", icon: GiftIcon },
    { title: "Orders", key: "orders", icon: ShoppingBagIcon },
    { title: "Settings", key: "settings", icon: Cog6ToothIcon }
];

const selectOption = (key: string) => emit('select', key);

</script>

<template>
    <div>
        <transition name="fade">
            <div v-if="props.open" class="fixed inset-0 bg-black/40 z-40" @click="$emit('close')"></div>
        </transition>

        <transition enter-active-class="transform transition duration-300 ease-out" enter-from-class="-translate-x-full"
            enter-to-class="translate-x-0" leave-active-class="transform transition duration-200 ease-in"
            leave-from-class="translate-x-0" leave-to-class="-translate-x-full">
            <aside v-if="props.open" class="fixed inset-y-0 left-0 w-64 bg-surface border-r border-subtle z-50 p-4">
                <div class="flex mb-4">
                    <h2 class="font-semibold">ხათის საჩუქრების ზარდახშა</h2>
                    <button @click="$emit('close')"
                        class="w-8 h-8 rounded-md transition-colors flex items-center justify-center">
                        <XCircleIcon />
                    </button>
                </div>

                <nav class="space-y-2">
                    <button v-for="opt in options" :key="opt.key" @click="selectOption(opt.key)"
                        class="w-full text-left flex items-center gap-3 px-3 py-2 rounded-md hover:bg-subtle cursor-pointer transition-colors">
                        <component :is="opt.icon" class="w-5 h-5" />
                        <span class="text-sm">{{ opt.title }}</span>
                    </button>
                </nav>
            </aside>
        </transition>
    </div>
</template>

<style scoped></style>