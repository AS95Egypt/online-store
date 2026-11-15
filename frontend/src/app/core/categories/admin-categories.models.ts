export interface AdminCategory {
  id: number;
  nameAr: string;
  nameEn: string;
  displayImageUrl: string | null;
  isActive: boolean;
}

export interface AdminCategoryRequest {
  nameAr: string;
  nameEn: string;
  displayImageUrl: string | null;
}

export interface AdminCategoryStatusRequest {
  isActive: boolean;
}

export interface AdminCategoriesResponse<T> {
  success: boolean;
  data: T;
}
