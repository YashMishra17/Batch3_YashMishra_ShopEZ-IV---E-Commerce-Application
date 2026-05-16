/// <reference types="jasmine" />

import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HomeComponent } from './home.component';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { of, throwError } from 'rxjs';
import { Product } from '../../models/product.model';
import { RouterTestingModule } from '@angular/router/testing';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  let productServiceSpy: jasmine.SpyObj<ProductService>;
  let cartServiceSpy: jasmine.SpyObj<CartService>;

  const mockProducts: Product[] = [
    {
      ProductId: 1,
      Name: 'Laptop Pro',
      Description: 'High performance laptop',
      Price: 100,
      Stock: 5,
      ImageUrl: ''
    },
    {
      ProductId: 2,
      Name: 'Wireless Mouse',
      Description: 'Smooth mouse',
      Price: 50,
      Stock: 10,
      ImageUrl: ''
    }
  ];

  beforeEach(async () => {
    productServiceSpy = jasmine.createSpyObj('ProductService', ['getAllProducts']);
    cartServiceSpy = jasmine.createSpyObj('CartService', ['addToCart']);

    await TestBed.configureTestingModule({
      imports: [
        HomeComponent,
        RouterTestingModule   //  provides ActivatedRoute
      ],
      providers: [
        { provide: ProductService, useValue: productServiceSpy },
        { provide: CartService, useValue: cartServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
  });

  // ---------------- INIT ----------------

  it('should load products on init', () => {
    productServiceSpy.getAllProducts.and.returnValue(
      of({ success: true, data: mockProducts })
    );

    fixture.detectChanges();

    expect(component.loading).toBeFalse();
    expect(component.featuredProducts.length).toBe(2);
    expect(component.allProducts.length).toBe(2);
  });

  it('should handle API error', () => {
    productServiceSpy.getAllProducts.and.returnValue(
      throwError(() => ({ status: 500 }))
    );

    fixture.detectChanges();

    expect(component.loading).toBeFalse();
    expect(component.errorMessage).toBe('Failed to load products.');
  });

  it('should show API connection error when status 0', () => {
    productServiceSpy.getAllProducts.and.returnValue(
      throwError(() => ({ status: 0 }))
    );

    fixture.detectChanges();

    expect(component.errorMessage).toContain('Cannot connect to API');
  });

  // ---------------- IMAGE ----------------

  it('should return mapped image based on product name', () => {
    const image = component.getProductImage(mockProducts[1]);

    expect(image).toContain('mouse.jpg');
  });

  it('should fallback to default image', () => {
    const product: Product = {
      ProductId: 3,
      Name: 'Unknown Gadget',
      Description: '',
      Price: 10,
      Stock: 1,
      ImageUrl: ''
    };

    const image = component.getProductImage(product);

    expect(image).toBe('assets/images/mouse.jpg');
  });

  // ---------------- CART ----------------

  it('should add product to cart', () => {
    component.addToCart(mockProducts[0]);

    expect(cartServiceSpy.addToCart).toHaveBeenCalled();
    expect(component.toastMsg).toContain('Laptop');
  });

  it('should NOT add out-of-stock product', () => {
    const product = { ...mockProducts[0], Stock: 0 };

    component.addToCart(product);

    expect(cartServiceSpy.addToCart).not.toHaveBeenCalled();
  });

  // ---------------- TOAST ----------------

  it('should show and clear toast message', fakeAsync(() => {
    component.showToast('Test Message');

    expect(component.toastMsg).toBe('Test Message');

    tick(2500);

    expect(component.toastMsg).toBe('');
  }));
});