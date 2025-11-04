<template>
    <div class="max-w-5xl mx-auto bg-surface border border-subtle rounded-lg shadow-sm p-4 animate-pulse">
        <div class="flex justify-between items-center mb-4">
            <div class="h-8 w-32 bg-gray-300 rounded"></div>
            <div class="h-4 w-24 bg-gray-300 rounded"></div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

            <div class="lg:col-span-2">
                <div class="w-full h-[520px] bg-gray-300 rounded-md"></div>
            </div>

            <div class="space-y-4 max-h-[520px] overflow-y-auto pr-2">
                <div class="grid grid-cols-4 gap-2">
                    <div v-for="i in 8" :key="i" class="h-20 bg-gray-300 rounded-md"></div>
                </div>

                <div class="space-y-2">
                    <div class="h-4 w-20 bg-gray-300 rounded"></div>
                    <div class="h-3 w-40 bg-gray-300 rounded"></div>
                </div>
            </div>
        </div>

        <div class="mt-6 border-t border-subtle pt-6 grid grid-cols-1 md:grid-cols-3 gap-4">

            <div class="md:col-span-2 space-y-2">
                <div class="h-6 w-1/2 bg-gray-300 rounded"></div>
                <div class="h-3 w-full bg-gray-300 rounded"></div>
                <div class="h-3 w-5/6 bg-gray-300 rounded"></div>
                <div class="h-3 w-4/5 bg-gray-300 rounded"></div>
            </div>

            <div class="md:col-span-1 space-y-4">
                <div class="space-y-1">
                    <div class="h-3 w-16 bg-gray-300 rounded"></div>
                    <div class="h-6 w-20 bg-gray-300 rounded"></div>
                </div>

                <div class="flex gap-2">
                    <div class="flex-1 h-10 bg-gray-300 rounded"></div>
                    <div class="flex-1 h-10 bg-gray-300 rounded"></div>
                </div>
            </div>
        </div>

    </div>
</template>