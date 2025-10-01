import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { AuthService } from '../../core/services/auth.service';
import { NotificationPanel } from '../../shared/components/notification-panel/notification-panel';

@Component({
  selector: 'app-main-layout',
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule,
    NotificationPanel
  ],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss'
})
export class MainLayout {
  authService = inject(AuthService);
  
  menuItems = [
    { path: '/dashboard', icon: 'dashboard', label: 'Dashboard' },
    { path: '/slip-upload', icon: 'upload', label: 'Upload Slip' },
    { path: '/orders', icon: 'shopping_cart', label: 'Orders' },
    { path: '/transactions', icon: 'history', label: 'Transactions' },
    { path: '/reports', icon: 'assessment', label: 'Reports', roles: ['Admin'] }
  ];

  logout(): void {
    this.authService.logout();
  }

  canShowMenuItem(item: any): boolean {
    if (!item.roles) return true;
    return this.authService.hasAnyRole(item.roles);
  }
}
