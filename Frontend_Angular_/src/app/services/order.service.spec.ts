import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';

import { OrderService } from './order.service';
import { environment } from '../../environments/environment';
import { CreateOrderRequest, Order } from '../models/order.model';
import { ApiResponse } from '../models/api-response.model';

describe('OrderService', () => {
  let service: OrderService;
  let httpMock: HttpTestingController;

  const apiUrl = environment.apiUrl;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        OrderService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(OrderService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  //
  const mockOrder: Order = {
    OrderId: 1,
    UserId: 1,
    UserName: 'John Doe',
    OrderDate: new Date().toISOString(),
    TotalAmount: 500,
    OrderItems: []
  };

  const mockRequest: CreateOrderRequest = {
    UserId: 1,
    CartItems: [{ ProductId: 1, Quantity: 2 }]
  };

  // -----------------------------
  // CREATE ORDER
  // -----------------------------
  it('should create order', () => {
    const mockResponse: ApiResponse<Order> = {
      success: true,
      data: mockOrder
    };

    service.createOrder(mockRequest).subscribe(res => {
      expect(res.success).toBeTrue();

      // 
      if (res.data) {
        expect(res.data.OrderId).toBe(1);
        expect(res.data.TotalAmount).toBe(500);
      }
    });

    const req = httpMock.expectOne(`${apiUrl}/orders`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockRequest);

    req.flush(mockResponse);
  });

  // -----------------------------
  // GET ALL ORDERS
  // -----------------------------
  it('should fetch all orders', () => {
    const mockResponse: ApiResponse<Order[]> = {
      success: true,
      data: [mockOrder]
    };

    service.getAllOrders().subscribe(res => {
      expect(res.success).toBeTrue();

      if (res.data) {
        expect(res.data.length).toBe(1);
        expect(res.data[0].UserName).toBe('John Doe');
      }
    });

    const req = httpMock.expectOne(`${apiUrl}/orders`);
    expect(req.request.method).toBe('GET');

    req.flush(mockResponse);
  });

  // -----------------------------
  // GET ORDER BY ID
  // -----------------------------
  it('should fetch order by id', () => {
    const mockResponse: ApiResponse<Order> = {
      success: true,
      data: mockOrder
    };

    service.getOrderById(1).subscribe(res => {
      expect(res.success).toBeTrue();

      if (res.data) {
        expect(res.data.OrderId).toBe(1);
        expect(res.data.UserName).toBe('John Doe');
      }
    });

    const req = httpMock.expectOne(`${apiUrl}/orders/1`);
    expect(req.request.method).toBe('GET');

    req.flush(mockResponse);
  });

  // -----------------------------
  // ERROR HANDLING
  // -----------------------------
  it('should handle API error', () => {
    service.getAllOrders().subscribe({
      next: () => fail('should fail'),
      error: (err) => {
        expect(err.status).toBe(500);
      }
    });

    const req = httpMock.expectOne(`${apiUrl}/orders`);
    req.flush('Error', { status: 500, statusText: 'Server Error' });
  });
});