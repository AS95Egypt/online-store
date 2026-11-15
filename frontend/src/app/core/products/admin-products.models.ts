import { BrandSummary, CategorySummary, ProductPageMeta } from './products.models';

export interface AdminProductListItem {
  id: number;
  sku: string;
  nameAr: string;
  nameEn: string;
  price: number;
  qty: number;
  isActive: boolean;
  primaryImageUrl: string | null;
  brand: BrandSummary;
  category: CategorySummary;
}

export interface AdminProductDetails extends AdminProductListItem {
  descriptionAr: string | null;
  descriptionEn: string | null;
  thumb1Url: string | null;
  thumb2Url: string | null;
  thumb3Url: string | null;
  thumb4Url: string | null;
}

export interface AdminProductRequest {
  nameAr: string;
  nameEn: string;
  descriptionAr: string | null;
  descriptionEn: string | null;
  price: number;
  qty: number;
  categoryId: number;
  brandId: number;
  primaryImageUrl: string | null;
  thumb1Url: string | null;
  thumb2Url: string | null;
  thumb3Url: string | null;
  thumb4Url: string | null;
}

export interface AdminProductStatusRequest {
  isActive: boolean;
}

export interface AdminProductsResponse<T> {
  success: boolean;
  data: T;
  meta?: ProductPageMeta;
}
