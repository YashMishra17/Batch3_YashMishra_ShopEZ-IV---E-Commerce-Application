import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Product, CreateProductRequest, UpdateProductRequest } from '../models/product.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // GET /api/products — No token required
  // Response: { success: true, data: Product[] }
  getAllProducts(): Observable<ApiResponse<Product[]>> {
    return this.http.get<ApiResponse<Product[]>>(`${this.apiUrl}/products`);
  }

  // GET /api/products/{id} — No token required
  // Response: { success: true, data: Product }
  getProductById(id: number): Observable<ApiResponse<Product>> {
    return this.http.get<ApiResponse<Product>>(`${this.apiUrl}/products/${id}`);
  }

  // POST /api/products — Admin token required
  // Request:  { Name, Description, Price, ImageUrl, Stock }
  // Response: { success: true, data: Product }
  createProduct(product: CreateProductRequest): Observable<ApiResponse<Product>> {
    return this.http.post<ApiResponse<Product>>(`${this.apiUrl}/products`, product);
  }

  // PUT /api/products/{id} — Admin token required
  // Request:  { Name, Description, Price, ImageUrl, Stock }
  // Response: { success: true, data: Product }
  updateProduct(id: number, product: UpdateProductRequest): Observable<ApiResponse<Product>> {
    return this.http.put<ApiResponse<Product>>(`${this.apiUrl}/products/${id}`, product);
  }

  // DELETE /api/products/{id} — Admin token required
  // Response: { success: true, message: "..." }
  deleteProduct(id: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/products/${id}`);
  }
}