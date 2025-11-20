import ky, { type Options } from "ky";
import type { UserInfo, Product, UploadableProduct, ApiMeta, AuditLog } from "../types";

const headers = new Headers();
headers.set("content-type", "application/json");
const auditClient = ky.create({ prefixUrl: "cl" })
const httpClint = ky.create({
    headers,
    prefixUrl: "cl",
    credentials: "include",
    timeout: 30000,
    retry:
    {
        afterStatusCodes: [],
        statusCodes: [401, 429, 500],
        retryOnTimeout: true,
        limit: 10,
        jitter: true,
    },
    hooks: {
        beforeRequest: [],
        afterResponse: [
            async (req, opt, res, state) => {
                req; opt; state;
                if (!res.ok) {
                    const data = await res.text();
                    console.error(`api err ${data}`);
                }
            },
            async (req, _, res,) => {
                const resClone = res.clone();
                if (resClone.status == 204) return;
                const apiMeta: ApiMeta = await resClone.json();
                if (!apiMeta.request_id) return;
                let requestBody = "{}";

                if (req.method === "POST") {
                    try {
                        const clonedReq = req.clone();
                        const text = await clonedReq.text();
                        requestBody = text || "{}";
                    }
                    catch {
                        requestBody = "{}";
                    }
                }
                const auditLog: AuditLog = {
                    requestId: apiMeta.request_id,
                    requestBody: requestBody,
                    responseBody: "{}",
                    route: resClone.url,
                    status: resClone.status
                }

                auditClient.post("audit", { body: JSON.stringify(auditLog) });
            },
        ]
    }
});

const makeGet = <T>(url: string, options?: Options) => {
    const controller = new AbortController();
    return {
        request: httpClint.get(url, { ...options, signal: controller.signal }).json<T>(),
        cancel: (reason?: any) => controller.abort(reason ?? { abort: url })
    };
}

const makeDelete = <T>(url: string, options?: Options) => {
    const controller = new AbortController();
    return {
        request: httpClint.delete(url, { ...options, signal: controller.signal }).json<T>(),
        cancel: (reason?: any) => controller.abort(reason ?? { abort: url })
    };
}


const makePost = <T>(url: string, body: any, options?: Options) => {
    const controller = new AbortController();
    return {
        request: httpClint.post(url, { json: body, ...options, signal: controller.signal }).json<T>(),
        cancel: (reason?: any) => controller.abort(reason ?? { abort: url })
    };
}

const makePut = <T>(url: string, body?: any, options?: Options) => {
    const controller = new AbortController();
    return {
        request: httpClint.put(url, { json: body, ...options, signal: controller.signal }).json<T>(),
        cancel: (reason?: any) => controller.abort(reason ?? { abort: url })
    };
}

export const getSession = () => makeGet<void>("s");

export const getMe = () => makeGet<UserInfo>("s/me");

export const getProducts = (args: { query?: string | null, fromCookie?: boolean | null } = { fromCookie: null, query: null }) => {
    const params = new URLSearchParams();
    if (args.query) params.set('q', args.query);
    if (args.fromCookie) params.set("fc", args.fromCookie.toString());
    return makeGet<{ products: Product[] } & ApiMeta>(`p?${params.toString()}`);
}

export const getProduct = (id: number) => makeGet<Product & ApiMeta>(`p/${id}`);

export const uploadProduct = (product: UploadableProduct) => makePost<{ product_id: number } & ApiMeta>("p", product);

export const getSignedUrl = (productId: number, fileName: string) =>
    makeGet<{ url: string } & ApiMeta>(`p/${productId}/s?fn=${encodeURIComponent(fileName)}`)

export const putOnUrl = (url: string, buffer: ArrayBuffer) => ky(url, { method: "put", body: buffer });

export const deleteProduct = (id: number) => makeDelete<void>(`p/${id}/s`);

export const includeProduct = (id: number) => makePut<ApiMeta>(`p/${id}/cart`)

export const removeProduct = (id: number) => makeDelete<ApiMeta>(`p/${id}/cart`)

export const sendAudit = (arg: AuditLog) => makePost<void>("audit", arg);