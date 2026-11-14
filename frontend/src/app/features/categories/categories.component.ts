import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CategorySummary } from '../../core/categories/categories.models';
import { CategoriesService } from '../../core/categories/categories.service';

@Component({
  selector: 'app-categories',
  standalone: true,
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss',
})
export class CategoriesComponent implements OnInit {
  private readonly categoriesService = inject(CategoriesService);
  private readonly router = inject(Router);

  readonly categories = signal<CategorySummary[]>([]);
  readonly isLoading = signal(true);
  readonly loadError = signal('');

  ngOnInit(): void {
    this.loadCategories();
  }

  openCategory(category: CategorySummary): void {
    void this.router.navigate(['/'], {
      queryParams: { categoryId: category.id, page: 1 },
      queryParamsHandling: 'merge',
    });
  }

  retry(): void {
    this.loadCategories();
  }

  private loadCategories(): void {
    this.isLoading.set(true);
    this.loadError.set('');
    this.categoriesService.getCategories().subscribe({
      next: (response) => {
        this.categories.set(response.data);
        this.isLoading.set(false);
      },
      error: () => {
        this.loadError.set('Categories could not be loaded. Please try again.');
        this.isLoading.set(false);
      },
    });
  }
}
