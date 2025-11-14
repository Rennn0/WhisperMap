export default {
    async fetch(request, env) {
        const url = new URL(request.url);

        if (!url.pathname.startsWith("/api/")) {
            return new Response(
                JSON.stringify({ error: "Not allowed" }),
                {
                    status: 403,
                    headers: { "Content-Type": "application/json" }
                }
            );
        }

        const backendUrl =
            env.XC_BACKEND_URL +
            url.pathname.replace("/api", "") +
            url.search;

        const headers = new Headers(request.headers);
        headers.set("x-api-key", env.API_KEY);

        const ip = request.headers.get("cf-connecting-ip") || "unknown";
        headers.set("x-public-ip", ip);

        return fetch(backendUrl, {
            method: request.method,
            headers,
            body: request.body,
            redirect: "manual"
        });
    }
};
