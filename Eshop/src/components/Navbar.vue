<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { Bars3Icon, MagnifyingGlassIcon, SunIcon, MoonIcon } from '@heroicons/vue/24/outline';

const emit = defineEmits<{
  (e: 'menu-toggle'): void;
  (e: 'search', value: string): void;
  (e: 'profile-click'): void;
}>();

const query = ref('');

function onInput() {
  emit('search', query.value);
}
function onEnter() {
  emit('search', query.value);
}
function onMenu() {
  emit('menu-toggle');
}
function onProfile() {
  emit('profile-click');
}
</script>


<template>
  <nav class="w-full bg-white/50 border-b border-gray-200">
    <div class="max-w-5xl mx-auto px-3 py-2 flex items-center gap-3">
      <button @click="[onMenu(), onProfile()]"
        class="p-2 rounded-md border border-transparent hover:bg-gray-100 hover:border-gray-400 transition sm:mr-2"
        aria-label="Toggle menu">
        <Bars3Icon class="w-6 h-6 text-gray-700" />
      </button>

      <div class="sm:flex flex-1 justify-center">
        <div class="w-full max-w-xl">
          <div class="relative">
            <span class="absolute inset-y-0 left-3 flex items-center pointer-events-none">
              <MagnifyingGlassIcon class="w-5 h-5 text-gray-400 dark:text-gray-400" />
            </span>
            <input v-model="query" @input="onInput" @keyup.enter="onEnter" type="search" placeholder="Search..."
              class="w-full pl-10 pr-4 py-2 rounded-full border border-gray-200 shadow-sm focus:outline-none focus:ring-1 focus:ring-gray-400"
              aria-label="Search" />
          </div>
        </div>
      </div>
    </div>
  </nav>
</template>

<style scoped></style>