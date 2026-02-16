<script setup lang="ts">
import type { authProvider } from './AuthView.vue';
import { onMounted } from 'vue';
import { useRoute } from 'vue-router';

const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
const redirect = import.meta.env.VITE_GOOGLE_REDIRECT_URI;

const emit = defineEmits<{
    (e: 'authenticated', token: string, provider: authProvider): void;
}>();

const route = useRoute();

const loginWithGoogle = () => {
    const state = crypto.randomUUID();
    sessionStorage.setItem("google_oauth_state", state);
    const url =
        `https://accounts.google.com/o/oauth2/v2/auth` +
        `?client_id=${googleClientId}` +
        `&redirect_uri=${encodeURIComponent(redirect)}` +
        `&response_type=code` +
        `&scope=openid email profile` +
        `&state=${state}` +
        `&prompt=select_account`;
    window.location.href = url;
};

onMounted(() => {
    const code = route.query.code as string;
    const state = route.query.state as string;

    const storedState = sessionStorage.getItem("google_oauth_state");

    if (code && state === storedState) {
        sessionStorage.removeItem("google_oauth_state");
        emit("authenticated", code, "google");
    }
});


</script>

<template>
    <button @click="loginWithGoogle" class="group w-full flex items-center justify-center gap-3
               bg-white dark:bg-neutral-800
               border border-gray-300 dark:border-neutral-700
               text-gray-700 dark:text-gray-200
               font-medium
               py-3 px-4
               rounded-xl
               shadow-sm
               transition-all duration-200
               hover:shadow-md hover:-translate-y-[1px]
               active:translate-y-0 active:scale-[0.98]
               focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
        <!-- Proper Google G icon -->
        <svg class="w-5 h-5" viewBox="0 0 48 48">
            <path fill="#4285F4"
                d="M24 9.5c3.54 0 6.73 1.22 9.23 3.6l6.9-6.9C35.82 2.4 30.28 0 24 0 14.82 0 6.92 5.34 3.2 13.09l8.04 6.24C13.27 13.2 18.15 9.5 24 9.5z" />
            <path fill="#34A853"
                d="M46.145 24.5c0-1.63-.145-3.2-.41-4.7H24v9h12.4c-.53 2.85-2.13 5.27-4.53 6.9l7.04 5.47C43.91 37.5 46.145 31.5 46.145 24.5z" />
            <path fill="#FBBC05"
                d="M11.24 28.33A14.49 14.49 0 019.5 24c0-1.5.27-2.95.74-4.33l-8.04-6.24A23.93 23.93 0 000 24c0 3.93.94 7.65 2.6 10.93l8.64-6.6z" />
            <path fill="#EA4335"
                d="M24 48c6.48 0 11.92-2.14 15.89-5.82l-7.04-5.47c-2 1.35-4.55 2.14-8.85 2.14-5.85 0-10.73-3.7-12.5-8.83l-8.64 6.6C6.92 42.66 14.82 48 24 48z" />
        </svg>

        <span class="tracking-wide">
            Continue with Google
        </span>
    </button>
</template>
