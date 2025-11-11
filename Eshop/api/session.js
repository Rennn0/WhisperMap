import { backendUrl } from "./variables.js"
export const config = {
    runtime: 'nodejs',
};

export default async function handler(req, res) {
    const ip = req.headers['x-real-ip'] || req.headers['x-forwarded-for']?.split(',')[0].trim();
    const apiResponse = await fetch(`${backendUrl}/session`, {
        method: "GET",
        headers: {
            'x-public-ip': ip,
            'content-type': 'application/json'
        }
    });
    const setCookies = apiResponse.headers.getSetCookie?.();
    if (setCookies && setCookies.length > 0) {
        res.setHeader('set-cookie', setCookies);
    }
    const bodyText = await apiResponse.text();
    let data;
    try {
        data = JSON.parse(bodyText);
    } catch (error) {
        data = bodyText;
    }
    return res.status(apiResponse.status).send(data);
}