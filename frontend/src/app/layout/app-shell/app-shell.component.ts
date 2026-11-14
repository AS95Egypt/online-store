import { Component, DestroyRef, HostListener, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AccountMenuComponent } from '../account-menu/account-menu.component';
import { SideMenuComponent } from '../side-menu/side-menu.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [FormsModule, RouterLink, RouterOutlet, AccountMenuComponent, SideMenuComponent],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss',
})
export class AppShellComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly isHomePage = signal(false);
  readonly isScrolled = signal(false);
  searchTerm = '';

  ngOnInit(): void {
    this.updateCurrentRoute();
    this.onWindowScroll();
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

  private updateCurrentRoute(): void {
    const url = this.router.url;
    const path = url.split('?')[0];
    this.isHomePage.set(path === '/');

    const searchParam = new URLSearchParams(url.split('?')[1] ?? '').get('search');
    this.searchTerm = searchParam ?? '';
  }
}
