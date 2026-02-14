<script setup lang="ts">
import { onMounted, provide, readonly, ref } from 'vue';
import { userInfoInjectionKey } from './injectionKeys';
import type { UserInfo } from './types';
import { getMe, getSession } from './services/http';
import TablerLoaderBlockWave from './components/freestyle/TablerLoaderBlockWave.vue';

const sessionEnabled = ref(false);
const userInfo = ref<UserInfo>();
provide(userInfoInjectionKey, readonly(userInfo));

onMounted(() => {
  getSession().request.then(() => {
    getMe().request.then((v) => {
      if (!v) return;
      userInfo.value = v;
      sessionEnabled.value = true;
    })
  })
  const saved = localStorage.getItem('theme') || 'light';
  document.documentElement.classList.add(saved);
})
</script>
<template>
  <RouterView v-if="sessionEnabled" />
  <div v-else class="flex items-center justify-center min-h-screen">
    <TablerLoaderBlockWave class="w-16 md:w-28 lg:w-44 h-16 md:h-28 lg:h-44" />
  </div>
</template>