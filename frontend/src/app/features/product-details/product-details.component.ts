import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProductDetails } from '../../core/products/products.models';
import { ProductsService } from '../../core/products/products.service';

type ProductDetailsState = 'loading' | 'not-found' | 'error' | 'loaded';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.scss',
})
export class ProductDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly productsService = inject(ProductsService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly placeholderImage = '/product-placeholder.svg';
  private productId = 0;

  readonly state = signal<ProductDetailsState>('loading');
  readonly product = signal<ProductDetails | null>(null);
  readonly activeImage = signal(this.placeholderImage);
  readonly thumbnails = computed(() => {
    const product = this.product();
    if (!product) {
      return [];
    }

    return [
      product.thumb1Url,
      product.thumb2Url,
      product.thumb3Url,
      product.thumb4Url,
    ].filter((url): url is string => url !== null && url.trim().length > 0);
  });

  ngOnInit(): void {
    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => this.loadProduct(Number(params.get('id'))));
  }

  selectImage(imageUrl: string): void {
    this.activeImage.set(imageUrl);
  }

  retry(): void {
    this.loadProduct(this.productId);
  }

  backToCatalog(): void {
    void this.router.navigate(['/']);
  }

  private loadProduct(id: number): void {
    this.productId = id;
    this.product.set(null);
    this.activeImage.set(this.placeholderImage);
    this.state.set('loading');

    this.productsService.getProductById(id).subscribe({
      next: (product) => {
        this.product.set(product);
        this.activeImage.set(product.primaryImageUrl || this.placeholderImage);
        this.state.set('loaded');
      },
      error: (error: unknown) => {
        this.state.set(
          error instanceof HttpErrorResponse && error.status === 404
            ? 'not-found'
            : 'error',
        );
      },
    });
  }
}
