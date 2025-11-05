export const getUsername = async (): Promise<string | null> => {
    const response = await fetch(`/api/config`);
    const data = await response.json();
    return data || null;
}

export const getSession = async (): Promise<{} | null> => {
    const response = await fetch(`/api/session`);
    const data = await response.json();
    return data || null;
}