import { Component, DestroyRef, HostListener, OnInit, inject, signal } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { CartService } from '../../core/cart/cart.service';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationService } from '../../core/notifications/notification.service';
import { AccountMenuComponent } from '../account-menu/account-menu.component';
import { SideMenuComponent } from '../side-menu/side-menu.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [AsyncPipe, FormsModule, RouterLink, RouterOutlet, AccountMenuComponent, SideMenuComponent],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss',
})
export class AppShellComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly authService = inject(AuthService);
  readonly cartService = inject(CartService);
  readonly notificationService = inject(NotificationService);
  readonly currentRole$ = this.authService.currentRole$;

  readonly isHomePage = signal(false);
  readonly isScrolled = signal(false);
  searchTerm = '';

  ngOnInit(): void {
    this.updateCurrentRoute();
    this.onWindowScroll();
    this.authService.isAuthenticated$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((isAuthenticated) => {
        if (isAuthenticated) {
          this.refreshCartCount();
        } else {
          this.cartService.clearItemCount();
        }
      });
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this.updateCurrentRoute());
  }

  @HostListener('window:scroll')
  onWindowScroll(): void {
    this.isScrolled.set(window.scrollY > 0);
  }

  submitSearch(): void {
    const search = this.searchTerm.trim();
    void this.router.navigate(['/'], {
      queryParams: { search: search || null, page: 1 },
      queryParamsHandling: 'merge',
    });
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.submitSearch();
  }

  dismissNotification(): void {
    this.notificationService.dismiss();
  }

  private refreshCartCount(): void {
    this.cartService.getCart()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        error: () => this.notificationService.show('Your cart count could not be refreshed.'),
      });
  }

  private updateCurrentRoute(): void {
    const url = this.router.url;
    const path = url.split('?')[0];
    this.isHomePage.set(path === '/');

    const searchParam = new URLSearchParams(url.split('?')[1] ?? '').get('search');
    this.searchTerm = searchParam ?? '';
  }
}
