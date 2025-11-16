export enum CurrentViewSelection {
    Product, Order, Setting
}

export interface Product {
    id: string;
    title: string;
    description: string;
    price?: number;
    preview_img?: string,
    resources?: string[]
}

export interface UserInfo {
    can_upload: boolean,
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

export type ThemeDropdown = { name: string, label: string, icon: any };

export type MediaItem = { type: 'image' | 'video'; src: string; alt?: string };

export type SidebarOptions = { title: string; key: CurrentViewSelection; icon: any };

export type ApiMeta = { timestamp: string, request_id: string };