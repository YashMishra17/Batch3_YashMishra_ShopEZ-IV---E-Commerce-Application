/// <reference types="jasmine" />

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductDetailComponent } from './product-detail.component';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { Product } from '../../models/product.model';

describe('ProductDetailComponent', () => {
  let component: ProductDetailComponent;
  let fixture: ComponentFixture<ProductDetailComponent>;

  let mockProductService: jasmine.SpyObj<ProductService>;
  let mockCartService: jasmine.SpyObj<CartService>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockProduct: Product = {
    ProductId: 1,
    Name: 'Laptop',
    Description: 'Test Laptop',
    Price: 50000,
    ImageUrl: 'assets/images/laptop.jpg',
    Stock: 5
  };

  beforeEach(async () => {
    mockProductService = jasmine.createSpyObj('ProductService', ['getProductById']);
    mockCartService = jasmine.createSpyObj('CartService', ['addToCart']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [ProductDetailComponent],
      providers: [
        { provide: ProductService, useValue: mockProductService },
        { provide: CartService, useValue: mockCartService },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: { get: () => '1' }
            }
          }
        },
        { provide: Router, useValue: mockRouter }
      ],
      schemas: [NO_ERRORS_SCHEMA] 
    })
    .overrideComponent(ProductDetailComponent, {
      set: {
        imports: [] 
      }
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductDetailComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load product on init', () => {
    mockProductService.getProductById.and.returnValue(of({ success: true, data: mockProduct }));

    component.ngOnInit();

    expect(component.product).toEqual(mockProduct);
    expect(component.loading).toBeFalse();
  });

  it('should handle 404 error', () => {
    mockProductService.getProductById.and.returnValue(
      throwError(() => ({ status: 404 }))
    );

    component.ngOnInit();

    expect(component.errorMessage).toBe('Product not found.');
  });

  it('should handle generic error', () => {
    mockProductService.getProductById.and.returnValue(
      throwError(() => ({ status: 500 }))
    );

    component.ngOnInit();

    expect(component.errorMessage).toBe('Failed to load product.');
  });

  it('should increase quantity', () => {
    component.product = mockProduct;
    component.quantity = 1;

    component.increaseQty();

    expect(component.quantity).toBe(2);
  });

  it('should not exceed stock', () => {
    component.product = mockProduct;
    component.quantity = 5;

    component.increaseQty();

    expect(component.quantity).toBe(5);
  });

  it('should decrease quantity', () => {
    component.quantity = 2;

    component.decreaseQty();

    expect(component.quantity).toBe(1);
  });

  it('should map image correctly', () => {
    const product = { ...mockProduct, Name: 'Gaming Laptop' };

    const result = component.getImage(product);

    expect(result).toContain('laptop');
  });

  it('should fallback to default image', () => {
    const product = { ...mockProduct, Name: 'Unknown Device', ImageUrl: '' };

    const result = component.getImage(product);

    expect(result).toContain('mouse.jpg');
  });

  it('should add to cart multiple times based on quantity', () => {
    component.product = mockProduct;
    component.quantity = 3;
    component.productImage = 'img.jpg';

    component.addToCart();

    expect(mockCartService.addToCart).toHaveBeenCalledTimes(3);
  });

  it('should not add to cart if out of stock', () => {
    component.product = { ...mockProduct, Stock: 0 };

    component.addToCart();

    expect(mockCartService.addToCart).not.toHaveBeenCalled();
  });

  it('should navigate to cart on buyNow', () => {
    component.product = mockProduct;
    component.quantity = 1;
    component.productImage = 'img.jpg';

    component.buyNow();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/cart']);
  });
});