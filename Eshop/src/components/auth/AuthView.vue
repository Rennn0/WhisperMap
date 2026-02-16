<script setup lang="ts">
import GoogleAuthComponent from './GoogleAuthComponent.vue';
import { sendAuthToken } from '../../services/http';
import GithubAuthComponent from './GithubAuthComponent.vue';
import { onMounted, ref } from 'vue';
import TablerLoaderBlockWave from '../freestyle/TablerLoaderBlockWave.vue';

export type authProvider = "google" | "github";
const authProgressKey = "__xc_apk";
const waitingForBackend = ref(false);

const onAuthenticated = (token: string, provider: authProvider) => {
    if (waitingForBackend.value) return;

    waitingForBackend.value = true;
    sessionStorage.setItem(authProgressKey, Date.now().toString());

    sendAuthToken(token, provider).request.then(() => {
        sessionStorage.removeItem(authProgressKey);
        window.location.href = "/";
    }).catch(() => {
        waitingForBackend.value = false;
        sessionStorage.removeItem(authProgressKey);
    });
}

onMounted(() => {
    const value = sessionStorage.getItem(authProgressKey);
    if (value) {
        const startedAt = Number(value);
        if (Date.now() - startedAt < 60_000) {
            waitingForBackend.value = true;
        } else {
            sessionStorage.removeItem(authProgressKey);
        }
    }
})
</script>

<template>
    <title>Auth</title>

    <div class="min-h-screen flex items-center justify-center bg-surface text-text px-4 transition-colors">
        <div class="w-full max-w-md">

            <!-- Card -->
            <div v-if="!waitingForBackend"
                class="bg-surface border border-subtle shadow-xl rounded-2xl p-8 space-y-6 transition-colors">
                <!-- Header -->
                <div class="text-center space-y-2">
                    <h1 class="text-2xl font-semibold tracking-tight">
                        Welcome Back
                    </h1>
                    <p class="text-sm opacity-70">
                        Sign in to continue
                    </p>
                </div>

                <!-- Providers -->
                <div class="space-y-4">
                    <GoogleAuthComponent @authenticated="onAuthenticated" />

                    <!-- Divider -->
                    <div class="flex items-center gap-3">
                        <div class="flex-1 h-px bg-subtle"></div>
                        <span class="text-xs opacity-60">OR</span>
                        <div class="flex-1 h-px bg-subtle"></div>
                    </div>

                    <GithubAuthComponent @authenticated="onAuthenticated" />
                </div>

                <!-- Footer -->
                <div class="text-center text-xs opacity-60 pt-4">
                    Secure authentication powered by OAuth 2.0
                </div>
            </div>

            <!-- Loader State -->
            <div v-else
                class="flex flex-col items-center justify-center bg-surface border border-subtle shadow-xl rounded-2xl p-10">
                <TablerLoaderBlockWave class="w-16 h-16" />
                <p class="mt-4 text-sm opacity-70">
                    Completing authentication...
                </p>
            </div>

        </div>
    </div>
</template>
