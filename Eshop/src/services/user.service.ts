export async function getUsername(): Promise<string | null> {
    const response = await fetch(`/api/config`);
    const data = await response.json();
    return data || null;
}