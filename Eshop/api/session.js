export const config = {
    runtime: 'nodejs',
};

export default async function handler(req, res) {
    const backendUrl = process.env.BACKEND_URL || 'localhost:5158';
    const apiResponse = await fetch(`${backendUrl}/session`);
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
