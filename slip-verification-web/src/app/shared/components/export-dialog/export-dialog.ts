import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

export interface ExportDialogData {
  title?: string;
  defaultFormat?: 'csv' | 'excel' | 'pdf';
  availableFormats?: string[];
  showDateRange?: boolean;
  showColumns?: boolean;
  columns?: { key: string; label: string; selected: boolean }[];
}

export interface ExportDialogResult {
  format: string;
  startDate?: Date;
  endDate?: Date;
  columns?: string[];
}

@Component({
  selector: 'app-export-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatInputModule,
    MatCheckboxModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="export-dialog">
      <h2 mat-dialog-title>
        <mat-icon>download</mat-icon>
        {{ data.title || 'Export Data' }}
      </h2>
      
      <mat-dialog-content>
        <!-- Format Selection -->
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Export Format</mat-label>
          <mat-select [(ngModel)]="selectedFormat">
            @for (format of formats; track format) {
              <mat-option [value]="format.value">
                <mat-icon>{{ format.icon }}</mat-icon>
                {{ format.label }}
              </mat-option>
            }
          </mat-select>
        </mat-form-field>

        <!-- Date Range -->
        @if (data.showDateRange !== false) {
          <div class="date-range">
            <mat-form-field appearance="outline">
              <mat-label>Start Date</mat-label>
              <input matInput [matDatepicker]="startPicker" [(ngModel)]="startDate">
              <mat-datepicker-toggle matIconSuffix [for]="startPicker"></mat-datepicker-toggle>
              <mat-datepicker #startPicker></mat-datepicker>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>End Date</mat-label>
              <input matInput [matDatepicker]="endPicker" [(ngModel)]="endDate">
              <mat-datepicker-toggle matIconSuffix [for]="endPicker"></mat-datepicker-toggle>
              <mat-datepicker #endPicker></mat-datepicker>
            </mat-form-field>
          </div>
        }

        <!-- Column Selection -->
        @if (data.showColumns && columns.length > 0) {
          <div class="columns-section">
            <h4>Select Columns</h4>
            <div class="columns-grid">
              @for (col of columns; track col.key) {
                <mat-checkbox [(ngModel)]="col.selected">
                  {{ col.label }}
                </mat-checkbox>
              }
            </div>
          </div>
        }
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="onCancel()">Cancel</button>
        <button mat-flat-button color="primary" (click)="onExport()" [disabled]="exporting">
          @if (exporting) {
            <mat-spinner diameter="20"></mat-spinner>
          } @else {
            <mat-icon>download</mat-icon>
            Export
          }
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .export-dialog {
      min-width: 400px;
    }
    h2 {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .full-width {
      width: 100%;
    }
    .date-range {
      display: flex;
      gap: 16px;
    }
    .date-range mat-form-field {
      flex: 1;
    }
    .columns-section {
      margin-top: 16px;
    }
    .columns-section h4 {
      margin: 0 0 12px;
      color: #666;
    }
    .columns-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 8px;
    }
    mat-dialog-actions button mat-spinner {
      display: inline-block;
    }
  `]
})
export class ExportDialogComponent {
  selectedFormat = 'csv';
  startDate: Date | null = null;
  endDate: Date | null = null;
  columns: { key: string; label: string; selected: boolean }[] = [];
  exporting = false;

  formats = [
    { value: 'csv', label: 'CSV', icon: 'description' },
    { value: 'excel', label: 'Excel (.xlsx)', icon: 'grid_on' },
    { value: 'pdf', label: 'PDF', icon: 'picture_as_pdf' }
  ];

  constructor(
    public dialogRef: MatDialogRef<ExportDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ExportDialogData
  ) {
    this.selectedFormat = data.defaultFormat || 'csv';
    this.columns = data.columns || [];
    
    // Set default date range (last 30 days)
    this.endDate = new Date();
    this.startDate = new Date();
    this.startDate.setDate(this.startDate.getDate() - 30);
  }

  onExport(): void {
    const result: ExportDialogResult = {
      format: this.selectedFormat,
      startDate: this.startDate || undefined,
      endDate: this.endDate || undefined,
      columns: this.columns.filter(c => c.selected).map(c => c.key)
    };
    this.dialogRef.close(result);
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }
}
