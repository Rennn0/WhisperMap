const isDev = process.env.XC_ENV === 'local';

export const backendUrl = isDev
    ? 'http://localhost:5158'
    : process.env.BACKEND_URL;

export const apiKey = isDev
    ? 'dev'
    : process.env.CLIENT_API_KEY;
