export default {
    async fetch(request, env) {
        const url = new URL(request.url);

        if (!url.pathname.startsWith("/cl/")) {
            return new Response(JSON.stringify({ error: "Not allowed" }), {
                status: 403,
                headers: { "Content-Type": "application/json" },
            });
        }

        const backendUrl = env.XC_BACKEND_URL + url.pathname.slice(4) + url.search;
        console.log(backendUrl);
        const headers = new Headers(request.headers);
        headers.set("x-api-key", env.XC_API_KEY);

        const ip = request.headers.get("cf-connecting-ip") || "unknown";
        headers.set("x-public-ip", ip);

        return fetch(backendUrl, {
            method: request.method,
            headers,
            body:
                request.method !== "GET" && request.method !== "HEAD"
                    ? request.body
                    : null,
            redirect: "manual",
        });
    },
};
