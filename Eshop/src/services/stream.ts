const host = import.meta.env.VITE_REALTIME_HOST;
export const eventStream = (options: {
    url: string,
    callback: (event: any) => void,
    onOpen?: () => void,
    onError?: (err: any) => void,
    streamId?: string
}) => {
    const source = new EventSource(`${host}${options.url}${options.streamId ? `?sid=${options.streamId}` : ''}`);
    source.onmessage = options.callback;

    source.onerror = (e) => {
        console.error("[SSE] err", e, source?.readyState)
        options.onError?.(e);
    }

    source.onopen = () => {
        console.log("[SSE] opened", new Date().toISOString())
        options.onOpen?.();
    }

    return source;
}