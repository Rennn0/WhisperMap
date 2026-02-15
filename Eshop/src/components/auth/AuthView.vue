<script setup lang="ts">
import GoogleAuthComponent from './GoogleAuthComponent.vue';
import { sendAuthToken } from '../../services/http';
import GithubAuthComponent from './GithubAuthComponent.vue';

export type authProvider = "google" | "github";

const onAuthenticated = (token: string, provider: authProvider) => {
    sendAuthToken(token, provider).request.then(() => {
        window.location.href = "/";
    });
}
</script>
<template>
    <title>Auth</title>

    <div class="min-h-screen flex items-center justify-center bg-surface text-text px-4">
        <div class="w-full max-w-md">
            <div class="bg-white dark:bg-neutral-900 shadow-xl rounded-2xl p-8 space-y-6">

                <div class="text-center space-y-2">
                    <h1 class="text-2xl font-semibold tracking-tight">
                        Welcome Back
                    </h1>
                    <p class="text-sm text-gray-500 dark:text-gray-400">
                        Sign in to continue
                    </p>
                </div>

                <div class="space-y-4">
                    <GoogleAuthComponent @authenticated="onAuthenticated" />
                    <div class="flex items-center gap-3">
                        <div class="flex-1 h-px bg-gray-200 dark:bg-neutral-700"></div>
                        <span class="text-xs text-gray-400">OR</span>
                        <div class="flex-1 h-px bg-gray-200 dark:bg-neutral-700"></div>
                    </div>
                    <GithubAuthComponent @authenticated="onAuthenticated" />
                </div>

                <div class="text-center text-xs text-gray-400 pt-4">
                    Secure authentication powered by OAuth 2.0
                </div>

            </div>
        </div>
    </div>
</template>
