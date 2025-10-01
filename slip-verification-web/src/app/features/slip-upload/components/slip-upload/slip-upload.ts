import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { SlipUploadService } from '../../services/slip-upload.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { SlipVerification } from '../../../../core/models/domain.models';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-slip-upload',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule
  ],
  templateUrl: './slip-upload.html',
  styleUrl: './slip-upload.scss'
})
export class SlipUpload {
  private readonly fb = inject(FormBuilder);
  private readonly slipUploadService = inject(SlipUploadService);
  private readonly notificationService = inject(NotificationService);

  uploadForm: FormGroup;
  selectedFile = signal<File | null>(null);
  previewUrl = signal<string | null>(null);
  uploadProgress = signal<number>(0);
  isUploading = signal<boolean>(false);
  result = signal<SlipVerification | null>(null);
  isDragOver = signal<boolean>(false);

  constructor() {
    this.uploadForm = this.fb.group({
      orderId: ['', [Validators.required]],
      file: [null, [Validators.required]]
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.processFile(input.files[0]);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(false);

    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      this.processFile(event.dataTransfer.files[0]);
    }
  }

  private processFile(file: File): void {
    // Validate file type
    if (!environment.supportedFileTypes.includes(file.type)) {
      this.notificationService.error('Invalid File', 'Please select a valid image file (JPEG, PNG)');
      return;
    }

    // Validate file size
    if (file.size > environment.uploadMaxSize) {
      this.notificationService.error('File Too Large', `File size must be less than ${environment.uploadMaxSize / 1024 / 1024}MB`);
      return;
    }

    this.selectedFile.set(file);
    this.uploadForm.patchValue({ file });

    // Generate preview
    const reader = new FileReader();
    reader.onload = (e) => {
      this.previewUrl.set(e.target?.result as string);
    };
    reader.readAsDataURL(file);
  }

  removeFile(): void {
    this.selectedFile.set(null);
    this.previewUrl.set(null);
    this.uploadForm.patchValue({ file: null });
  }

  onSubmit(): void {
    if (this.uploadForm.invalid || !this.selectedFile()) {
      this.notificationService.warning('Validation Error', 'Please fill all required fields');
      return;
    }

    this.isUploading.set(true);
    this.uploadProgress.set(0);

    const request = {
      orderId: this.uploadForm.value.orderId,
      file: this.selectedFile()!
    };

    this.slipUploadService.verifySlip(request).subscribe({
      next: (result) => {
        this.result.set(result);
        this.isUploading.set(false);
        this.uploadProgress.set(100);
        this.notificationService.success('Success', 'Slip uploaded successfully');
      },
      error: (error) => {
        this.isUploading.set(false);
        this.uploadProgress.set(0);
      }
    });
  }

  reset(): void {
    this.uploadForm.reset();
    this.selectedFile.set(null);
    this.previewUrl.set(null);
    this.result.set(null);
    this.uploadProgress.set(0);
  }
}
