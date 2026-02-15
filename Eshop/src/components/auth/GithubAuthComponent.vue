<script setup lang="ts">
import { onMounted } from 'vue';
import type { authProvider } from './AuthView.vue';
import { useRoute } from 'vue-router';

const githubClientId = import.meta.env.VITE_GITHUB_CLIENT_ID;
const redirect = import.meta.env.VITE_GITHUB_REDIRECT_URI;

const route = useRoute();

const emit = defineEmits<{
    (e: 'authenticated', token: string, provider: authProvider): void;
}>();

const loginWithGithub = () => {
    const state = crypto.randomUUID();
    sessionStorage.setItem("gh_oauth_state", state);
    const url =
        `https://github.com/login/oauth/authorize` +
        `?client_id=${githubClientId}` +
        `&redirect_uri=${encodeURIComponent(redirect)}` +
        `&scope=user:email` +
        `&state=${state}`;
    window.location.href = url;
};

onMounted(() => {
    const code = route.query.code as string;
    const state = route.query.state as string;

    const storedState = sessionStorage.getItem("gh_oauth_state");

    if (code && state === storedState) {
        sessionStorage.removeItem("gh_oauth_state");
        emit("authenticated", code, "github");
    }
});
</script>

<template>
    <button @click="loginWithGithub" class="w-full flex items-center justify-center gap-3
               bg-black text-white
               font-medium
               py-3 px-4
               rounded-xl
               transition-all duration-200
               hover:bg-neutral-800
               active:scale-[0.98]
               focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-black">
        <svg viewBox="0 0 24 24" class="w-5 h-5 fill-current">
            <path d="M12 0C5.37 0 0 5.37 0 12c0 5.3 3.438 9.8 8.207 11.385.6.113.793-.262.793-.583 
                     0-.287-.01-1.044-.016-2.05-3.338.726-4.042-1.61-4.042-1.61
                     -.546-1.387-1.333-1.757-1.333-1.757-1.089-.744.083-.729.083-.729
                     1.205.084 1.838 1.237 1.838 1.237
                     1.07 1.835 2.807 1.304 3.492.997
                     .108-.776.418-1.304.762-1.604
                     -2.665-.303-5.467-1.332-5.467-5.93
                     0-1.31.468-2.382 1.235-3.222
                     -.124-.303-.536-1.524.117-3.176 0 0 1.008-.322 3.3 1.23
                     .957-.266 1.983-.399 3.003-.404
                     1.02.005 2.047.138 3.006.404
                     2.29-1.552 3.296-1.23 3.296-1.23
                     .655 1.653.243 2.874.12 3.176
                     .77.84 1.233 1.912 1.233 3.222
                     0 4.61-2.807 5.624-5.48 5.921
                     .43.37.823 1.096.823 2.21
                     0 1.596-.014 2.884-.014 3.277
                     0 .324.192.699.8.58
                     C20.565 21.795 24 17.296 24 12
                     24 5.37 18.63 0 12 0z" />
        </svg>

        <span>Continue with GitHub</span>
    </button>
</template>
