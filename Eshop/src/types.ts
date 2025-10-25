export enum CurrentViewSelection {
    Product, Order, Setting
}

export interface Product {
    id: string;
    title: string;
    description: string;
    image: string;
    price?: number;
    seller?: string;
}

export type ThemeDropdown = { name: string, label: string, icon: any };

export type MediaItem = { type: 'image' | 'video'; src: string; alt?: string };

export type SidebarOptions = { title: string; key: CurrentViewSelection; icon: any };