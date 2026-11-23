import { Routes } from '@angular/router';

import { Login } from './features/auth/login';
import { Dashboard } from './features/dashboard/dashboard';
import { Categories } from './features/categories/categories';
import { Products } from './features/products/products';
import { AdminLayout } from './layouts/admin-layout/admin-layout';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: Login,
  },
  {
    path: '',
    component: AdminLayout,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        component: Dashboard,
      },
      {
        path: 'categories',
        component: Categories,
      },
      {
        path: 'products',
        component: Products,
      },
      {
        path: 'suppliers',
        loadComponent: () => import('./features/suppliers/suppliers').then((m) => m.Suppliers),
      },
      {
        path: 'warehouses',
        loadComponent: () => import('./features/warehouses/warehouses').then((m) => m.Warehouses),
        canActivate: [authGuard],
        data: { roles: ['Admin', 'Manager', 'Staff'] },
      },
      {
        path: 'customers',
        loadComponent: () => import('./features/customers/customers').then((m) => m.Customers),
        canActivate: [authGuard],
        data: { roles: ['Admin', 'Manager', 'Staff'] },
      },
      {
        path: 'inventory',
        loadComponent: () => import('./features/inventory/inventory').then((m) => m.Inventory),
        canActivate: [authGuard],
        data: { roles: ['Admin', 'Manager', 'Staff'] },
      },
      {
        path: 'purchase-orders',
        loadComponent: () =>
          import('./features/purchase-orders/purchase-orders').then((m) => m.PurchaseOrders),
        canActivate: [authGuard],
        data: { roles: ['Admin', 'Manager', 'Staff'] },
      },
      {
        path: 'sales-orders',
        loadComponent: () =>
          import('./features/sales-orders/sales-orders').then((m) => m.SalesOrders),
      },
      {
        path: 'payments',
        loadComponent: () => import('./features/payments/payments').then((m) => m.Payments),
      },
      {
        path: 'stock-transfers',
        loadComponent: () =>
          import('./features/stock-transfers/stock-transfers').then((m) => m.StockTransfers),
        canActivate: [authGuard],
        data: { roles: ['Admin', 'Manager', 'Staff'] },
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports').then((m) => m.Reports),
        canActivate: [authGuard],
      },
      {
        path: 'audit-logs',
        loadComponent: () => import('./features/audit-logs/audit-logs').then((m) => m.AuditLogs),
        canActivate: [authGuard],
        data: { roles: ['Admin', 'Manager'] },
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
