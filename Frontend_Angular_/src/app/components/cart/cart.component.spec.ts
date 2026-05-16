/// <reference types="jasmine" />

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CartComponent } from './cart.component';
import { CartService } from '../../services/cart.service';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { BehaviorSubject } from 'rxjs';
import { CommonModule } from '@angular/common';
import { CartProduct } from '../../models/order.model';

describe('CartComponent', () => {
  let component: CartComponent;
  let fixture: ComponentFixture<CartComponent>;
  let cartServiceSpy: jasmine.SpyObj<CartService>;
  let router: Router;

  const cartSubject = new BehaviorSubject<CartProduct[]>([]);

  const mockItems: CartProduct[] = [
    { ProductId: 1, Name: 'Mouse', Price: 100, Quantity: 2, ImageUrl: '', Stock: 5 },
    { ProductId: 2, Name: 'Keyboard', Price: 200, Quantity: 1, ImageUrl: '', Stock: 10 }
  ];

  beforeEach(async () => {
    cartServiceSpy = jasmine.createSpyObj(
      'CartService',
      ['getCartTotal', 'getCartCount', 'increaseQuantity', 'decreaseQuantity', 'removeFromCart'],
      { cart$: cartSubject.asObservable() }
    );

    await TestBed.configureTestingModule({
      imports: [
        CartComponent,
        CommonModule,
        RouterTestingModule
      ],
      providers: [
        { provide: CartService, useValue: cartServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CartComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    spyOn(router, 'navigate');
  });

  it('should subscribe to cart$ on init and update values', () => {
    cartServiceSpy.getCartTotal.and.returnValue(400);
    cartServiceSpy.getCartCount.and.returnValue(3);

    component.ngOnInit();
    cartSubject.next(mockItems);

    expect(component.cartItems).toEqual(mockItems);
    expect(component.total).toBe(400);
    expect(component.totalItems).toBe(3);
  });

  it('should call increaseQuantity', () => {
    component.increase(1);
    expect(cartServiceSpy.increaseQuantity).toHaveBeenCalledWith(1);
  });

  it('should call decreaseQuantity', () => {
    component.decrease(1);
    expect(cartServiceSpy.decreaseQuantity).toHaveBeenCalledWith(1);
  });

  it('should call removeFromCart', () => {
    component.remove(1);
    expect(cartServiceSpy.removeFromCart).toHaveBeenCalledWith(1);
  });

  it('should navigate to checkout on checkout()', () => {
    component.checkout();
    expect(router.navigate).toHaveBeenCalledWith(['/checkout']);
  });

  it('should update totals when cart changes multiple times', () => {
    component.ngOnInit();

    cartServiceSpy.getCartTotal.and.returnValue(100);
    cartServiceSpy.getCartCount.and.returnValue(1);

    cartSubject.next([mockItems[0]]);
    expect(component.total).toBe(100);
    expect(component.totalItems).toBe(1);

    cartServiceSpy.getCartTotal.and.returnValue(300);
    cartServiceSpy.getCartCount.and.returnValue(3);

    cartSubject.next(mockItems);
    expect(component.total).toBe(300);
    expect(component.totalItems).toBe(3);
  });

  it('should handle empty cart correctly', () => {
    cartServiceSpy.getCartTotal.and.returnValue(0);
    cartServiceSpy.getCartCount.and.returnValue(0);

    component.ngOnInit();
    cartSubject.next([]);

    expect(component.cartItems.length).toBe(0);
    expect(component.total).toBe(0);
    expect(component.totalItems).toBe(0);
  });
});