export interface BrandSummary {
  id: number;
  nameEn: string;
  displayImageUrl: string | null;
}

export interface BrandsResponse {
  success: boolean;
  data: BrandSummary[];
}
