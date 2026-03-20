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
    if (options.onOpen)
        source.onopen = options.onOpen;
    if (options.onError)
        source.onerror = options.onError;
    return source;
}