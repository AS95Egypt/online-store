export interface AdminBrand {
  id: number;
  nameAr: string;
  nameEn: string;
  displayImageUrl: string | null;
}

export interface AdminBrandRequest {
  nameAr: string;
  nameEn: string;
  displayImageUrl: string | null;
}

export interface AdminBrandsResponse<T> {
  success: boolean;
  data: T;
}
