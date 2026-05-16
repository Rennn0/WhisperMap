<script setup lang="ts">
import { computed, onUnmounted, ref } from 'vue';

type SessionMode = 'idle' | 'working' | 'resting';

interface SetRecord {
    setNumber: number;
    workTime: number;
    restTime: number;
}

const mode = ref<SessionMode>('idle');
const timerSeconds = ref(0);
const completedSets = ref(0);
const setHistory = ref<SetRecord[]>([]);

let intervalId: number | null = null;

// Computed properties
const isWorking = computed(() => mode.value === 'working');
const isResting = computed(() => mode.value === 'resting');
const isIdle = computed(() => mode.value === 'idle');

const displayTimer = computed(() => {
    const minutes = Math.floor(timerSeconds.value / 60)
        .toString()
        .padStart(2, '0');
    const seconds = (timerSeconds.value % 60)
        .toString()
        .padStart(2, '0');
    return `${minutes}:${seconds}`;
});

const buttonConfig = computed(() => {
    const configs = {
        idle: {
            text: 'START SET',
            color: 'from-green-500 to-green-600',
            icon: '▶',
        },
        working: {
            text: 'DONE SET',
            color: 'from-green-500 to-green-600',
            icon: '⏸',
        },
        resting: {
            text: 'START SET',
            color: 'from-blue-500 to-blue-600',
            icon: '✓',
        },
    };
    return configs[mode.value];
});

const statusInfo = computed(() => {
    const statuses = {
        idle: { label: 'Ready', icon: '●', color: 'text-gray-500' },
        working: { label: 'Workout Active', icon: '●', color: 'text-green-500' },
        resting: { label: 'Rest Active', icon: '●', color: 'text-blue-500' },
    };
    return statuses[mode.value];
});

const timerBg = computed(() => {
    const backgrounds = {
        idle: 'from-gray-50 to-gray-100 border-gray-200',
        working: 'from-green-50 to-green-100 border-green-300',
        resting: 'from-blue-50 to-blue-100 border-blue-300',
    };
    return backgrounds[mode.value];
});

// Timer functions
function startTimer(): void {
    if (intervalId !== null) clearInterval(intervalId);
    intervalId = window.setInterval(() => {
        timerSeconds.value += 1;
    }, 1000);
}

function stopTimer(): void {
    if (intervalId !== null) {
        clearInterval(intervalId);
        intervalId = null;
    }
}

function resetTimer(): void {
    timerSeconds.value = 0;
}

// Action handlers
function handleMainAction(): void {
    if (isIdle.value) {
        // IDLE → WORKING
        resetTimer();
        mode.value = 'working';
        startTimer();
    } else if (isWorking.value) {
        // WORKING → RESTING
        stopTimer();
        const workTime = timerSeconds.value;
        resetTimer();
        mode.value = 'resting';
        startTimer();

        // Store work time for later
        sessionStorage.setItem('lastWorkTime', workTime.toString());
    } else if (isResting.value) {
        // RESTING → WORKING (save history, start new set)
        stopTimer();
        const restTime = timerSeconds.value;
        const workTime = parseInt(sessionStorage.getItem('lastWorkTime') || '0');

        completedSets.value += 1;
        setHistory.value.unshift({
            setNumber: completedSets.value,
            workTime,
            restTime,
        });

        resetTimer();
        mode.value = 'working';
        startTimer();
    }
}

function handleReset(): void {
    stopTimer();
    resetTimer();
    mode.value = 'idle';
    completedSets.value = 0;
    setHistory.value = [];
    sessionStorage.removeItem('lastWorkTime');
}

onUnmounted(() => {
    stopTimer();
});
</script>

<template>
    <div
        class="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 p-3 sm:p-6 flex items-center justify-center">
        <div class="w-full max-w-2xl">

            <!-- Header -->
            <div class="mb-6 sm:mb-12 text-center px-2">
                <h1
                    class="text-3xl sm:text-5xl font-black bg-gradient-to-r from-green-400 to-blue-400 bg-clip-text text-transparent mb-1 sm:mb-2">
                    WORKOUT TIMER
                </h1>
                <p class="text-gray-400 text-sm sm:text-lg">Track your sets and rest intervals</p>
            </div>

            <!-- Main Timer Panel -->
            <div class="bg-white rounded-2xl sm:rounded-3xl shadow-2xl overflow-hidden mb-6 sm:mb-8">

                <!-- Status Bar -->
                <div
                    class="bg-gradient-to-r from-gray-100 to-gray-50 px-4 sm:px-8 py-3 sm:py-4 flex items-center justify-between border-b border-gray-200 gap-2 sm:gap-0">
                    <div class="flex items-center gap-2 sm:gap-3 min-w-0">
                        <span :class="['text-lg sm:text-2xl animate-pulse flex-shrink-0', statusInfo.color]">
                            {{ statusInfo.icon }}
                        </span>
                        <span class="font-semibold text-gray-700 text-xs sm:text-base truncate">
                            {{ statusInfo.label }}
                        </span>
                    </div>
                    <span class="text-xs sm:text-sm font-bold text-gray-500 uppercase tracking-wider flex-shrink-0">
                        Set {{ completedSets + 1 }}
                    </span>
                </div>

                <!-- Timer Display -->
                <div :class="['bg-gradient-to-br p-6 sm:p-12 transition-all duration-300 border-2', timerBg]">
                    <div class="text-center">
                        <div
                            class="text-xs sm:text-sm font-bold text-gray-500 uppercase tracking-[0.15em] sm:tracking-[0.25em] mb-3 sm:mb-6">
                            {{ isWorking ? 'Work Time' : isResting ? 'Rest Time' : 'Ready' }}
                        </div>
                        <div
                            class="text-6xl sm:text-9xl font-black text-gray-900 tracking-tight font-mono tabular-nums">
                            {{ displayTimer }}
                        </div>
                    </div>
                </div>

                <!-- Action Buttons -->
                <div class="px-4 sm:px-8 py-4 sm:py-8 bg-white flex gap-2 sm:gap-4 flex-col sm:flex-row">
                    <!-- Main Action Button -->
                    <button @click="handleMainAction"
                        :class="['flex-1 rounded-lg sm:rounded-xl px-4 sm:px-8 py-4 sm:py-6 font-bold text-sm sm:text-lg text-white transition-all duration-200 hover:shadow-lg hover:-translate-y-1 active:translate-y-0 bg-gradient-to-r flex items-center justify-center gap-2 sm:gap-3 min-h-12 sm:min-h-auto', buttonConfig.color]">
                        <span class="text-lg sm:text-xl">{{ buttonConfig.icon }}</span>
                        <span>{{ buttonConfig.text }}</span>
                    </button>

                    <!-- Reset Button -->
                    <button @click="handleReset"
                        class="rounded-lg sm:rounded-xl px-4 sm:px-8 py-4 sm:py-6 font-bold text-gray-700 bg-gray-100 hover:bg-gray-200 transition-colors duration-200 text-sm sm:text-lg min-h-12 sm:min-h-auto">
                        ↻ RESET
                    </button>
                </div>
            </div>

            <!-- Set History Table -->
            <div class="bg-white rounded-2xl sm:rounded-3xl shadow-2xl overflow-hidden">
                <div
                    class="px-4 sm:px-8 py-3 sm:py-6 border-b border-gray-200 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-2 sm:gap-0 bg-gradient-to-r from-gray-50 to-gray-100">
                    <h2 class="text-lg sm:text-xl font-bold text-gray-900">Set History</h2>
                    <span class="text-xs sm:text-sm font-bold text-gray-500 bg-white px-3 py-1 rounded-full">
                        {{ setHistory.length }} sets
                    </span>
                </div>

                <div class="overflow-x-auto">
                    <table class="w-full text-sm sm:text-base">
                        <thead>
                            <tr class="border-b border-gray-200 bg-gray-50">
                                <th
                                    class="px-3 sm:px-8 py-3 sm:py-4 text-left text-xs font-bold text-gray-600 uppercase tracking-wider">
                                    Set #
                                </th>
                                <th
                                    class="px-3 sm:px-8 py-3 sm:py-4 text-left text-xs font-bold text-green-600 uppercase tracking-wider">
                                    Work
                                </th>
                                <th
                                    class="px-3 sm:px-8 py-3 sm:py-4 text-left text-xs font-bold text-blue-600 uppercase tracking-wider">
                                    Rest
                                </th>
                                <th
                                    class="px-3 sm:px-8 py-3 sm:py-4 text-right text-xs font-bold text-gray-600 uppercase tracking-wider">
                                    Total
                                </th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="(set, idx) in setHistory" :key="idx"
                                class="border-b border-gray-100 hover:bg-gray-50 transition-colors">
                                <td class="px-3 sm:px-8 py-3 sm:py-5 font-bold text-gray-900">
                                    #{{ set.setNumber }}
                                </td>
                                <td class="px-3 sm:px-8 py-3 sm:py-5 font-mono font-bold text-green-600">
                                    {{ formatTime(set.workTime) }}
                                </td>
                                <td class="px-3 sm:px-8 py-3 sm:py-5 font-mono font-bold text-blue-600">
                                    {{ formatTime(set.restTime) }}
                                </td>
                                <td class="px-3 sm:px-8 py-3 sm:py-5 text-right font-mono font-semibold text-gray-700">
                                    {{ formatTime(set.workTime + set.restTime) }}
                                </td>
                            </tr>
                            <tr v-if="setHistory.length === 0">
                                <td colspan="4"
                                    class="px-3 sm:px-8 py-8 sm:py-16 text-center text-gray-400 font-medium text-sm sm:text-base">
                                    No sets completed yet. Start your first set!
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <!-- Summary Stats -->
                <div v-if="setHistory.length > 0"
                    class="grid grid-cols-3 gap-0 bg-gradient-to-r from-gray-50 to-gray-100 border-t border-gray-200">
                    <div class="px-3 sm:px-8 py-3 sm:py-5 text-center border-r border-gray-200">
                        <div class="text-xs font-bold text-gray-600 uppercase tracking-wider mb-1 sm:mb-2">
                            Total Work
                        </div>
                        <div class="text-lg sm:text-2xl font-black text-green-600">
                            {{formatTime(setHistory.reduce((sum, s) => sum + s.workTime, 0))}}
                        </div>
                    </div>
                    <div class="px-3 sm:px-8 py-3 sm:py-5 text-center border-r border-gray-200">
                        <div class="text-xs font-bold text-gray-600 uppercase tracking-wider mb-1 sm:mb-2">
                            Total Rest
                        </div>
                        <div class="text-lg sm:text-2xl font-black text-blue-600">
                            {{formatTime(setHistory.reduce((sum, s) => sum + s.restTime, 0))}}
                        </div>
                    </div>
                    <div class="px-3 sm:px-8 py-3 sm:py-5 text-center">
                        <div class="text-xs font-bold text-gray-600 uppercase tracking-wider mb-1 sm:mb-2">
                            Total Time
                        </div>
                        <div class="text-lg sm:text-2xl font-black text-gray-900">
                            {{formatTime(setHistory.reduce((sum, s) => sum + s.workTime + s.restTime, 0))}}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
function formatTime(totalSeconds: number): string {
    const minutes = Math.floor(totalSeconds / 60)
        .toString()
        .padStart(2, '0');
    const seconds = (totalSeconds % 60)
        .toString()
        .padStart(2, '0');
    return `${minutes}:${seconds}`;
}
</script>

<style scoped>
/* Smooth transitions for mode changes */
:deep() {
    transition-property: all;
    transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
    transition-duration: 300ms;
}
</style>