import { AsyncPipe } from '@angular/common';
import { Component, DestroyRef, ElementRef, HostListener, OnInit, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subscription } from 'rxjs';
import { AccountService } from '../../core/account/account.service';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-account-menu',
  standalone: true,
  imports: [AsyncPipe, RouterLink],
  templateUrl: './account-menu.component.html',
  styleUrl: './account-menu.component.scss',
})
export class AccountMenuComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly elementRef = inject(ElementRef<HTMLElement>);
  private readonly authService = inject(AuthService);
  private readonly accountService = inject(AccountService);
  private readonly router = inject(Router);

  readonly isAuthenticated$ = this.authService.isAuthenticated$;
  readonly currentUser$ = this.authService.currentUser$;
  isOpen = false;
  profileLoadError = '';
  private profileSubscription: Subscription | null = null;

  ngOnInit(): void {
    this.authService.isAuthenticated$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((isAuthenticated) => {
        this.profileSubscription?.unsubscribe();
        this.profileSubscription = null;
        if (!isAuthenticated) {
          this.isOpen = false;
          this.profileLoadError = '';
          this.authService.setCurrentUser(null);
          return;
        }

        this.profileLoadError = '';
        this.profileSubscription = this.accountService
          .getProfile()
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            error: () => {
              this.profileLoadError = 'Unable to load account details.';
            },
          });
      });
  }

  @HostListener('document:click', ['$event'])
  closeOnOutsideClick(event: MouseEvent): void {
    if (!(event.target instanceof Node) || !this.elementRef.nativeElement.contains(event.target)) {
      this.closeMenu();
    }
  }

  toggleMenu(): void {
    this.isOpen = !this.isOpen;
  }

  closeMenu(): void {
    this.isOpen = false;
  }

  avatarInitials(name: string | null | undefined): string {
    const parts = name?.trim().split(/\s+/).filter(Boolean) ?? [];
    return parts.length
      ? parts.slice(0, 2).map((part) => part[0].toUpperCase()).join('')
      : '?';
  }

  logout(): void {
    this.closeMenu();
    this.authService.logout();
    void this.router.navigateByUrl('/');
  }
}
