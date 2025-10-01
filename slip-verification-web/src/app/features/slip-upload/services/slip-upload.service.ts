import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { SlipVerification } from '../../../core/models/domain.models';

export interface VerifySlipRequest {
  orderId: string;
  file: File;
}

@Injectable({
  providedIn: 'root'
})
export class SlipUploadService {
  private readonly apiService = inject(ApiService);

  verifySlip(request: VerifySlipRequest): Observable<SlipVerification> {
    const formData = new FormData();
    formData.append('orderId', request.orderId);
    formData.append('file', request.file);

    return this.apiService.uploadFile<SlipVerification>('/slips/verify', formData);
  }

  getSlipById(id: string): Observable<SlipVerification> {
    return this.apiService.get<SlipVerification>(`/slips/${id}`);
  }
}
