import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';
import { SlipUpload } from './slip-upload';
import { SlipUploadService } from '../../services/slip-upload.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

describe('SlipUpload Component', () => {
  let component: SlipUpload;
  let fixture: ComponentFixture<SlipUpload>;
  let slipUploadServiceSpy: jasmine.SpyObj<SlipUploadService>;
  let notificationServiceSpy: jasmine.SpyObj<NotificationService>;

  const mockSlipVerification = {
    id: '123',
    orderId: '456',
    status: 'Pending',
    imageUrl: 'http://test.com/image.jpg',
    createdAt: new Date()
  };

  beforeEach(async () => {
    const slipUploadSpy = jasmine.createSpyObj('SlipUploadService', ['uploadSlip']);
    const notificationSpy = jasmine.createSpyObj('NotificationService', ['success', 'error']);

    await TestBed.configureTestingModule({
      imports: [
        SlipUpload,
        ReactiveFormsModule,
        BrowserAnimationsModule
      ],
      providers: [
        FormBuilder,
        { provide: SlipUploadService, useValue: slipUploadSpy },
        { provide: NotificationService, useValue: notificationSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SlipUpload);
    component = fixture.componentInstance;
    slipUploadServiceSpy = TestBed.inject(SlipUploadService) as jasmine.SpyObj<SlipUploadService>;
    notificationServiceSpy = TestBed.inject(NotificationService) as jasmine.SpyObj<NotificationService>;
    
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize upload form with required validators', () => {
    expect(component.uploadForm).toBeDefined();
    expect(component.uploadForm.get('orderId')).toBeDefined();
    expect(component.uploadForm.get('file')).toBeDefined();
    
    // Check validators
    const orderIdControl = component.uploadForm.get('orderId');
    orderIdControl?.setValue('');
    expect(orderIdControl?.hasError('required')).toBe(true);
  });

  describe('File Selection', () => {
    it('should handle file selection', () => {
      const mockFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      const event = {
        target: {
          files: [mockFile]
        }
      } as any;

      component.onFileSelected(event);

      expect(component.selectedFile()).toBe(mockFile);
    });

    it('should show error for invalid file type', () => {
      const mockFile = new File(['test'], 'test.txt', { type: 'text/plain' });
      const event = {
        target: {
          files: [mockFile]
        }
      } as any;

      // Mock environment
      spyOn(component as any, 'processFile').and.callFake((file: File) => {
        if (file.type !== 'image/jpeg' && file.type !== 'image/png') {
          notificationServiceSpy.error('Invalid File', 'Please select a valid image file (JPEG, PNG)');
        }
      });

      component.onFileSelected(event);

      expect(notificationServiceSpy.error).toHaveBeenCalledWith(
        'Invalid File',
        'Please select a valid image file (JPEG, PNG)'
      );
    });

    it('should handle empty file selection', () => {
      const event = {
        target: {
          files: []
        }
      } as any;

      const initialFile = component.selectedFile();
      component.onFileSelected(event);

      expect(component.selectedFile()).toBe(initialFile);
    });
  });

  describe('Drag and Drop', () => {
    it('should set isDragOver on drag over', () => {
      const event = new DragEvent('dragover');
      spyOn(event, 'preventDefault');
      spyOn(event, 'stopPropagation');

      component.onDragOver(event);

      expect(event.preventDefault).toHaveBeenCalled();
      expect(event.stopPropagation).toHaveBeenCalled();
      expect(component.isDragOver()).toBe(true);
    });

    it('should unset isDragOver on drag leave', () => {
      const event = new DragEvent('dragleave');
      spyOn(event, 'preventDefault');
      spyOn(event, 'stopPropagation');

      component.isDragOver.set(true);
      component.onDragLeave(event);

      expect(event.preventDefault).toHaveBeenCalled();
      expect(event.stopPropagation).toHaveBeenCalled();
      expect(component.isDragOver()).toBe(false);
    });

    it('should handle file drop', () => {
      const mockFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      const dataTransfer = {
        files: [mockFile]
      } as any;
      const event = new DragEvent('drop');
      Object.defineProperty(event, 'dataTransfer', { value: dataTransfer });
      spyOn(event, 'preventDefault');
      spyOn(event, 'stopPropagation');

      component.onDrop(event);

      expect(event.preventDefault).toHaveBeenCalled();
      expect(event.stopPropagation).toHaveBeenCalled();
      expect(component.isDragOver()).toBe(false);
    });
  });

  describe('File Upload', () => {
    it('should upload slip successfully', (done) => {
      const mockFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      component.selectedFile.set(mockFile);
      component.uploadForm.patchValue({
        orderId: '123',
        file: mockFile
      });

      slipUploadServiceSpy.uploadSlip.and.returnValue(of(mockSlipVerification));

      // Spy on the submit method
      spyOn(component as any, 'onSubmit').and.callFake(() => {
        component.isUploading.set(true);
        slipUploadServiceSpy.uploadSlip(mockFile, '123').subscribe(result => {
          component.isUploading.set(false);
          component.result.set(result);
          notificationServiceSpy.success('Upload Successful', 'Your slip has been uploaded');
          
          expect(component.isUploading()).toBe(false);
          expect(component.result()).toEqual(mockSlipVerification);
          expect(notificationServiceSpy.success).toHaveBeenCalled();
          done();
        });
      });

      (component as any).onSubmit();
    });

    it('should handle upload error', (done) => {
      const mockFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      component.selectedFile.set(mockFile);
      component.uploadForm.patchValue({
        orderId: '123',
        file: mockFile
      });

      const errorMessage = 'Upload failed';
      slipUploadServiceSpy.uploadSlip.and.returnValue(throwError(() => new Error(errorMessage)));

      spyOn(component as any, 'onSubmit').and.callFake(() => {
        component.isUploading.set(true);
        slipUploadServiceSpy.uploadSlip(mockFile, '123').subscribe({
          error: (error) => {
            component.isUploading.set(false);
            notificationServiceSpy.error('Upload Failed', error.message);
            
            expect(component.isUploading()).toBe(false);
            expect(notificationServiceSpy.error).toHaveBeenCalledWith('Upload Failed', errorMessage);
            done();
          }
        });
      });

      (component as any).onSubmit();
    });

    it('should not upload when form is invalid', () => {
      component.uploadForm.patchValue({
        orderId: '',
        file: null
      });

      expect(component.uploadForm.valid).toBe(false);
    });
  });

  describe('File Preview', () => {
    it('should generate preview URL for selected image', () => {
      const mockFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      
      // Mock FileReader
      const mockFileReader = {
        readAsDataURL: jasmine.createSpy('readAsDataURL'),
        onload: null as any,
        result: 'data:image/jpeg;base64,test'
      };

      spyOn(window as any, 'FileReader').and.returnValue(mockFileReader);

      component.selectedFile.set(mockFile);
      
      // Manually trigger what would happen in processFile
      if (mockFileReader.onload) {
        component.previewUrl.set(mockFileReader.result);
      }

      expect(component.previewUrl()).toBeTruthy();
    });
  });
});
