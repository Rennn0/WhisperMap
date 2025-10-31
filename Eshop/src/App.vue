<script setup lang="ts" async>
import { ref, onMounted, provide, readonly, onActivated, onUpdated, onUnmounted } from 'vue';
import NavBar from './components/Navbar.vue';
import Sidebar from './components/Sidebar.vue';
import { CurrentViewSelection } from './types';
import { titleInjectionKey } from './injectionKeys';

//#region variables and providers

const title = ref("საჩუქრების ზარდახშა");
const sidebarOpen = ref(false);

provide(titleInjectionKey, { title: readonly(title), update: (t: string) => title.value = t });
//#endregion

//#region computed / watchEffect

//#endregion

//#region event hooks

const onMenuToggle = () => sidebarOpen.value = true

const onProfileClick = (): void => { }

const onOptionSelect = (key: CurrentViewSelection) => {
  sidebarOpen.value = false;
  switch (key) {
    case CurrentViewSelection.Product:
      break;

    case CurrentViewSelection.Order:
      break;

    case CurrentViewSelection.Setting:
      break;
  }
}

//#endregion

//#region lifecycle hooks
onActivated(() => { });
onUpdated(() => { });
onMounted(() => { });
onUnmounted(() => { })
//#endregion

</script>

<template>
  <div class="min-h-screen bg-surface text-text transition-colors duration-300">
    <title>{{ title }}</title>

    <NavBar @menu-toggle="onMenuToggle" @profile-click="onProfileClick" />
    <Sidebar :open="sidebarOpen" @close="sidebarOpen = false" @select="onOptionSelect" />

    <main class="max-w-6xl mx-auto p-4">
      <RouterView />
    </main>
  </div>
</template>