import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';

export interface TransactionDetailData {
  id: string;
  referenceNumber?: string;
  amount: number;
  status: string;
  transactionType?: string;
  bankName?: string;
  senderName?: string;
  senderAccount?: string;
  receiverName?: string;
  receiverAccount?: string;
  transactionDate?: string;
  createdAt: string;
  slipImageUrl?: string;
}

@Component({
  selector: 'app-transaction-detail-modal',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule
  ],
  template: `
    <div class="transaction-detail">
      <div class="header">
        <h2 mat-dialog-title>Transaction Details</h2>
        <button mat-icon-button (click)="onClose()">
          <mat-icon>close</mat-icon>
        </button>
      </div>
      
      <mat-dialog-content>
        <!-- Status Badge -->
        <div class="status-section">
          <mat-chip [color]="getStatusColor()" selected>
            <mat-icon>{{ getStatusIcon() }}</mat-icon>
            {{ data.status }}
          </mat-chip>
        </div>

        <!-- Amount -->
        <div class="amount-section">
          <span class="amount">{{ formatCurrency(data.amount) }}</span>
          @if (data.transactionType) {
            <span class="type">{{ data.transactionType }}</span>
          }
        </div>

        <mat-divider></mat-divider>

        <!-- Details Grid -->
        <div class="details-grid">
          @if (data.referenceNumber) {
            <div class="detail-item">
              <span class="label">Reference #</span>
              <span class="value">{{ data.referenceNumber }}</span>
            </div>
          }
          @if (data.bankName) {
            <div class="detail-item">
              <span class="label">Bank</span>
              <span class="value">{{ data.bankName }}</span>
            </div>
          }
          @if (data.senderName) {
            <div class="detail-item">
              <span class="label">Sender</span>
              <span class="value">{{ data.senderName }}</span>
              @if (data.senderAccount) {
                <span class="sub-value">{{ data.senderAccount }}</span>
              }
            </div>
          }
          @if (data.receiverName) {
            <div class="detail-item">
              <span class="label">Receiver</span>
              <span class="value">{{ data.receiverName }}</span>
              @if (data.receiverAccount) {
                <span class="sub-value">{{ data.receiverAccount }}</span>
              }
            </div>
          }
          <div class="detail-item">
            <span class="label">Transaction Date</span>
            <span class="value">{{ formatDate(data.transactionDate || data.createdAt) }}</span>
          </div>
          <div class="detail-item">
            <span class="label">Created</span>
            <span class="value">{{ formatDate(data.createdAt) }}</span>
          </div>
        </div>

        <!-- Slip Image -->
        @if (data.slipImageUrl) {
          <mat-divider></mat-divider>
          <div class="slip-section">
            <h4>Slip Image</h4>
            <img [src]="data.slipImageUrl" alt="Transaction Slip" class="slip-image">
          </div>
        }
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="onClose()">Close</button>
        <button mat-flat-button color="primary" (click)="onPrint()">
          <mat-icon>print</mat-icon>
          Print
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .transaction-detail {
      min-width: 450px;
      max-width: 600px;
    }
    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 24px 0;
    }
    .header h2 { margin: 0; }
    .status-section {
      margin-bottom: 16px;
    }
    .status-section mat-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
      margin-right: 4px;
    }
    .amount-section {
      text-align: center;
      margin: 24px 0;
    }
    .amount {
      font-size: 32px;
      font-weight: bold;
      color: #1976D2;
    }
    .type {
      display: block;
      font-size: 14px;
      color: #666;
      margin-top: 4px;
    }
    mat-divider {
      margin: 16px 0;
    }
    .details-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }
    .detail-item {
      display: flex;
      flex-direction: column;
    }
    .label {
      font-size: 12px;
      color: #999;
      text-transform: uppercase;
    }
    .value {
      font-size: 14px;
      font-weight: 500;
    }
    .sub-value {
      font-size: 12px;
      color: #666;
    }
    .slip-section h4 {
      margin: 16px 0 12px;
      color: #666;
    }
    .slip-image {
      max-width: 100%;
      border-radius: 8px;
      border: 1px solid #eee;
    }
    mat-dialog-actions {
      padding: 16px 24px;
    }
  `]
})
export class TransactionDetailModalComponent {
  constructor(
    public dialogRef: MatDialogRef<TransactionDetailModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: TransactionDetailData
  ) {}

  getStatusColor(): string {
    switch (this.data.status?.toLowerCase()) {
      case 'verified':
      case 'completed':
      case 'paid': return 'primary';
      case 'pending':
      case 'processing': return 'accent';
      case 'rejected':
      case 'failed': return 'warn';
      default: return '';
    }
  }

  getStatusIcon(): string {
    switch (this.data.status?.toLowerCase()) {
      case 'verified':
      case 'completed':
      case 'paid': return 'check_circle';
      case 'pending':
      case 'processing': return 'schedule';
      case 'rejected':
      case 'failed': return 'cancel';
      default: return 'info';
    }
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('th-TH', {
      style: 'currency',
      currency: 'THB'
    }).format(amount);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleString('th-TH');
  }

  onClose(): void {
    this.dialogRef.close();
  }

  onPrint(): void {
    window.print();
  }
}
