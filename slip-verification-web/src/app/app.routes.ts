import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard').then(m => m.DashboardComponent)
      },
      {
        path: 'slip-upload',
        loadComponent: () => import('./features/slip-upload/components/slip-upload/slip-upload').then(m => m.SlipUpload)
      },
      {
        path: 'orders',
        loadComponent: () => import('./features/order-management/order-list/order-list').then(m => m.OrderList)
      },
      {
        path: 'transactions',
        loadComponent: () => import('./features/transaction-history/transaction-list/transaction-list').then(m => m.TransactionList)
      },
      {
        path: 'reports',
        canActivate: [roleGuard],
        data: { roles: ['Admin'] },
        loadComponent: () => import('./features/reports/reports').then(m => m.Reports)
      }
    ]
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('./shared/components/unauthorized/unauthorized').then(m => m.Unauthorized)
  },
  {
    path: '**',
    loadComponent: () => import('./shared/components/not-found/not-found').then(m => m.NotFound)
  }
];
