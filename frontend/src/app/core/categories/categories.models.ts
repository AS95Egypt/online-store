export interface CategorySummary {
  id: number;
  nameEn: string;
  displayImageUrl: string | null;
}

export interface CategoriesResponse {
  success: boolean;
  data: CategorySummary[];
}
