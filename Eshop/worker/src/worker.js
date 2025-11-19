export default {
    async fetch(request, env) {
        const url = new URL(request.url);
        if (url.pathname === "/cl/audit" && request.method === "POST") {
            return await handleAudit(request, env);
        }

        if (url.pathname.startsWith("/cl/")) {
            return handleProxy(request, env);
        }

        return new Response("Not found", { status: 404 });
    }
}

function handleProxy(request, env) {
    const url = new URL(request.url);
    const backendUrl = env.XC_BACKEND_URL + url.pathname.slice(4) + url.search;
    const headers = new Headers(request.headers);

    headers.set("x-api-key", env.XC_API_KEY);
    headers.set("x-public-ip", request.headers.get("cf-connecting-ip") || "unknown");

    return fetch(backendUrl, {
        method: request.method,
        headers,
        body:
            request.method !== "GET" && request.method !== "HEAD"
                ? request.body
                : null,
        redirect: "manual"
    });
}

async function handleAudit(request, env) {
    const body = await request.json();
    console.log(body)
    await env.xati_db.prepare(
        `INSERT INTO RequestLogs (RequestId, RequestBody, ResponseBody, Status)
           VALUES (?, ?, ?, ?)`
    )
        .bind(
            body.requestId,
            body.requestBody,
            body.responseBody,
            body.status
        )
        .run();

    return new Response(JSON.stringify({ ok: true }), {
        headers: { "Content-Type": "application/json" }
    });
}