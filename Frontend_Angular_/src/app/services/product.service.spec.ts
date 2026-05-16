import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';

import { ProductService } from './product.service';
import { environment } from '../../environments/environment';
import {
  Product,
  CreateProductRequest,
  UpdateProductRequest
} from '../models/product.model';
import { ApiResponse } from '../models/api-response.model';

describe('ProductService', () => {
  let service: ProductService;
  let httpMock: HttpTestingController;

  const apiUrl = environment.apiUrl;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ProductService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(ProductService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // ensure no pending requests
  });

  //ONLY include fields that exist in our Product model
  const mockProduct: Product = {
    ProductId: 1,
    Name: 'Mouse',
    Description: 'Wireless mouse',
    Price: 500,
    ImageUrl: 'mouse.jpg',
    Stock: 10
  };

  const createPayload: CreateProductRequest = {
    Name: 'Keyboard',
    Description: 'Mechanical keyboard',
    Price: 1500,
    ImageUrl: 'keyboard.jpg',
    Stock: 5
  };

  const updatePayload: UpdateProductRequest = {
    Name: 'Updated Keyboard',
    Description: 'RGB keyboard',
    Price: 2000,
    ImageUrl: 'keyboard-new.jpg',
    Stock: 8
  };

  // -----------------------------
  // GET ALL PRODUCTS
  // -----------------------------
  it('should fetch all products', () => {
    const mockResponse: ApiResponse<Product[]> = {
      success: true,
      data: [mockProduct]
    };

    service.getAllProducts().subscribe(res => {
      expect(res.success).toBeTrue();

      if (res.data) {
        expect(res.data.length).toBe(1);
        expect(res.data[0].Name).toBe('Mouse');
      }
    });

    const req = httpMock.expectOne(`${apiUrl}/products`);
    expect(req.request.method).toBe('GET');

    req.flush(mockResponse);
  });

  // -----------------------------
  // GET PRODUCT BY ID
  // -----------------------------
  it('should fetch product by id', () => {
    const mockResponse: ApiResponse<Product> = {
      success: true,
      data: mockProduct
    };

    service.getProductById(1).subscribe(res => {
      expect(res.success).toBeTrue();

      if (res.data) {
        expect(res.data.ProductId).toBe(1);
      }
    });

    const req = httpMock.expectOne(`${apiUrl}/products/1`);
    expect(req.request.method).toBe('GET');

    req.flush(mockResponse);
  });

  // -----------------------------
  // CREATE PRODUCT
  // -----------------------------
  it('should create product', () => {
    const mockResponse: ApiResponse<Product> = {
      success: true,
      data: mockProduct
    };

    service.createProduct(createPayload).subscribe(res => {
      expect(res.success).toBeTrue();

      if (res.data) {
        expect(res.data.Name).toBe('Mouse');
      }
    });

    const req = httpMock.expectOne(`${apiUrl}/products`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(createPayload);

    req.flush(mockResponse);
  });

  // -----------------------------
  // UPDATE PRODUCT
  // -----------------------------
  it('should update product', () => {
    const mockResponse: ApiResponse<Product> = {
      success: true,
      data: mockProduct
    };

    service.updateProduct(1, updatePayload).subscribe(res => {
      expect(res.success).toBeTrue();

      if (res.data) {
        expect(res.data.ProductId).toBe(1);
      }
    });

    const req = httpMock.expectOne(`${apiUrl}/products/1`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(updatePayload);

    req.flush(mockResponse);
  });

  // -----------------------------
  // DELETE PRODUCT
  // -----------------------------
  it('should delete product', () => {
    const mockResponse: ApiResponse<any> = {
      success: true,
      message: 'Deleted successfully'
    };

    service.deleteProduct(1).subscribe(res => {
      expect(res.success).toBeTrue();
      expect(res.message).toBe('Deleted successfully');
    });

    const req = httpMock.expectOne(`${apiUrl}/products/1`);
    expect(req.request.method).toBe('DELETE');

    req.flush(mockResponse);
  });

  // -----------------------------
  // ERROR HANDLING
  // -----------------------------
  it('should handle API error', () => {
    service.getAllProducts().subscribe({
      next: () => fail('should fail'),
      error: (err) => {
        expect(err.status).toBe(500);
      }
    });

    const req = httpMock.expectOne(`${apiUrl}/products`);
    req.flush('Error', { status: 500, statusText: 'Server Error' });
  });
});