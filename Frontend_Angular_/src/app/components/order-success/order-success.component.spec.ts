/// <reference types="jasmine" />

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

import { OrderSuccessComponent } from './order-success.component';
import { OrderService } from '../../services/order.service';
import { Order } from '../../models/order.model';

/* API wrapper (matches your service response shape) */
interface ApiResponse<T> {
  success: boolean;
  data?: T;
}

/* Mock service */
class MockOrderService {
  getOrderById = jasmine.createSpy();
}

describe('OrderSuccessComponent', () => {
  let component: OrderSuccessComponent;
  let fixture: ComponentFixture<OrderSuccessComponent>;
  let orderService: MockOrderService;

  /* Must match real Order model EXACTLY */
  const mockOrder: Order = {
    OrderId: 1,
    UserId: 100,
    UserName: 'John Doe',
    OrderDate: new Date().toISOString(), // string for Angular date pipe
    TotalAmount: 5000,
    OrderItems: [
      {
        OrderItemId: 10,
        ProductId: 101,
        ProductName: 'Laptop',
        Quantity: 1,
        Price: 5000,
        Subtotal: 5000
      }
    ]
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrderSuccessComponent],
      providers: [
        { provide: OrderService, useClass: MockOrderService },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: () => '1'
              }
            }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(OrderSuccessComponent);
    component = fixture.componentInstance;
    orderService = TestBed.inject(OrderService) as unknown as MockOrderService;
  });

  it('should load order on init', () => {
    orderService.getOrderById.and.returnValue(
      of({ success: true, data: mockOrder } as ApiResponse<Order>)
    );

    fixture.detectChanges();

    expect(component.orderId).toBe(1);
    expect(component.order).toEqual(mockOrder);
  });

  it('should NOT set order if API returns success false', () => {
    orderService.getOrderById.and.returnValue(
      of({ success: false } as ApiResponse<Order>)
    );

    fixture.detectChanges();

    expect(component.order).toBeNull();
  });

  it('should handle API error gracefully', () => {
    orderService.getOrderById.and.returnValue(
      throwError(() => new Error('API error'))
    );

    fixture.detectChanges();

    expect(component.order).toBeNull();
  });

  it('should render order details in template', () => {
    orderService.getOrderById.and.returnValue(
      of({ success: true, data: mockOrder } as ApiResponse<Order>)
    );

    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain('Laptop');

    // Must match formatted value (Angular number pipe)
    expect(compiled.textContent).toContain('5,000');

    // sanity check for UI
    expect(compiled.textContent).toContain('Order Placed Successfully');
  });
});