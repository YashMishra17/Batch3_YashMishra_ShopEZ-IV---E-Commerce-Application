/// <reference types="jasmine" />

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CheckoutComponent } from './checkout.component';
import { CartService } from '../../services/cart.service';
import { OrderService } from '../../services/order.service';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { of, throwError } from 'rxjs';
import { CartProduct } from '../../models/order.model';

describe('CheckoutComponent', () => {
  let component: CheckoutComponent;
  let fixture: ComponentFixture<CheckoutComponent>;

  let cartServiceSpy: jasmine.SpyObj<CartService>;
  let orderServiceSpy: jasmine.SpyObj<OrderService>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  const mockCart: CartProduct[] = [
    { ProductId: 1, Name: 'Mouse', Price: 100, Quantity: 2, ImageUrl: '', Stock: 5 }
  ];

  beforeEach(async () => {
    cartServiceSpy = jasmine.createSpyObj('CartService', [
      'getCart',
      'getCartTotal',
      'getCartCount',
      'clearCart'
    ]);

    orderServiceSpy = jasmine.createSpyObj('OrderService', ['createOrder']);

    authServiceSpy = jasmine.createSpyObj('AuthService', [
      'isLoggedIn',
      'getCurrentUser',
      'getUserId'
    ]);

    await TestBed.configureTestingModule({
      imports: [CheckoutComponent, RouterTestingModule],
      providers: [
        { provide: CartService, useValue: cartServiceSpy },
        { provide: OrderService, useValue: orderServiceSpy },
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CheckoutComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    spyOn(router, 'navigate');
  });

  it('should initialize values on ngOnInit', () => {
    authServiceSpy.isLoggedIn.and.returnValue(true);
    authServiceSpy.getCurrentUser.and.returnValue({
      UserId: 1,
      Name: 'John',
      Email: 'john@test.com',
      Role: 'User',
      Token: 'mock-token',
      ExpiresAt: new Date().toISOString()
    });

    cartServiceSpy.getCart.and.returnValue(mockCart);
    cartServiceSpy.getCartTotal.and.returnValue(200);
    cartServiceSpy.getCartCount.and.returnValue(2);

    component.ngOnInit();

    expect(component.isLoggedIn).toBeTrue();
    expect(component.cartItems).toEqual(mockCart);
    expect(component.total).toBe(200);
    expect(component.totalItems).toBe(2);
    expect(component.finalTotal).toBe(Math.round(200 - (200 * 0.05) + 29));
    expect(component.name).toBe('John');
    expect(component.email).toBe('john@test.com');
  });

  it('should redirect to login if not logged in', () => {
    component.isLoggedIn = false;

    component.placeOrder();

    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should validate required fields before placing order', () => {
    component.isLoggedIn = true;
    component.name = '';
    component.address = '';
    component.phone = '';

    component.placeOrder();

    expect(component.errorMessage).toBe('Please fill in Name, Phone, and Address.');
  });

  it('should place order successfully', () => {
    component.isLoggedIn = true;
    component.name = 'John';
    component.address = 'Address';
    component.phone = '1234567890';
    component.cartItems = mockCart;

    authServiceSpy.getUserId.and.returnValue(1);

    orderServiceSpy.createOrder.and.returnValue(of({
      success: true,
      data: {
        OrderId: 99,
        UserId: 1,
        UserName: 'John',
        OrderDate: new Date().toISOString(),
        TotalAmount: 200,
        OrderItems: []
      }
    }));

    component.placeOrder();

    expect(orderServiceSpy.createOrder).toHaveBeenCalledWith({
      UserId: 1,
      CartItems: [{ ProductId: 1, Quantity: 2 }]
    });

    expect(cartServiceSpy.clearCart).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/order-success', 99]);
    expect(component.loading).toBeFalse();
  });

  it('should handle 401 error on placeOrder', () => {
    component.isLoggedIn = true;
    component.name = 'John';
    component.address = 'Address';
    component.phone = '1234567890';
    component.cartItems = mockCart;

    orderServiceSpy.createOrder.and.returnValue(
      throwError(() => ({ status: 401 }))
    );

    component.placeOrder();

    expect(component.errorMessage).toBe('Session expired. Please login.');
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
    expect(component.loading).toBeFalse();
  });

  it('should handle generic error on placeOrder', () => {
    component.isLoggedIn = true;
    component.name = 'John';
    component.address = 'Address';
    component.phone = '1234567890';
    component.cartItems = mockCart;

    orderServiceSpy.createOrder.and.returnValue(
      throwError(() => ({ error: { message: 'Error occurred' } }))
    );

    component.placeOrder();

    expect(component.errorMessage).toBe('Error occurred');
    expect(component.loading).toBeFalse();
  });
});