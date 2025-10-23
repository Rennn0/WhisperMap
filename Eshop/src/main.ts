import { createApp, ref, toValue, watchEffect, type Ref } from 'vue'
import './style.css'
import App from './App.vue'

createApp(App).mount('#app')

export function useLoader<TData>(dependency: Ref<any>, loaderFunc: (arg: any) => Promise<TData>) {
    const data: Ref<TData | undefined> = ref();
    const error: Ref<Error | undefined> = ref();

    watchEffect(() => {
        data.value = undefined;
        error.value = undefined;
        loaderFunc(toValue(dependency)).then(x => data.value = x).catch(e => error.value = new Error(JSON.stringify(e)));
    });
    return { data, error };
}

export function useLoaderWithRetry<TData>(dependency: Ref<any>, loaderFunc: (arg: any) => Promise<TData>, retry: number, delayMs: number) {
    const data: Ref<TData | undefined> = ref();
    const error: Ref<Error | undefined> = ref();

    const jitter = 1.5;
    let runId = 0;

    watchEffect((onInvalidate) => {
        data.value = undefined;
        error.value = undefined;
        const thisRun = ++runId;
        let cancelled = false;
        onInvalidate(() => { cancelled = true; });

        const attempt = async (remaining: number, backoffMs: number) => {
            try {
                const result = await loaderFunc(toValue(dependency));
                if (cancelled || thisRun !== runId) return;
                console.log(thisRun);
                data.value = result;
            } catch (e) {
                if (cancelled || thisRun !== runId) return;
                if (remaining > 0) {
                    await new Promise(res => setTimeout(res, backoffMs));
                    if (cancelled || thisRun !== runId) return;
                    return attempt(remaining - 1, backoffMs * jitter);
                } else {
                    error.value = new Error(JSON.stringify(e));
                }
            }
        };

        void attempt(retry, delayMs);
    });

    return { data, error };
}

