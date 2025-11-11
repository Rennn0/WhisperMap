import { backendUrl, apiKey } from "../variables.js"
export const config = {
    runtime: 'nodejs',
};

export default async function handler(req, res) {
    const cookies = req.headers.cookie;
    const { id } = req.query
    const apiResponse = await fetch(`${backendUrl}/product/${id}`, {
        method: 'GET',
        headers: {
            'content-type': 'application/json',
            'x-api-key': apiKey,
            ...(cookies ? { 'cookie': cookies } : {})
        },
        credentials: "include"
    });
    const bodyText = await apiResponse.text();
    return res.status(apiResponse.status).send(bodyText);
}
