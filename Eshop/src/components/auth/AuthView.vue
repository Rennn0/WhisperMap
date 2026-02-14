<script setup lang="ts">
import GoogleAuthComponent from './GoogleAuthComponent.vue';
import { sendGoogleToken } from '../../services/http';

const onAuthenticated = (token: string) => {
    sendGoogleToken(token).request.then(() => {
        window.location.href = "/";
    });
}
</script>

<template>
    <title>Auth</title>
    <div class="min-h-screen flex items-center justify-center bg-surface text-text">
        <div class="max-w-6xl mx-auto p-4">
            <GoogleAuthComponent v-on:authenticated="onAuthenticated" />
        </div>
    </div>
</template>
