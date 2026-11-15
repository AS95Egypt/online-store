import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/auth/auth.guard';
import { HomeComponent } from './features/home/home.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'catalog', redirectTo: '', pathMatch: 'full' },
  { path: 'cart', canActivate: [authGuard], loadComponent: () => import('./features/cart/cart.component').then((m) => m.CartComponent) },
  { path: 'checkout', canActivate: [authGuard], loadComponent: () => import('./features/checkout/checkout.component').then((m) => m.CheckoutComponent) },
  { path: 'order-confirmation/:orderNumber', canActivate: [authGuard], loadComponent: () => import('./features/checkout/order-confirmation/order-confirmation.component').then((m) => m.OrderConfirmationComponent) },
  { path: 'categories', loadComponent: () => import('./features/categories/categories.component').then((m) => m.CategoriesComponent) },
  { path: 'products/:id', loadComponent: () => import('./features/product-details/product-details.component').then((m) => m.ProductDetailsComponent) },
  { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent) },
  { path: 'forgot-password', loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then((m) => m.ForgotPasswordComponent) },
  { path: 'verify-otp', loadComponent: () => import('./features/auth/verify-otp/verify-otp.component').then((m) => m.VerifyOtpComponent) },
  { path: 'reset-password', loadComponent: () => import('./features/auth/reset-password/reset-password.component').then((m) => m.ResetPasswordComponent) },
  { path: 'profile', canActivate: [authGuard], loadComponent: () => import('./features/account/profile/profile.component').then((m) => m.ProfileComponent) },
  { path: 'change-password', canActivate: [authGuard], loadComponent: () => import('./features/account/change-password/change-password.component').then((m) => m.ChangePasswordComponent) },
  { path: 'orders', canActivate: [authGuard], loadComponent: () => import('./features/account/orders/my-orders.component').then((m) => m.MyOrdersComponent) },
  { path: 'account', canActivate: [authGuard], loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent) },
  {
    path: 'admin',
    canActivate: [adminGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', loadComponent: () => import('./features/shared/placeholder-page.component').then((m) => m.PlaceholderPageComponent), data: { title: 'Admin Dashboard', message: 'The admin dashboard is coming soon.' } },
      { path: 'categories', loadComponent: () => import('./features/admin/categories/admin-categories.component').then((m) => m.AdminCategoriesComponent) },
      { path: 'brands', loadComponent: () => import('./features/admin/brands/admin-brands.component').then((m) => m.AdminBrandsComponent) },
      { path: 'products', loadComponent: () => import('./features/admin/products/admin-products.component').then((m) => m.AdminProductsComponent) },
      { path: 'orders/:id', loadComponent: () => import('./features/admin/orders/details/admin-order-details.component').then((m) => m.AdminOrderDetailsComponent) },
      { path: 'orders', loadComponent: () => import('./features/admin/orders/admin-orders.component').then((m) => m.AdminOrdersComponent) },
      { path: 'inventory', loadComponent: () => import('./features/shared/placeholder-page.component').then((m) => m.PlaceholderPageComponent), data: { title: 'Inventory', message: 'Inventory management is coming soon.' } },
      { path: 'shipping', loadComponent: () => import('./features/admin/shipping/admin-shipping.component').then((m) => m.AdminShippingComponent) },
    ],
  },
  { path: '**', redirectTo: '' },
];
