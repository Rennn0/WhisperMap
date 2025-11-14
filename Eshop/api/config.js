import { getAll } from '@vercel/edge-config';

export const config = {
    runtime: 'edge',
};

export default async function handler() {
    try {
        const allValues = await getAll();

        return new Response(JSON.stringify(allValues, null, 2), {
            status: 200,
            headers: { 'Content-Type': 'application/json' },
        });
    } catch (error) {
        return new Response(
            JSON.stringify({ error: error.message || String(error) }),
            { status: 500, headers: { 'Content-Type': 'application/json' } }
        );
    }
}
