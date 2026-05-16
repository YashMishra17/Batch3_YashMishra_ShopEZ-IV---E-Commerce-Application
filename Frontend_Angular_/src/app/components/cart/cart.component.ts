import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { CartProduct } from '../../models/order.model';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page-wrapper">
      <div class="container-shopez py-4">

        <!-- Breadcrumb -->
        <nav class="shopez-breadcrumb mb-4">
          <a routerLink="/">Home</a>
          <i class="bi bi-chevron-right"></i>
          <span>Shopping Cart</span>
        </nav>

        <h2 class="section-title">Shopping Cart <span class="cart-count-text">({{ cartItems.length }} items)</span></h2>

        <!-- Empty Cart -->
        <div class="empty-cart-page" *ngIf="cartItems.length === 0">
          <div class="empty-cart-icon">🛒</div>
          <h3>Your cart is empty!</h3>
          <p class="text-muted">Add items to it now.</p>
          <a routerLink="/products" class="btn-shopez btn-primary-shopez btn-lg-shopez mt-3">
            <i class="bi bi-shop"></i> Shop Now
          </a>
        </div>

        <!-- Cart Layout -->
        <div class="cart-layout" *ngIf="cartItems.length > 0">

          <!-- Cart Items -->
          <div class="cart-items-col">

            <!-- Delivery Notice -->
            <div class="delivery-notice">
              <i class="bi bi-truck-front text-success me-2"></i>
              <span>Your order is eligible for <strong>FREE Delivery</strong>!</span>
            </div>

            <!-- Each Cart Item -->
            <div class="cart-item-card shopez-card mb-3" *ngFor="let item of cartItems">
              <div class="cart-item-layout">

                <!-- Image -->
                <a [routerLink]="['/products', item.ProductId]" class="cart-item-img-link">
                  <img [src]="item.ImageUrl" [alt]="item.Name" class="cart-item-img" />
                </a>

                <!-- Info -->
                <div class="cart-item-info">
                  <a [routerLink]="['/products', item.ProductId]" class="cart-item-name">{{ item.Name }}</a>
                  <div class="cart-item-meta">
                    <span class="badge-stock-in">In Stock</span>
                  </div>
                  <div class="cart-item-offer">
                    <i class="bi bi-tag text-success me-1"></i>
                    Free delivery on this item
                  </div>

                  <!-- Qty Controls -->
                  <div class="cart-item-actions mt-2">
                    <div class="qty-control">
                      <button class="qty-btn" (click)="decrease(item.ProductId)">−</button>
                      <span class="qty-value">{{ item.Quantity }}</span>
                      <button class="qty-btn" (click)="increase(item.ProductId)">+</button>
                    </div>
                    <button class="cart-action-btn text-danger" (click)="remove(item.ProductId)">
                      <i class="bi bi-trash3"></i> Delete
                    </button>
                    <button class="cart-action-btn text-primary">
                      <i class="bi bi-heart"></i> Save for later
                    </button>
                  </div>
                </div>

                <!-- Price -->
                <div class="cart-item-price-col">
                  <div class="cart-item-price">₹{{ item.Price * item.Quantity | number:'1.0-0' }}</div>
                  <div class="cart-item-unit-price text-muted small">₹{{ item.Price }} each</div>
                </div>

              </div>
            </div>

            <!-- Subtotal -->
            <div class="shopez-card cart-subtotal-bar">
              <span>Subtotal ({{ totalItems }} items):</span>
              <strong class="ms-2" style="font-size:18px;">₹{{ total | number:'1.0-0' }}</strong>
            </div>

          </div>

          <!-- Order Summary Sidebar -->
          <div class="cart-summary-col">
            <div class="shopez-card order-summary-card">
              <div class="shopez-card-header">Order Summary</div>
              <div class="shopez-card-body">

                <div class="summary-row">
                  <span>Price ({{ totalItems }} items)</span>
                  <span>₹{{ total | number:'1.0-0' }}</span>
                </div>
                <div class="summary-row text-success">
                  <span>Discount</span>
                  <span>− ₹{{ (total * 0.05) | number:'1.0-0' }}</span>
                </div>
                <div class="summary-row">
                  <span>Delivery Charges</span>
                  <span class="text-success">FREE</span>
                </div>
                <div class="summary-row">
                  <span>Secured Packaging Fee</span>
                  <span>₹29</span>
                </div>

                <hr style="border-color:#f0f0f0;" />

                <div class="summary-row total-row">
                  <strong>Total Amount</strong>
                  <strong>₹{{ (total - (total * 0.05) + 29) | number:'1.0-0' }}</strong>
                </div>

                <div class="saving-notice">
                  <i class="bi bi-piggy-bank-fill text-success me-1"></i>
                  You will save ₹{{ (total * 0.05) | number:'1.0-0' }} on this order
                </div>

                <button class="btn-shopez btn-success-shopez btn-lg-shopez btn-block mt-3" (click)="checkout()">
                  <i class="bi bi-lock-fill me-1"></i> Proceed to Checkout
                </button>

                <div class="secure-notice mt-3">
                  <i class="bi bi-shield-lock-fill text-success me-1"></i>
                  Safe and Secure Payments
                </div>

              </div>
            </div>
          </div>
        </div>

      </div>

      <!-- Footer -->
      <footer class="shopez-footer">
        <div class="footer-bottom" style="margin-top:0;padding-top:20px;">
          <p>© 2026 ShopEZ Technologies Pvt. Ltd.</p>
        </div>
      </footer>
    </div>
  `,
  styles: [`
    .shopez-breadcrumb { display:flex; align-items:center; gap:6px; font-size:13px; color:#757575; }
    .shopez-breadcrumb a { color:#2874f0; }
    .cart-count-text { font-size:16px; font-weight:400; color:#757575; }

    .empty-cart-page { text-align:center; padding:80px 20px; }
    .empty-cart-icon { font-size:80px; margin-bottom:16px; }

    /* Cart layout */
    .cart-layout { display:grid; grid-template-columns:1fr 320px; gap:16px; align-items:start; }
    .cart-items-col {}
    .cart-summary-col { position:sticky; top:80px; }

    /* Delivery notice */
    .delivery-notice {
      background:#f0fff4; border:1px solid #c6f6d5;
      border-radius:4px; padding:12px 16px;
      font-size:13px; margin-bottom:16px;
    }

    /* Cart item card */
    .cart-item-layout { display:flex; gap:16px; padding:16px; }
    .cart-item-img-link { flex-shrink:0; }
    .cart-item-img { width:120px; height:120px; object-fit:contain; border-radius:4px; border:1px solid #f0f0f0; }
    .cart-item-info { flex:1; }
    .cart-item-name { font-size:15px; font-weight:600; color:#212121; display:block; margin-bottom:6px; }
    .cart-item-name:hover { color:#2874f0; }
    .cart-item-meta { margin-bottom:4px; }
    .cart-item-offer { font-size:12px; color:#388e3c; margin-bottom:8px; }
    .cart-item-actions { display:flex; align-items:center; gap:12px; flex-wrap:wrap; }
    .cart-action-btn {
      background:none; border:none;
      font-size:13px; font-weight:600;
      padding:0; cursor:pointer;
      display:flex; align-items:center; gap:4px;
    }
    .cart-action-btn:hover { text-decoration:underline; }
    .cart-item-price-col { flex-shrink:0; text-align:right; }
    .cart-item-price { font-size:20px; font-weight:700; color:#212121; }
    .cart-item-unit-price { font-size:12px; }

    /* Subtotal bar */
    .cart-subtotal-bar {
      padding:14px 20px;
      display:flex; align-items:center; justify-content:flex-end;
      font-size:15px;
    }

    /* Summary */
    .order-summary-card {}
    .summary-row { display:flex; justify-content:space-between; padding:8px 0; font-size:14px; }
    .total-row { font-size:16px; padding:12px 0; }
    .saving-notice {
      background:#e8f5e9; color:#2e7d32;
      padding:10px 12px; border-radius:4px;
      font-size:13px; font-weight:600; margin-top:12px;
    }
    .secure-notice { text-align:center; font-size:12px; color:#757575; }

    @media(max-width:768px){
      .cart-layout { grid-template-columns:1fr; }
      .cart-summary-col { position:static; }
      .cart-item-layout { flex-wrap:wrap; }
      .cart-item-img { width:80px; height:80px; }
    }
  `]
})
export class CartComponent implements OnInit {
  cartItems: CartProduct[] = [];
  total = 0;
  totalItems = 0;

  constructor(private cartService: CartService, private router: Router) {}

  ngOnInit(): void {
    this.cartService.cart$.subscribe(items => {
      this.cartItems = items;
      this.total = this.cartService.getCartTotal();
      this.totalItems = this.cartService.getCartCount();
    });
  }

  increase(productId: number): void { this.cartService.increaseQuantity(productId); }
  decrease(productId: number): void { this.cartService.decreaseQuantity(productId); }
  remove(productId: number): void   { this.cartService.removeFromCart(productId); }
  checkout(): void { this.router.navigate(['/checkout']); }
}