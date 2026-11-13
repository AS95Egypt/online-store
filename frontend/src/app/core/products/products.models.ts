export interface BrandSummary {
  id: number;
  nameAr: string;
  nameEn: string;
  displayImageUrl: string | null;
}

export interface CategorySummary {
  id: number;
  nameAr: string;
  nameEn: string;
  displayImageUrl: string | null;
}

export interface ProductListItem {
  id: number;
  nameAr: string;
  nameEn: string;
  price: number;
  qty: number;
  stockStatus: 'InStock' | 'OutOfStock';
  primaryImageUrl: string | null;
  brand: BrandSummary;
  category: CategorySummary;
}

export interface ProductDetails {
  id: number;
  nameAr: string;
  nameEn: string;
  price: number;
  qty: number;
  stockStatus: 'InStock' | 'OutOfStock';
  primaryImageUrl: string | null;
  thumb1Url: string | null;
  thumb2Url: string | null;
  thumb3Url: string | null;
  thumb4Url: string | null;
  descriptionAr: string | null;
  descriptionEn: string | null;
  brand: BrandSummary;
  category: CategorySummary;
}

export interface ProductPageMeta {
  total: number;
  page: number;
  pageSize: number;
}

export interface PagedResult<T> {
  success: boolean;
  data: T[];
  meta: ProductPageMeta;
}
