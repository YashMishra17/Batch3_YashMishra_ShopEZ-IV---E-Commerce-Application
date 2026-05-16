import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { OrderService } from '../../services/order.service';
import { AuthService } from '../../services/auth.service';
import { CartProduct } from '../../models/order.model';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page-wrapper">
      <div class="container-shopez py-4">

        <!-- Breadcrumb -->
        <nav class="shopez-breadcrumb mb-4">
          <a routerLink="/">Home</a>
          <i class="bi bi-chevron-right"></i>
          <a routerLink="/cart">Cart</a>
          <i class="bi bi-chevron-right"></i>
          <span>Checkout</span>
        </nav>

        <!-- Login warning -->
        <div class="alert-shopez alert-info mb-4" *ngIf="!isLoggedIn">
          <i class="bi bi-info-circle me-2"></i>
          Please <a routerLink="/login" style="color:#1565c0;font-weight:700;">login</a> to place an order.
        </div>

        <div class="checkout-layout">

          <!-- Left: Delivery + Payment -->
          <div class="checkout-main">

            <!-- Step 1: Delivery Address -->
            <div class="checkout-step shopez-card mb-3">
              <div class="step-header">
                <span class="step-num">1</span>
                <h5 class="step-title">Delivery Address</h5>
              </div>
              <div class="step-body">
                <div class="row g-3">
                  <div class="col-md-6">
                    <div class="shopez-form-group">
                      <label class="shopez-label">Full Name *</label>
                      <input type="text" class="shopez-input" [(ngModel)]="name" placeholder="Enter your full name" />
                    </div>
                  </div>
                  <div class="col-md-6">
                    <div class="shopez-form-group">
                      <label class="shopez-label">Email *</label>
                      <input type="email" class="shopez-input" [(ngModel)]="email" placeholder="Enter your email" />
                    </div>
                  </div>
                  <div class="col-md-6">
                    <div class="shopez-form-group">
                      <label class="shopez-label">Phone Number *</label>
                      <input type="tel" class="shopez-input" [(ngModel)]="phone" placeholder="10-digit mobile number" />
                    </div>
                  </div>
                  <div class="col-md-6">
                    <div class="shopez-form-group">
                      <label class="shopez-label">PIN Code *</label>
                      <input type="text" class="shopez-input" [(ngModel)]="pincode" placeholder="6-digit PIN code" />
                    </div>
                  </div>
                  <div class="col-12">
                    <div class="shopez-form-group">
                      <label class="shopez-label">Address (House No., Building, Street, Area) *</label>
                      <textarea class="shopez-input" [(ngModel)]="address" rows="3" placeholder="Enter complete address"></textarea>
                    </div>
                  </div>
                  <div class="col-md-6">
                    <div class="shopez-form-group">
                      <label class="shopez-label">City *</label>
                      <input type="text" class="shopez-input" [(ngModel)]="city" placeholder="City" />
                    </div>
                  </div>
                  <div class="col-md-6">
                    <div class="shopez-form-group">
                      <label class="shopez-label">State *</label>
                      <select class="shopez-input" [(ngModel)]="state">
                        <option value="">Select State</option>
                        <option *ngFor="let s of states" [value]="s">{{ s }}</option>
                      </select>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Step 2: Payment -->
            <div class="checkout-step shopez-card mb-3">
              <div class="step-header">
                <span class="step-num">2</span>
                <h5 class="step-title">Payment Method</h5>
              </div>
              <div class="step-body">
                <div class="payment-options">
                  <label class="payment-option" [class.selected]="paymentMethod==='cod'" (click)="paymentMethod='cod'">
                    <input type="radio" name="payment" value="cod" [(ngModel)]="paymentMethod" />
                    <i class="bi bi-cash-stack payment-icon"></i>
                    <div>
                      <strong>Cash on Delivery</strong>
                      <p>Pay when your order arrives</p>
                    </div>
                  </label>
                  <label class="payment-option" [class.selected]="paymentMethod==='upi'" (click)="paymentMethod='upi'">
                    <input type="radio" name="payment" value="upi" [(ngModel)]="paymentMethod" />
                    <i class="bi bi-phone payment-icon"></i>
                    <div>
                      <strong>UPI</strong>
                      <p>Google Pay, PhonePe, Paytm</p>
                    </div>
                  </label>
                  <label class="payment-option" [class.selected]="paymentMethod==='card'" (click)="paymentMethod='card'">
                    <input type="radio" name="payment" value="card" [(ngModel)]="paymentMethod" />
                    <i class="bi bi-credit-card payment-icon"></i>
                    <div>
                      <strong>Credit / Debit Card</strong>
                      <p>Visa, Mastercard, RuPay</p>
                    </div>
                  </label>
                </div>
              </div>
            </div>

            <!-- Step 3: Review Items -->
            <div class="checkout-step shopez-card mb-3">
              <div class="step-header">
                <span class="step-num">3</span>
                <h5 class="step-title">Review Items</h5>
              </div>
              <div class="step-body p-0">
                <div class="review-item" *ngFor="let item of cartItems">
                  <img [src]="item.ImageUrl" [alt]="item.Name" class="review-item-img" />
                  <div class="review-item-info">
                    <span class="review-item-name">{{ item.Name }}</span>
                    <span class="review-item-qty text-muted">Qty: {{ item.Quantity }}</span>
                  </div>
                  <div class="review-item-price">₹{{ item.Price * item.Quantity | number:'1.0-0' }}</div>
                </div>
              </div>
            </div>

            <!-- Error -->
            <div class="alert-shopez alert-error" *ngIf="errorMessage">
              <i class="bi bi-exclamation-triangle me-2"></i>{{ errorMessage }}
            </div>

          </div>

          <!-- Right: Price Details -->
          <div class="checkout-sidebar">
            <div class="shopez-card order-summary-card sticky-top" style="top:80px;">
              <div class="shopez-card-header">Price Details</div>
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
                  <span>Secured Packaging</span>
                  <span>₹29</span>
                </div>
                <hr style="border-color:#f0f0f0;" />
                <div class="summary-row total-row">
                  <strong>Total Amount</strong>
                  <strong>₹{{ finalTotal | number:'1.0-0' }}</strong>
                </div>
                <div class="saving-notice mt-2 mb-3">
                  <i class="bi bi-piggy-bank-fill text-success me-1"></i>
                  You save ₹{{ (total * 0.05) | number:'1.0-0' }} on this order
                </div>

                <button
                  class="btn-shopez btn-success-shopez btn-lg-shopez btn-block"
                  (click)="placeOrder()"
                  [disabled]="loading || cartItems.length === 0"
                >
                  <span *ngIf="!loading"><i class="bi bi-lock-fill me-1"></i> Place Order</span>
                  <span *ngIf="loading">
                    <span class="spinner-border spinner-border-sm me-2"></span>Placing Order...
                  </span>
                </button>

                <p class="secure-text mt-2">
                  <i class="bi bi-shield-lock-fill text-success me-1"></i>
                  Safe and Secure Payments. Easy returns. 100% Authentic products.
                </p>
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
    .checkout-layout { display:grid; grid-template-columns:1fr 320px; gap:16px; align-items:start; }
    .checkout-step { overflow:hidden; }
    .step-header {
      display:flex; align-items:center; gap:12px;
      padding:14px 20px; background:#2874f0; color:#fff;
    }
    .step-num {
      width:28px; height:28px; border-radius:50%;
      background:#fff; color:#2874f0;
      display:flex; align-items:center; justify-content:center;
      font-weight:800; font-size:14px; flex-shrink:0;
    }
    .step-title { margin:0; font-size:15px; font-weight:700; }
    .step-body { padding:20px; }
    /* Payment options */
    .payment-options { display:flex; flex-direction:column; gap:10px; }
    .payment-option {
      display:flex; align-items:center; gap:14px;
      padding:14px 16px; border:1.5px solid #e0e0e0;
      border-radius:8px; cursor:pointer; transition:all .2s;
    }
    .payment-option:hover, .selected { border-color:#2874f0; background:#f5f8ff; }
    .payment-option input[type=radio] { accent-color:#2874f0; width:16px; height:16px; flex-shrink:0; }
    .payment-icon { font-size:24px; color:#2874f0; }
    .payment-option strong { font-size:14px; font-weight:700; display:block; }
    .payment-option p { font-size:12px; color:#757575; margin:0; }
    /* Review items */
    .review-item {
      display:flex; align-items:center; gap:12px;
      padding:12px 16px; border-bottom:1px solid #f0f0f0;
    }
    .review-item:last-child { border-bottom:none; }
    .review-item-img { width:60px; height:60px; object-fit:contain; border:1px solid #f0f0f0; border-radius:4px; }
    .review-item-info { flex:1; display:flex; flex-direction:column; gap:4px; }
    .review-item-name { font-size:14px; font-weight:600; color:#212121; }
    .review-item-qty { font-size:12px; }
    .review-item-price { font-size:15px; font-weight:700; color:#212121; }
    /* Summary */
    .summary-row { display:flex; justify-content:space-between; padding:8px 0; font-size:14px; }
    .total-row { font-size:16px; padding:12px 0; }
    .saving-notice { background:#e8f5e9; color:#2e7d32; padding:10px 12px; border-radius:4px; font-size:13px; font-weight:600; }
    .secure-text { font-size:11px; color:#757575; text-align:center; line-height:1.5; }
    @media(max-width:768px){
      .checkout-layout { grid-template-columns:1fr; }
    }
  `]
})
export class CheckoutComponent implements OnInit {
  cartItems: CartProduct[] = [];
  total = 0;
  totalItems = 0;
  finalTotal = 0;
  loading = false;
  errorMessage = '';
  isLoggedIn = false;

  name = ''; email = ''; phone = '';
  address = ''; city = ''; state = ''; pincode = '';
  paymentMethod = 'cod';

  states = ['Andhra Pradesh','Delhi','Gujarat','Karnataka','Kerala','Madhya Pradesh','Maharashtra','Punjab','Rajasthan','Tamil Nadu','Telangana','Uttar Pradesh','West Bengal'];

  constructor(
    private cartService: CartService,
    private orderService: OrderService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn();
    this.cartItems = this.cartService.getCart();
    this.total = this.cartService.getCartTotal();
    this.totalItems = this.cartService.getCartCount();
    this.finalTotal = Math.round(this.total - (this.total * 0.05) + 29);

    const user = this.authService.getCurrentUser();
    this.name  = user?.Name ?? '';
    this.email = user?.Email ?? '';
  }

  placeOrder(): void {
    if (!this.isLoggedIn) { this.router.navigate(['/login']); return; }
    if (!this.name || !this.address || !this.phone) {
      this.errorMessage = 'Please fill in Name, Phone, and Address.'; return;
    }
    this.loading = true; this.errorMessage = '';

    this.orderService.createOrder({
      UserId: this.authService.getUserId(),
      CartItems: this.cartItems.map(i => ({ ProductId: i.ProductId, Quantity: i.Quantity }))
    }).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          this.cartService.clearCart();
          this.router.navigate(['/order-success', res.data.OrderId]);
        }
      },
      error: (err) => {
        this.loading = false;
        if (err.status === 401) { this.errorMessage = 'Session expired. Please login.'; this.router.navigate(['/login']); }
        else this.errorMessage = err.error?.message ?? 'Failed to place order. Please try again.';
      }
    });
  }
}