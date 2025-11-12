<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import GeorgiaFlag from '../freestyle/TablerGeorgiaFlag.vue';
import RussiaFlag from '../freestyle/TablerRussiaFlag.vue';
import UsaFlag from '../freestyle/TablerUsaFlag.vue';

const { locale } = useI18n();
const languages = [
    { code: 'ka', label: 'KA', flag: GeorgiaFlag },
    { code: 'eng', label: 'EN', flag: UsaFlag },
    { code: 'rus', label: 'RU', flag: RussiaFlag },
];

const setLanguage = (lang: string) => {
    locale.value = lang;
    localStorage.setItem('lang', lang);
};
</script>

<template>
    <div class="mt-6 border-t border-subtle pt-4">
        <span class="text-sm font-medium text-muted mb-2 block">{{ $t('sidebar.lang') }}</span>
        <div class="flex gap-2">
            <button v-for="lang in languages" :key="lang.code" @click="setLanguage(lang.code)" :class="[
                'flex items-center gap-2 px-3 py-1 rounded-md border-2 transition-colors text-text',
                locale === lang.code
                    ? 'border-primary'
                    : 'hover:shadow-md hover:bg-subtle'
            ]">
                <component :is="lang.flag" class="w-5 h-5" />
                <span>{{ lang.label }}</span>
            </button>
        </div>
    </div>
</template>