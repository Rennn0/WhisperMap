export enum CurrentViewSelection {
    Product, Cart, Setting
}

export interface Product {
    id: number;
    title: string;
    description: string;
    price?: number;
    preview_img?: string,
    in_cart?: boolean,
    resources?: string[]
}

export interface TCartItem {
    id: number,
    title: string,
    price?: number
}

export interface UserInfo {
    can_upload: boolean,
    can_delete: boolean,
    claims: number[]
}

export interface UploadableProduct {
    title: string;
    price: number;
    description: string;
}

export interface UploadProps {
    product: UploadableProduct;
    existingFiles: File[];
    id: number | null
}

export interface AuditLog {
    requestId: string,
    requestBody: string,
    responseBody: string,
    status: number,
}

export type ThemeDropdown = { name: string, label: string, icon: any };

export type MediaItem = { type: 'image' | 'video'; src: string; alt?: string };

export type SidebarOptions = { title: string; key: CurrentViewSelection; icon: any };

export type ApiMeta = { timestamp: string, request_id: string };
