import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpParams } from '@angular/common/http';
import { ApiService, ApiResponse, PagedResult } from './api.service';
import { environment } from '../../../environments/environment';

describe('ApiService', () => {
  let service: ApiService;
  let httpMock: HttpTestingController;
  const baseUrl = environment.apiUrl || 'http://localhost:5000/api/v1';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ApiService]
    });

    service = TestBed.inject(ApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // Verify that no unmatched requests are outstanding
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('get', () => {
    it('should make GET request with correct URL', () => {
      const mockData = { id: 1, name: 'Test' };
      const endpoint = '/test';

      service.get<typeof mockData>(endpoint).subscribe(data => {
        expect(data).toEqual(mockData);
      });

      const req = httpMock.expectOne(`${baseUrl}${endpoint}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });

    it('should include query parameters', () => {
      const mockData = { items: [] };
      const endpoint = '/items';
      const params = new HttpParams()
        .set('page', '1')
        .set('size', '10');

      service.get<typeof mockData>(endpoint, params).subscribe(data => {
        expect(data).toEqual(mockData);
      });

      const req = httpMock.expectOne(req => 
        req.url === `${baseUrl}${endpoint}` && 
        req.params.get('page') === '1' &&
        req.params.get('size') === '10'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });
  });

  describe('post', () => {
    it('should make POST request with correct URL and body', () => {
      const mockResponse = { id: 1, success: true };
      const endpoint = '/create';
      const body = { name: 'New Item' };

      service.post<typeof mockResponse>(endpoint, body).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}${endpoint}`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush(mockResponse);
    });
  });

  describe('put', () => {
    it('should make PUT request with correct URL and body', () => {
      const mockResponse = { id: 1, success: true };
      const endpoint = '/update/1';
      const body = { name: 'Updated Item' };

      service.put<typeof mockResponse>(endpoint, body).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}${endpoint}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(body);
      req.flush(mockResponse);
    });
  });

  describe('delete', () => {
    it('should make DELETE request with correct URL', () => {
      const mockResponse = { success: true };
      const endpoint = '/delete/1';

      service.delete<typeof mockResponse>(endpoint).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}${endpoint}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(mockResponse);
    });
  });

  describe('uploadFile', () => {
    it('should upload file using FormData', () => {
      const mockResponse = { fileId: '123', success: true };
      const endpoint = '/upload';
      const formData = new FormData();
      formData.append('file', new Blob(['test']), 'test.txt');

      service.uploadFile<typeof mockResponse>(endpoint, formData).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}${endpoint}`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toBe(formData);
      req.flush(mockResponse);
    });
  });

  describe('error handling', () => {
    it('should handle HTTP error responses', () => {
      const endpoint = '/error';
      const errorMessage = 'Internal Server Error';

      service.get(endpoint).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(500);
          expect(error.statusText).toBe('Server Error');
        }
      });

      const req = httpMock.expectOne(`${baseUrl}${endpoint}`);
      req.flush(errorMessage, { status: 500, statusText: 'Server Error' });
    });

    it('should handle network errors', () => {
      const endpoint = '/network-error';

      service.get(endpoint).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.error.type).toBe('Network error');
        }
      });

      const req = httpMock.expectOne(`${baseUrl}${endpoint}`);
      req.error(new ProgressEvent('Network error'));
    });
  });

  describe('ApiResponse handling', () => {
    it('should handle ApiResponse format', () => {
      const mockApiResponse: ApiResponse<{ id: number }> = {
        data: { id: 1 },
        isSuccess: true,
        message: 'Success'
      };
      const endpoint = '/api-response';

      service.get<ApiResponse<{ id: number }>>(endpoint).subscribe(response => {
        expect(response.isSuccess).toBe(true);
        expect(response.data).toEqual({ id: 1 });
        expect(response.message).toBe('Success');
      });

      const req = httpMock.expectOne(`${baseUrl}${endpoint}`);
      req.flush(mockApiResponse);
    });

    it('should handle PagedResult format', () => {
      const mockPagedResult: PagedResult<{ id: number }> = {
        items: [{ id: 1 }, { id: 2 }],
        totalCount: 100,
        pageNumber: 1,
        pageSize: 10,
        totalPages: 10
      };
      const endpoint = '/paged';

      service.get<PagedResult<{ id: number }>>(endpoint).subscribe(response => {
        expect(response.items.length).toBe(2);
        expect(response.totalCount).toBe(100);
        expect(response.pageNumber).toBe(1);
      });

      const req = httpMock.expectOne(`${baseUrl}${endpoint}`);
      req.flush(mockPagedResult);
    });
  });
});
