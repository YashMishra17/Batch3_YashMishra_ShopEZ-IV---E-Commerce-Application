import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateOrderRequest, Order } from '../models/order.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // POST /api/orders — Token required (Admin or Customer)
  // Request:  { UserId: number, CartItems: [{ ProductId, Quantity }] }
  // Response: { success: true, data: Order }
  createOrder(order: CreateOrderRequest): Observable<ApiResponse<Order>> {
    return this.http.post<ApiResponse<Order>>(`${this.apiUrl}/orders`, order);
  }

  // GET /api/orders — Token required (Admin only)
  // Response: { success: true, data: Order[] }
  getAllOrders(): Observable<ApiResponse<Order[]>> {
    return this.http.get<ApiResponse<Order[]>>(`${this.apiUrl}/orders`);
  }

  // GET /api/orders/{id} — Token required (Admin or Customer)
  // Response: { success: true, data: Order }
  getOrderById(id: number): Observable<ApiResponse<Order>> {
    return this.http.get<ApiResponse<Order>>(`${this.apiUrl}/orders/${id}`);
  }
}