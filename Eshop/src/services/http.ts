import ky, { type Options } from "ky";
import type { UserInfo, Product, UploadableProduct, ApiMeta, AuditLog } from "../types";

const headers = new Headers();
headers.set("content-type", "application/json; charset=utf-8");
const auditClient = ky.create({ prefixUrl: "cl" })
const appHeaders = {
    auditHeader: "xc-audit"
}
const noAudit = () => ({ [appHeaders.auditHeader]: "0" });
const httpClint = ky.create({
    headers,
    prefixUrl: "/cl",
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
                const reqClone = req.clone();

                if (reqClone.headers.get(appHeaders.auditHeader) == "0") return;
                if (res.status == 204) return;

                const resClone = res.clone();

                let apiMeta: ApiMeta | null = null;
                let requestBody = "{}";
                let responseBody = "{}";
                try {
                    responseBody = await resClone.text();
                    apiMeta = responseBody ? JSON.parse(responseBody) : null;
                } catch {
                    responseBody = "{e:1}";
                }
                if (!apiMeta?.request_id) return;
                if (req.method === "POST") {
                    try {
                        requestBody = (await reqClone.text()) || "{}";
                    }
                    catch {
                        requestBody = "{e:1}";
                    }
                }
                const auditLog: AuditLog = {
                    requestId: apiMeta.request_id,
                    requestBody: requestBody,
                    responseBody: responseBody,
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
    return makeGet<{ products: Product[] } & ApiMeta>(`p?${params.toString()}`, { headers: noAudit() });
}

export const getProduct = (id: number | string) => makeGet<Product & ApiMeta>(`p/${id}`, { headers: noAudit() });

export const uploadProduct = (product: UploadableProduct) => makePost<{ product_id: number } & ApiMeta>("p", product, { headers: noAudit() });

export const getSignedUrl = (productId: number, fileName: string) =>
    makeGet<{ url: string } & ApiMeta>(`p/${productId}/s?fn=${encodeURIComponent(fileName)}`, { headers: noAudit() })

export const putOnUrl = (url: string, buffer: ArrayBuffer) => ky(url, { method: "put", body: buffer });

export const deleteProduct = (id: number | string) => makeDelete<void>(`p/${id}/s`, { headers: noAudit() });

export const includeProduct = (id: number | string) => makePut<ApiMeta>(`p/${id}/cart`, { headers: noAudit() })

export const removeProduct = (id: number) => makeDelete<ApiMeta>(`p/${id}/cart`, { headers: noAudit() })

export const sendGoogleToken = (token: string) => makeGet<void>(`s/gt/${token}`, { headers: noAudit() });

export const sendAudit = (arg: AuditLog) => makePost<void>("audit", arg);