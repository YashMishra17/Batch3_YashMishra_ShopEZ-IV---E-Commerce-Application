/// <reference types="jasmine" />

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductsComponent } from './products.component';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('ProductsComponent', () => {
  let component: ProductsComponent;
  let fixture: ComponentFixture<ProductsComponent>;
  let productServiceSpy: jasmine.SpyObj<ProductService>;
  let cartServiceSpy: jasmine.SpyObj<CartService>;

  const mockProducts = [
    {
      ProductId: 1,
      Name: 'Mouse',
      Description: 'Wireless Mouse',
      Price: 500,
      ImageUrl: '',
      Stock: 10
    },
    {
      ProductId: 2,
      Name: 'Keyboard',
      Description: 'Mechanical Keyboard',
      Price: 1500,
      ImageUrl: '',
      Stock: 0
    }
  ];

  beforeEach(async () => {
    productServiceSpy = jasmine.createSpyObj('ProductService', ['getAllProducts']);
    cartServiceSpy = jasmine.createSpyObj('CartService', ['addToCart']);

    await TestBed.configureTestingModule({
      imports: [ProductsComponent],
      providers: [
        { provide: ProductService, useValue: productServiceSpy },
        { provide: CartService, useValue: cartServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            queryParams: of({})
          }
        }
      ],
      schemas: [NO_ERRORS_SCHEMA] // <-- THIS avoids RouterLink crash
    }).compileComponents();

    fixture = TestBed.createComponent(ProductsComponent);
    component = fixture.componentInstance;
  });

  // ---------------- BASIC ----------------
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ---------------- INIT ----------------
  it('should load products on init', () => {
    productServiceSpy.getAllProducts.and.returnValue(
      of({ success: true, data: mockProducts })
    );

    fixture.detectChanges();

    expect(component.products.length).toBe(2);
    expect(component.filteredProducts.length).toBe(2);
    expect(component.loading).toBeFalse();
  });

  it('should handle API error', () => {
    productServiceSpy.getAllProducts.and.returnValue(
      throwError(() => ({ status: 500 }))
    );

    fixture.detectChanges();

    expect(component.loading).toBeFalse();
    expect(component.errorMessage).toBeTruthy();
  });

  // ---------------- FILTERS ----------------
  it('should filter by search query', () => {
    component.products = mockProducts;
    component.searchQuery = 'mouse';

    component.applyFilters();

    expect(component.filteredProducts.length).toBe(1);
  });

  it('should filter in-stock only', () => {
    component.products = mockProducts;
    component.inStockOnly = true;

    component.applyFilters();

    expect(component.filteredProducts.length).toBe(1);
    expect(component.filteredProducts[0].Stock).toBeGreaterThan(0);
  });

  it('should filter by price range', () => {
    component.products = mockProducts;
    component.priceRange = '0-1000';

    component.applyFilters();

    expect(component.filteredProducts.length).toBe(1);
  });

  it('should sort by price ascending', () => {
    component.products = mockProducts;
    component.sortBy = 'price_asc';

    component.applyFilters();

    expect(component.filteredProducts[0].Price).toBe(500);
  });

  // ---------------- IMAGE ----------------
  it('should map image correctly', () => {
    const product = mockProducts[0];

    const image = component.getProductImage(product);

    expect(image).toContain('mouse');
  });

  it('should fallback to default image', () => {
    const product = {
      ProductId: 3,
      Name: 'Unknown Item',
      Description: '',
      Price: 100,
      ImageUrl: '',
      Stock: 1
    };

    const image = component.getProductImage(product as any);

    expect(image).toBe('assets/images/mouse.jpg');
  });

  // ---------------- CART ----------------
  it('should add to cart if in stock', () => {
    const product = mockProducts[0];

    component.addToCart(product);

    expect(cartServiceSpy.addToCart).toHaveBeenCalled();
    expect(component.toastMsg).toContain(product.Name);
  });

  it('should not add to cart if out of stock', () => {
    const product = mockProducts[1];

    component.addToCart(product);

    expect(cartServiceSpy.addToCart).not.toHaveBeenCalled();
  });

  // ---------------- CLEAR FILTER ----------------
  it('should clear filters', () => {
    component.sortBy = 'price_desc';
    component.inStockOnly = true;
    component.priceRange = '0-500';

    component.clearFilters();

    expect(component.sortBy).toBe('default');
    expect(component.inStockOnly).toBeFalse();
    expect(component.priceRange).toBe('all');
  });
});
