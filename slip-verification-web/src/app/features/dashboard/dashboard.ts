import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-dashboard',
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class DashboardComponent {
  stats = [
    { title: 'Total Orders', value: 1234, icon: 'shopping_cart', color: 'bg-blue-500' },
    { title: 'Pending Payments', value: 56, icon: 'payment', color: 'bg-yellow-500' },
    { title: 'Verified Slips', value: 890, icon: 'verified', color: 'bg-green-500' },
    { title: 'Failed Verifications', value: 23, icon: 'error', color: 'bg-red-500' }
  ];
}
