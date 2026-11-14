import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/auth/auth.guard';
import { HomeComponent } from './features/home/home.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'catalog', redirectTo: '', pathMatch: 'full' },
  { path: 'cart', loadComponent: () => import('./features/shared/placeholder-page.component').then((m) => m.PlaceholderPageComponent), data: { title: 'Cart', message: 'Your cart is coming soon.' } },
  { path: 'categories', loadComponent: () => import('./features/categories/categories.component').then((m) => m.CategoriesComponent) },
  { path: 'products/:id', loadComponent: () => import('./features/product-details/product-details.component').then((m) => m.ProductDetailsComponent) },
  { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent) },
  { path: 'forgot-password', loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then((m) => m.ForgotPasswordComponent) },
  { path: 'verify-otp', loadComponent: () => import('./features/auth/verify-otp/verify-otp.component').then((m) => m.VerifyOtpComponent) },
  { path: 'reset-password', loadComponent: () => import('./features/auth/reset-password/reset-password.component').then((m) => m.ResetPasswordComponent) },
  { path: 'profile', canActivate: [authGuard], loadComponent: () => import('./features/account/profile/profile.component').then((m) => m.ProfileComponent) },
  { path: 'change-password', canActivate: [authGuard], loadComponent: () => import('./features/account/change-password/change-password.component').then((m) => m.ChangePasswordComponent) },
  { path: 'orders', canActivate: [authGuard], loadComponent: () => import('./features/account/orders/orders-placeholder.component').then((m) => m.OrdersPlaceholderComponent) },
  { path: 'account', canActivate: [authGuard], loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent) },
  {
    path: 'admin',
    canActivate: [adminGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', loadComponent: () => import('./features/shared/placeholder-page.component').then((m) => m.PlaceholderPageComponent), data: { title: 'Admin Dashboard', message: 'The admin dashboard is coming soon.' } },
      { path: 'products', loadComponent: () => import('./features/shared/placeholder-page.component').then((m) => m.PlaceholderPageComponent), data: { title: 'Products', message: 'Admin product management is coming soon.' } },
      { path: 'orders', loadComponent: () => import('./features/shared/placeholder-page.component').then((m) => m.PlaceholderPageComponent), data: { title: 'Orders', message: 'Admin order management is coming soon.' } },
      { path: 'inventory', loadComponent: () => import('./features/shared/placeholder-page.component').then((m) => m.PlaceholderPageComponent), data: { title: 'Inventory', message: 'Inventory management is coming soon.' } },
      { path: 'shipping', loadComponent: () => import('./features/shared/placeholder-page.component').then((m) => m.PlaceholderPageComponent), data: { title: 'Shipping', message: 'Shipping management is coming soon.' } },
    ],
  },
  { path: '**', redirectTo: '' },
];
