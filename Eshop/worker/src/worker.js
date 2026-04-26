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

async function handleProxy(request, env) {
    const url = new URL(request.url);
    const backendUrl = env.XC_BACKEND_URL + url.pathname.slice(4) + url.search;

    const headers = new Headers(request.headers);
    headers.set("x-api-key", env.XC_API_KEY);
    headers.set("x-public-ip", request.headers.get("cf-connecting-ip") || "unknown");

    const response = await fetch(backendUrl, {
        method: request.method,
        headers,
        body:
            request.method !== "GET" && request.method !== "HEAD"
                ? request.body
                : null,
        redirect: "manual"
    });

    const newHeaders = new Headers(response.headers);

    newHeaders.delete("content-security-policy");
    newHeaders.delete("content-security-policy-report-only");

    newHeaders.set(
        "content-security-policy",
        "default-src 'self'; connect-src 'self' https:;"
    );

    return new Response(response.body, {
        status: response.status,
        statusText: response.statusText,
        headers: newHeaders
    });
}

async function handleAudit(request, env) {
    const body = await request.json();
    await env.xati_db.prepare(
        `INSERT INTO RequestLogs (RequestId, RequestBody, ResponseBody, Status,Route)
           VALUES (?, ?, ?, ?, ?)`
    )
        .bind(
            body.requestId,
            body.requestBody,
            body.responseBody,
            body.status,
            body.route
        )
        .run();

    return new Response(JSON.stringify({ ok: true }), {
        headers: { "Content-Type": "application/json" }
    });
}