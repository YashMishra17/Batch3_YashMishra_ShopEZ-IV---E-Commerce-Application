/// <reference types="jasmine" />

import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AdminComponent } from './admin.component';
import { ProductService } from '../../services/product.service';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
}

describe('AdminComponent', () => {
  let component: AdminComponent;
  let fixture: ComponentFixture<AdminComponent>;
  let productServiceSpy: jasmine.SpyObj<ProductService>;

  const mockProducts = [
    { ProductId: 1, Name: 'Mouse', Description: 'desc', Price: 100, ImageUrl: '', Stock: 5 },
    { ProductId: 2, Name: 'Keyboard', Description: 'desc', Price: 200, ImageUrl: '', Stock: 0 }
  ];

  beforeEach(async () => {
    productServiceSpy = jasmine.createSpyObj('ProductService', [
      'getAllProducts',
      'createProduct',
      'updateProduct',
      'deleteProduct'
    ]);

    await TestBed.configureTestingModule({
      imports: [AdminComponent, CommonModule, FormsModule],
      providers: [{ provide: ProductService, useValue: productServiceSpy }]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminComponent);
    component = fixture.componentInstance;
  });

  it('should call loadProducts on init', () => {
    spyOn(component, 'loadProducts');
    component.ngOnInit();
    expect(component.loadProducts).toHaveBeenCalled();
  });

  it('should load products successfully', () => {
    const response: ApiResponse<any> = { success: true, data: mockProducts };
    productServiceSpy.getAllProducts.and.returnValue(of(response));

    component.loadProducts();

    expect(component.loading).toBeFalse();
    expect(component.products).toEqual(mockProducts);
  });

  it('should handle loadProducts error', () => {
    productServiceSpy.getAllProducts.and.returnValue(throwError(() => ({})));

    component.loadProducts();

    expect(component.loading).toBeFalse();
  });

  it('should calculate inStockCount correctly', () => {
    component.products = mockProducts;
    expect(component.inStockCount).toBe(1);
  });

  it('should calculate outOfStockCount correctly', () => {
    component.products = mockProducts;
    expect(component.outOfStockCount).toBe(1);
  });

  it('should filter products by search', () => {
    component.products = mockProducts;
    component.tableSearch = 'mouse';
    expect(component.filteredTableProducts.length).toBe(1);
  });

  it('should return all products when no search', () => {
    component.products = mockProducts;
    component.tableSearch = '';
    expect(component.filteredTableProducts.length).toBe(2);
  });

  it('should not save product with invalid form', () => {
    component.form = { Name: '', Description: '', Price: 0, ImageUrl: '', Stock: -1 };

    component.saveProduct();

    expect(component.formError).toContain('mandatory');
    expect(productServiceSpy.createProduct).not.toHaveBeenCalled();
  });

  it('should create product successfully', fakeAsync(() => {
    const response: ApiResponse<any> = { success: true };
    productServiceSpy.createProduct.and.returnValue(of(response));
    spyOn(component, 'loadProducts');

    component.form = { Name: 'Test', Description: '', Price: 10, ImageUrl: '', Stock: 1 };

    component.saveProduct();
    tick();

    expect(component.saving).toBeFalse();
    expect(component.formSuccess).toContain('added successfully');
    expect(component.loadProducts).toHaveBeenCalled();
  }));

  it('should update product successfully', fakeAsync(() => {
    const response: ApiResponse<any> = { success: true };
    productServiceSpy.updateProduct.and.returnValue(of(response));
    spyOn(component, 'loadProducts');

    component.editingProduct = mockProducts[0];
    component.form = { Name: 'Test', Description: '', Price: 10, ImageUrl: '', Stock: 1 };

    component.saveProduct();
    tick();

    expect(component.formSuccess).toContain('updated successfully');
    expect(component.loadProducts).toHaveBeenCalled();
  }));

  it('should handle saveProduct error', fakeAsync(() => {
    productServiceSpy.createProduct.and.returnValue(
      throwError(() => ({ error: { message: 'fail' } }))
    );

    component.form = { Name: 'Test', Description: '', Price: 10, ImageUrl: '', Stock: 1 };

    component.saveProduct();
    tick();

    expect(component.formError).toBe('fail');
    expect(component.saving).toBeFalse();
  }));

  it('should start editing product', () => {
    spyOn(window, 'scrollTo');

    component.startEdit(mockProducts[0]);

    expect(component.editingProduct).toEqual(mockProducts[0]);
    expect(component.form.Name).toBe('Mouse');
    expect(window.scrollTo).toHaveBeenCalled();
  });

  it('should cancel edit', () => {
    component.editingProduct = mockProducts[0];

    component.cancelEdit();

    expect(component.editingProduct).toBeNull();
    expect(component.form.Name).toBe('');
  });

  it('should delete product when confirmed', fakeAsync(() => {
    spyOn(window, 'confirm').and.returnValue(true);
    const response: ApiResponse<any> = { success: true };
    productServiceSpy.deleteProduct.and.returnValue(of(response));
    spyOn(component, 'loadProducts');

    component.deleteProduct(1, 'Mouse');
    tick();

    expect(component.formSuccess).toContain('deleted successfully');
    expect(component.loadProducts).toHaveBeenCalled();
  }));

  it('should not delete product when not confirmed', () => {
    spyOn(window, 'confirm').and.returnValue(false);

    component.deleteProduct(1, 'Mouse');

    expect(productServiceSpy.deleteProduct).not.toHaveBeenCalled();
  });

  it('should handle delete error', fakeAsync(() => {
    spyOn(window, 'confirm').and.returnValue(true);
    productServiceSpy.deleteProduct.and.returnValue(
      throwError(() => ({ error: { message: 'error' } }))
    );

    component.deleteProduct(1, 'Mouse');
    tick();

    expect(component.formError).toBe('error');
  }));

  it('should return mapped image based on product name', () => {
    const result = component.getProductImage(mockProducts[0]);
    expect(result).toContain('mouse.jpg');
  });

  it('should return fallback image if no match and no ImageUrl', () => {
    const product = { ProductId: 3, Name: 'Unknown', Description: '', Price: 10, ImageUrl: '', Stock: 1 };
    const result = component.getProductImage(product as any);
    expect(result).toContain('mouse.jpg');
  });

  it('should return ImageUrl if no match but ImageUrl exists', () => {
    const product = { ProductId: 3, Name: 'Unknown', Description: '', Price: 10, ImageUrl: 'custom.jpg', Stock: 1 };
    const result = component.getProductImage(product as any);
    expect(result).toBe('custom.jpg');
  });
});