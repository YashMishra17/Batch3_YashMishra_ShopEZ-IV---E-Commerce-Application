import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { Product } from '../../models/product.model';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page-wrapper">
      <div class="container-shopez py-4">

        <!-- Breadcrumb -->
        <nav class="shopez-breadcrumb mb-4">
          <a routerLink="/">Home</a>
          <i class="bi bi-chevron-right"></i>
          <a routerLink="/products">Products</a>
          <i class="bi bi-chevron-right"></i>
          <span>{{ product?.Name || 'Loading...' }}</span>
        </nav>

        <!-- Loading -->
        <div class="shopez-spinner" *ngIf="loading">
          <div class="spinner"></div>
        </div>

        <!-- Error -->
        <div class="alert-shopez alert-error" *ngIf="errorMessage">
          <i class="bi bi-exclamation-triangle me-2"></i>{{ errorMessage }}
        </div>

        <!-- Product Detail Card -->
        <div class="shopez-card product-detail-card" *ngIf="product && !loading">
          <div class="product-detail-layout">

            <!-- Left: Image Gallery -->
            <div class="detail-img-col">
              <div class="main-img-wrapper">
                <img [src]="productImage" [alt]="product.Name" class="main-product-img" />
                <div class="img-actions">
                  <button class="img-action-btn" title="Wishlist">
                    <i class="bi bi-heart"></i>
                  </button>
                  <button class="img-action-btn" title="Share">
                    <i class="bi bi-share"></i>
                  </button>
                </div>
              </div>
              <!-- Thumbnail strip -->
              <div class="thumb-strip">
                <img [src]="productImage" class="thumb-img active-thumb" [alt]="product.Name" />
              </div>
              <!-- Action buttons under image -->
              <div class="under-img-actions" *ngIf="product.Stock > 0">
                <button class="btn-shopez btn-secondary-shopez btn-lg-shopez btn-block mb-2" (click)="addToCart()">
                  <i class="bi bi-cart-plus"></i> Add to Cart
                </button>
                <button class="btn-shopez btn-success-shopez btn-lg-shopez btn-block" (click)="buyNow()">
                  <i class="bi bi-lightning-fill"></i> Buy Now
                </button>
              </div>
              <div class="under-img-actions" *ngIf="product.Stock === 0">
                <button class="btn-shopez btn-disabled-shopez btn-lg-shopez btn-block" disabled>
                  <i class="bi bi-x-circle"></i> Out of Stock
                </button>
              </div>
            </div>

            <!-- Right: Product Info -->
            <div class="detail-info-col">

              <!-- Title -->
              <h1 class="detail-title">{{ product.Name }}</h1>

              <!-- Rating -->
              <div class="detail-rating-row">
                <span class="stars-lg">★★★★★</span>
                <span class="rating-count ms-2">{{ 120 + product.ProductId * 37 }} ratings</span>
                <span class="ms-2 text-muted">|</span>
                <span class="ms-2 review-link">{{ 40 + product.ProductId * 11 }} reviews</span>
              </div>

              <hr class="detail-divider" />

              <!-- Price Block -->
              <div class="detail-price-block">
                <div class="detail-price">
                  <span class="detail-currency">₹</span>
                  <span class="detail-price-val">{{ product.Price | number:'1.0-0' }}</span>
                </div>
                <div class="detail-price-meta">
                  <span class="price-original-lg">M.R.P.: <s>₹{{ (product.Price * 1.25) | number:'1.0-0' }}</s></span>
                  <span class="detail-discount ms-2">20% off</span>
                </div>
                <p class="detail-offer-text">
                  <i class="bi bi-tag-fill text-success me-1"></i>
                  No Cost EMI available from ₹{{ (product.Price / 12) | number:'1.0-0' }}/month
                </p>
              </div>

              <hr class="detail-divider" />

              <!-- Stock Status -->
              <div class="detail-stock mb-3">
                <span class="badge-stock-in fs-6"  *ngIf="product.Stock > 0">
                  <i class="bi bi-check-circle me-1"></i>In Stock ({{ product.Stock }} units available)
                </span>
                <span class="badge-stock-out fs-6" *ngIf="product.Stock === 0">
                  <i class="bi bi-x-circle me-1"></i>Currently Out of Stock
                </span>
              </div>

              <!-- Quantity Selector -->
              <div class="detail-qty-row mb-4" *ngIf="product.Stock > 0">
                <span class="detail-label">Quantity:</span>
                <div class="qty-control">
                  <button class="qty-btn" (click)="decreaseQty()">−</button>
                  <span class="qty-value">{{ quantity }}</span>
                  <button class="qty-btn" (click)="increaseQty()">+</button>
                </div>
                <span class="text-muted small ms-2">Max {{ product.Stock }} units</span>
              </div>

              <!-- Description -->
              <div class="detail-desc-block mb-4">
                <h6 class="detail-section-head">About this item</h6>
                <p class="detail-desc">{{ product.Description }}</p>
              </div>

              <!-- Features list -->
              <div class="detail-features mb-4">
                <h6 class="detail-section-head">Key Features</h6>
                <ul class="feature-list">
                  <li><i class="bi bi-check2 text-success"></i> High-quality product</li>
                  <li><i class="bi bi-check2 text-success"></i> 1 Year Manufacturer Warranty</li>
                  <li><i class="bi bi-check2 text-success"></i> Free delivery on this item</li>
                  <li><i class="bi bi-check2 text-success"></i> Easy 30-day returns</li>
                  <li><i class="bi bi-check2 text-success"></i> Cash on Delivery available</li>
                </ul>
              </div>

              <!-- Trust badges -->
              <div class="trust-badges">
                <div class="trust-badge">
                  <i class="bi bi-shield-check text-success"></i>
                  <span>Secure Payment</span>
                </div>
                <div class="trust-badge">
                  <i class="bi bi-truck text-primary"></i>
                  <span>Free Delivery</span>
                </div>
                <div class="trust-badge">
                  <i class="bi bi-arrow-return-left text-warning"></i>
                  <span>Easy Returns</span>
                </div>
                <div class="trust-badge">
                  <i class="bi bi-star-fill text-warning"></i>
                  <span>Top Rated</span>
                </div>
              </div>

            </div>
          </div>
        </div>

        <!-- Back link -->
        <div class="mt-3" *ngIf="!loading">
          <a routerLink="/products" class="btn-shopez btn-outline-shopez">
            <i class="bi bi-arrow-left"></i> Back to Products
          </a>
        </div>

      </div>

      <!-- Footer -->
      <footer class="shopez-footer">
        <div class="footer-bottom" style="margin-top:0;padding-top:20px;">
          <p>© 2026 ShopEZ Technologies Pvt. Ltd.</p>
        </div>
      </footer>
    </div>

    <!-- Toast -->
    <div class="toast-notification" *ngIf="toastMsg">
      <i class="bi bi-check-circle-fill me-2"></i>{{ toastMsg }}
    </div>
  `,
  styles: [`
    .shopez-breadcrumb {
      display:flex; align-items:center; gap:6px;
      font-size:13px; color:#757575;
    }
    .shopez-breadcrumb a { color:#2874f0; }
    .shopez-breadcrumb a:hover { text-decoration:underline; }

    .product-detail-card { overflow:visible; }
    .product-detail-layout { display:grid; grid-template-columns:400px 1fr; gap:0; }

    /* Left column */
    .detail-img-col { padding:24px; border-right:1px solid #f0f0f0; }
    .main-img-wrapper { position:relative; background:#fafafa; border-radius:8px; overflow:hidden; margin-bottom:12px; }
    .main-product-img { width:100%; height:340px; object-fit:contain; padding:12px; }
    .img-actions {
      position:absolute; top:12px; right:12px;
      display:flex; flex-direction:column; gap:8px;
    }
    .img-action-btn {
      width:36px; height:36px; border-radius:50%;
      background:#fff; border:1px solid #e0e0e0;
      display:flex; align-items:center; justify-content:center;
      font-size:16px; color:#424242;
      box-shadow:var(--shadow-sm);
      transition:all .2s;
    }
    .img-action-btn:hover { background:#f5f5f5; color:#2874f0; }
    .thumb-strip { display:flex; gap:8px; margin-bottom:16px; }
    .thumb-img { width:56px; height:56px; object-fit:cover; border:2px solid #e0e0e0; border-radius:6px; cursor:pointer; }
    .active-thumb { border-color:#2874f0; }
    .under-img-actions { margin-top:8px; }

    /* Right column */
    .detail-info-col { padding:24px; }
    .detail-title { font-size:22px; font-weight:700; color:#212121; line-height:1.3; margin-bottom:8px; }
    .detail-rating-row { display:flex; align-items:center; flex-wrap:wrap; gap:4px; margin-bottom:8px; }
    .stars-lg { color:#ff9f00; font-size:16px; letter-spacing:2px; }
    .review-link { color:#2874f0; font-size:13px; cursor:pointer; }
    .detail-divider { border-color:#f0f0f0; margin:14px 0; }

    .detail-price-block { margin-bottom:8px; }
    .detail-price { display:flex; align-items:baseline; gap:4px; }
    .detail-currency { font-size:18px; font-weight:600; color:#212121; }
    .detail-price-val { font-size:32px; font-weight:800; color:#212121; }
    .detail-price-meta { display:flex; align-items:center; gap:8px; margin-top:4px; }
    .price-original-lg { font-size:14px; color:#878787; }
    .detail-discount { background:#e8f5e9; color:#388e3c; padding:2px 8px; border-radius:3px; font-size:13px; font-weight:700; }
    .detail-offer-text { font-size:13px; color:#424242; margin-top:8px; }

    .detail-qty-row { display:flex; align-items:center; gap:12px; }
    .detail-label { font-size:14px; font-weight:600; color:#424242; }

    .detail-section-head { font-size:14px; font-weight:700; color:#212121; margin-bottom:8px; }
    .detail-desc { font-size:14px; color:#555; line-height:1.7; }
    .feature-list { list-style:none; padding:0; }
    .feature-list li { display:flex; align-items:center; gap:8px; font-size:13px; color:#424242; padding:4px 0; }

    /* Trust badges */
    .trust-badges { display:flex; gap:12px; flex-wrap:wrap; padding:16px; background:#f8f9fa; border-radius:8px; }
    .trust-badge { display:flex; align-items:center; gap:6px; font-size:12px; font-weight:600; color:#424242; }
    .trust-badge i { font-size:18px; }

    /* Toast */
    .toast-notification {
      position:fixed; bottom:24px; right:24px;
      background:#323232; color:#fff;
      padding:14px 20px; border-radius:6px;
      font-size:14px; z-index:9999;
      display:flex; align-items:center;
      box-shadow:0 4px 20px rgba(0,0,0,.3);
      animation:slideIn .3s ease;
    }
    @keyframes slideIn { from{transform:translateX(100%);opacity:0} to{transform:translateX(0);opacity:1} }

    @media(max-width:768px){
      .product-detail-layout { grid-template-columns:1fr; }
      .detail-img-col { border-right:none; border-bottom:1px solid #f0f0f0; }
      .main-product-img { height:240px; }
    }
  `]
})
export class ProductDetailComponent implements OnInit {
  product: Product | null = null;
  loading = true;
  errorMessage = '';
  quantity = 1;
  productImage = '';
  toastMsg = '';

  private imageMap: { [key: string]: string } = {
    mouse:'assets/images/mouse.jpg', keyboard:'assets/images/keyboard.jpg',
    hub:'assets/images/harddrive.jpg', usb:'assets/images/harddrive.jpg',
    hard:'assets/images/harddrive.jpg', drive:'assets/images/harddrive.jpg',
    laptop:'assets/images/laptop.jpg', monitor:'assets/images/monitor.jpg',
    headphone:'assets/images/headphones.jpg', speaker:'assets/images/speaker.jpg',
    mobile:'assets/images/mobile.jpg', phone:'assets/images/mobile.jpg',
    tablet:'assets/images/tablet.jpg', watch:'assets/images/watch.jpg',
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private cartService: CartService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.productService.getProductById(id).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          this.product = res.data;
          this.productImage = this.getImage(res.data);
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.status === 404 ? 'Product not found.' : 'Failed to load product.';
      }
    });
  }

  increaseQty(): void { if (this.product && this.quantity < this.product.Stock) this.quantity++; }
  decreaseQty(): void { if (this.quantity > 1) this.quantity--; }

  addToCart(): void {
    if (!this.product || this.product.Stock === 0) return;
    for (let i = 0; i < this.quantity; i++) {
      this.cartService.addToCart({ ProductId: this.product.ProductId, Name: this.product.Name, Price: this.product.Price, ImageUrl: this.productImage, Stock: this.product.Stock, Quantity: 1 });
    }
    this.toastMsg = `${this.product.Name} (x${this.quantity}) added to cart!`;
    setTimeout(() => this.toastMsg = '', 2500);
  }

  buyNow(): void { this.addToCart(); this.router.navigate(['/cart']); }

  getImage(product: Product): string {
    const name = product.Name.toLowerCase();
    for (const key of Object.keys(this.imageMap)) {
      if (name.includes(key)) return this.imageMap[key];
    }
    return product.ImageUrl || 'assets/images/mouse.jpg';
  }
}