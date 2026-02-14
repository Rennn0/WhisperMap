<script setup lang="ts">
import { GoogleLogin, type CallbackTypes } from 'vue3-google-login';

const emit = defineEmits<{
    (e: 'authenticated', token: string): void;
}>();

const handleCallback = (response: CallbackTypes.CredentialPopupResponse) => {
    if (!response.credential) return;
    emit("authenticated", response.credential);
};
</script>

<template>
    <div class="flex justify-center">
        <GoogleLogin :callback="handleCallback" />
    </div>
</template>
