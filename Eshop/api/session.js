export const config = {
    runtime: 'nodejs',
};

export default async function handler(req, res) {
    const backendUrl = process.env.BACKEND_URL;
    // const backendUrl = 'http://localhost:5158';
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
// CfDJ8HNV6rmQKWVGn6dSuKTbMhKVLZ28GJH4ah8_QcaT0dntquSSpAq9O62FcVXW8yf91IxfIguBXgerSnvxgiG1qS0fMs36RZKU5jnS2tPEwSNuRdsqbttdE_2x_cxFWRnBd4cjbRFNSw9ZuZuum8Cpv249E_VRbuH8dabl1V6MHGj86XHNkKzhC_wXs0FXw_8mnWr7q-jxI0pW41t3u6OSOPE