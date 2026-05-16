import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { Order } from '../../models/order.model';

@Component({
  selector: 'app-order-success',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page-wrapper">
      <div class="container-shopez py-5">
        <div class="success-wrapper">

          <!-- Success Card -->
          <div class="shopez-card success-card">

            <!-- Header -->
            <div class="success-header">
              <div class="success-icon-wrapper">
                <div class="success-icon">✓</div>
              </div>
              <h2 class="success-title">Order Placed Successfully!</h2>
              <p class="success-sub">
                Thank you for shopping with ShopEZ 🎉<br>
                Your Order ID: <strong>#{{ orderId }}</strong>
              </p>
            </div>

            <!-- Order Timeline -->
            <div class="order-timeline">
              <div class="timeline-step active">
                <div class="timeline-dot"><i class="bi bi-check-lg"></i></div>
                <div class="timeline-label">Order Placed</div>
                <div class="timeline-time">Just now</div>
              </div>
              <div class="timeline-line"></div>
              <div class="timeline-step">
                <div class="timeline-dot timeline-dot-pending"><i class="bi bi-box"></i></div>
                <div class="timeline-label">Processing</div>
                <div class="timeline-time">1-2 hours</div>
              </div>
              <div class="timeline-line"></div>
              <div class="timeline-step">
                <div class="timeline-dot timeline-dot-pending"><i class="bi bi-truck"></i></div>
                <div class="timeline-label">Shipped</div>
                <div class="timeline-time">1-2 days</div>
              </div>
              <div class="timeline-line"></div>
              <div class="timeline-step">
                <div class="timeline-dot timeline-dot-pending"><i class="bi bi-house"></i></div>
                <div class="timeline-label">Delivered</div>
                <div class="timeline-time">3-5 days</div>
              </div>
            </div>

            <!-- Order Details -->
            <div class="order-details-section" *ngIf="order">
              <h5 class="order-detail-title">
                <i class="bi bi-receipt me-2"></i>Order Summary
              </h5>

              <div class="order-items-list">
                <div class="order-item-row" *ngFor="let item of order.OrderItems">
                  <div class="order-item-info">
                    <span class="order-item-name">{{ item.ProductName }}</span>
                    <span class="order-item-qty">Qty: {{ item.Quantity }}</span>
                  </div>
                  <div class="order-item-price">₹{{ item.Subtotal | number:'1.0-0' }}</div>
                </div>
              </div>

              <div class="order-total-row">
                <span>Order Total</span>
                <strong>₹{{ order.TotalAmount | number:'1.0-0' }}</strong>
              </div>

              <div class="order-meta">
                <div class="order-meta-item">
                  <i class="bi bi-calendar3 text-primary"></i>
                  <span>Ordered: {{ order.OrderDate | date:'dd MMM yyyy, hh:mm a' }}</span>
                </div>
                <div class="order-meta-item">
                  <i class="bi bi-person-circle text-primary"></i>
                  <span>Customer: {{ order.UserName || 'You' }}</span>
                </div>
              </div>
            </div>

            <!-- CTA Buttons -->
            <div class="success-actions">
              <a routerLink="/products" class="btn-shopez btn-primary-shopez btn-lg-shopez">
                <i class="bi bi-bag-fill me-1"></i> Continue Shopping
              </a>
              <a routerLink="/" class="btn-shopez btn-outline-shopez btn-lg-shopez">
                <i class="bi bi-house me-1"></i> Go to Home
              </a>
            </div>

            <!-- Trust footer -->
            <div class="success-trust">
              <div class="trust-item">
                <i class="bi bi-shield-check text-success"></i>
                <span>Secure Order</span>
              </div>
              <div class="trust-item">
                <i class="bi bi-truck text-primary"></i>
                <span>Fast Delivery</span>
              </div>
              <div class="trust-item">
                <i class="bi bi-arrow-return-left text-warning"></i>
                <span>Easy Returns</span>
              </div>
            </div>

          </div>
        </div>
      </div>

      <footer class="shopez-footer">
        <div class="footer-bottom" style="margin-top:0;padding-top:20px;">
          <p>© 2026 ShopEZ Technologies Pvt. Ltd.</p>
        </div>
      </footer>
    </div>
  `,
  styles: [`
    .success-wrapper { max-width: 640px; margin: 0 auto; }

    /* Success card */
    .success-card { overflow: hidden; }
    .success-header {
      background: linear-gradient(135deg, #2874f0 0%, #0d3b6e 100%);
      padding: 40px 32px; text-align: center; color: #fff;
    }
    .success-icon-wrapper {
      width: 80px; height: 80px; border-radius: 50%;
      background: rgba(255,255,255,.2);
      display: flex; align-items: center; justify-content: center;
      margin: 0 auto 16px;
      border: 3px solid rgba(255,255,255,.5);
    }
    .success-icon {
      width: 56px; height: 56px; border-radius: 50%;
      background: #fff; color: #2874f0;
      display: flex; align-items: center; justify-content: center;
      font-size: 28px; font-weight: 900;
    }
    .success-title { font-size: 24px; font-weight: 800; margin-bottom: 8px; }
    .success-sub { font-size: 14px; opacity: .9; line-height: 1.7; }

    /* Timeline */
    .order-timeline {
      display: flex; align-items: center;
      padding: 24px 32px;
      background: #f8f9fa;
      border-bottom: 1px solid #f0f0f0;
    }
    .timeline-step { display: flex; flex-direction: column; align-items: center; gap: 6px; flex: 0; }
    .timeline-dot {
      width: 36px; height: 36px; border-radius: 50%;
      background: #2874f0; color: #fff;
      display: flex; align-items: center; justify-content: center;
      font-size: 16px;
    }
    .timeline-dot-pending { background: #e0e0e0; color: #9e9e9e; }
    .timeline-label { font-size: 11px; font-weight: 700; color: #424242; white-space: nowrap; }
    .timeline-time  { font-size: 10px; color: #9e9e9e; }
    .timeline-line { flex: 1; height: 2px; background: #e0e0e0; margin-bottom: 24px; }

    /* Order details */
    .order-details-section { padding: 24px; }
    .order-detail-title { font-size: 15px; font-weight: 700; color: #212121; margin-bottom: 16px; }
    .order-items-list { border: 1px solid #f0f0f0; border-radius: 8px; overflow: hidden; margin-bottom: 12px; }
    .order-item-row {
      display: flex; justify-content: space-between; align-items: center;
      padding: 12px 16px; border-bottom: 1px solid #f5f5f5;
    }
    .order-item-row:last-child { border-bottom: none; }
    .order-item-info { display: flex; flex-direction: column; gap: 2px; }
    .order-item-name { font-size: 14px; font-weight: 600; color: #212121; }
    .order-item-qty  { font-size: 12px; color: #757575; }
    .order-item-price { font-size: 15px; font-weight: 700; color: #212121; }
    .order-total-row {
      display: flex; justify-content: space-between;
      padding: 12px 0; font-size: 16px;
      border-top: 2px solid #f0f0f0; margin-bottom: 16px;
    }
    .order-meta { display: flex; flex-direction: column; gap: 8px; }
    .order-meta-item { display: flex; align-items: center; gap: 8px; font-size: 13px; color: #555; }

    /* CTA */
    .success-actions {
      display: flex; gap: 12px; flex-wrap: wrap;
      padding: 0 24px 24px; justify-content: center;
    }

    /* Trust */
    .success-trust {
      display: flex; justify-content: center; gap: 32px;
      padding: 16px 24px;
      background: #f8f9fa;
      border-top: 1px solid #f0f0f0;
    }
    .trust-item { display: flex; align-items: center; gap: 6px; font-size: 12px; font-weight: 600; color: #424242; }
    .trust-item i { font-size: 18px; }

    @media(max-width: 480px) {
      .order-timeline { padding: 16px; gap: 0; }
      .timeline-label, .timeline-time { display: none; }
      .success-actions { flex-direction: column; }
    }
  `]
})
export class OrderSuccessComponent implements OnInit {
  orderId = 0;
  order: Order | null = null;

  constructor(private route: ActivatedRoute, private orderService: OrderService) {}

  ngOnInit(): void {
    this.orderId = Number(this.route.snapshot.paramMap.get('id'));
    this.orderService.getOrderById(this.orderId).subscribe({
      next: (res) => { if (res.success) this.order = res.data!; },
      error: () => {}
    });
  }
}