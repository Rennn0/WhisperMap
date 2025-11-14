import type { UserInfo } from "../types";

export const getConfig = async (): Promise<string | null> => {
    const response = await fetch(`/ve/config`);
    const data = await response.json();
    return data || null;
}

export const getSession = async (): Promise<void> => {
    await fetch(`/ve/session`, { method: "GET", credentials: "include" });
}

export const getMe = async (): Promise<UserInfo | null> => {
    const response = await fetch(`/api/me`, { method: "GET", credentials: "include" });
    const data = response.json();
    return data;
}