import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { CartProduct } from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {

  private readonly CART_KEY = 'shopez_cart';

  private cartSubject = new BehaviorSubject<CartProduct[]>(this.loadCart());
  public cart$ = this.cartSubject.asObservable();

  // Add product to cart or increase quantity
  addToCart(product: CartProduct): void {
    const cart = this.loadCart();
    const existing = cart.find(item => item.ProductId === product.ProductId);

    if (existing) {
      // Check against available stock before adding
      if (existing.Quantity < product.Stock) {
        existing.Quantity++;
      }
    } else {
      cart.push({ ...product, Quantity: 1 });
    }

    this.saveCart(cart);
  }

  // Remove product from cart completely
  removeFromCart(productId: number): void {
    const cart = this.loadCart().filter(item => item.ProductId !== productId);
    this.saveCart(cart);
  }

  // Increase quantity by 1
  increaseQuantity(productId: number): void {
    const cart = this.loadCart();
    const item = cart.find(i => i.ProductId === productId);
    if (item && item.Quantity < item.Stock) {
      item.Quantity++;
      this.saveCart(cart);
    }
  }

  // Decrease quantity by 1 (remove if reaches 0)
  decreaseQuantity(productId: number): void {
    const cart = this.loadCart();
    const item = cart.find(i => i.ProductId === productId);
    if (item) {
      item.Quantity--;
      if (item.Quantity <= 0) {
        this.removeFromCart(productId);
        return;
      }
    }
    this.saveCart(cart);
  }

  // Get total item count for navbar badge
  getCartCount(): number {
    return this.loadCart().reduce((sum, item) => sum + item.Quantity, 0);
  }

  // Get total price
  getCartTotal(): number {
    return this.loadCart().reduce((sum, item) => sum + item.Price * item.Quantity, 0);
  }

  // Get all cart items
  getCart(): CartProduct[] {
    return this.loadCart();
  }

  // Clear entire cart (after order placed)
  clearCart(): void {
    localStorage.removeItem(this.CART_KEY);
    this.cartSubject.next([]);
  }

  private loadCart(): CartProduct[] {
    const stored = localStorage.getItem(this.CART_KEY);
    return stored ? JSON.parse(stored) : [];
  }

  private saveCart(cart: CartProduct[]): void {
    localStorage.setItem(this.CART_KEY, JSON.stringify(cart));
    this.cartSubject.next(cart);
  }
}