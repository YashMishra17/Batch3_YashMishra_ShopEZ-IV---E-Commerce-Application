import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { Product } from '../../models/product.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page-wrapper">

      <!-- ── Hero Banner ───────────────────────────────────── -->
      <div class="hero-banner">
        <div class="hero-content container-shopez">
          <div class="hero-text">
            <p class="hero-tag"> Best Deals of the Season</p>
            <h1 class="hero-title">Upgrade Your<br><span class="hero-highlight">Digital Lifestyle</span></h1>
            <p class="hero-sub">
              Laptops · Mobiles · Accessories · Gadgets<br>
              Fast delivery · Secure payment · Easy returns
            </p>
            <div class="hero-btns">
              <a routerLink="/products" class="btn-shopez btn-secondary-shopez btn-lg-shopez">
                <i class="bi bi-bag-fill"></i> Shop Now
              </a>
              <a routerLink="/register" class="btn-shopez btn-outline-white btn-lg-shopez" *ngIf="!isLoggedIn">
                <i class="bi bi-person-plus"></i> Join Free
              </a>
            </div>
          </div>
          <div class="hero-image-col">
            <div class="hero-image-grid">
              <img src="assets/images/laptop.jpg"     alt="Laptop"   class="hero-img hero-img-main" />
              <img src="assets/images/mobile.jpg"     alt="Mobile"   class="hero-img hero-img-sm" />
              <img src="assets/images/headphones.jpg" alt="Audio"    class="hero-img hero-img-sm" />
            </div>
          </div>
        </div>
      </div>

      <!-- ── Category Strip ────────────────────────────────── -->
      <div class="category-strip">
        <div class="container-shopez">
          <div class="category-row">
            <a [routerLink]="['/products']" [queryParams]="{q:'laptop'}"   class="cat-chip"><i class="bi bi-laptop"></i>     Laptops</a>
            <a [routerLink]="['/products']" [queryParams]="{q:'mobile'}"   class="cat-chip"><i class="bi bi-phone"></i>      Mobiles</a>
            <a [routerLink]="['/products']" [queryParams]="{q:'keyboard'}" class="cat-chip"><i class="bi bi-keyboard"></i>   Keyboards</a>
            <a [routerLink]="['/products']" [queryParams]="{q:'mouse'}"    class="cat-chip"><i class="bi bi-mouse"></i>      Mouse</a>
            <a [routerLink]="['/products']" [queryParams]="{q:'monitor'}"  class="cat-chip"><i class="bi bi-display"></i>    Monitors</a>
            <a [routerLink]="['/products']" [queryParams]="{q:'headphone'}" class="cat-chip"><i class="bi bi-headphones"></i> Headphones</a>
            <a [routerLink]="['/products']" [queryParams]="{q:'tablet'}"   class="cat-chip"><i class="bi bi-tablet"></i>     Tablets</a>
            <a [routerLink]="['/products']" [queryParams]="{q:'watch'}"    class="cat-chip"><i class="bi bi-watch"></i>      Watches</a>
          </div>
        </div>
      </div>

      <!-- ── Feature Badges ────────────────────────────────── -->
      <div class="container-shopez my-3">
        <div class="feature-badges">
          <div class="feat-badge"><i class="bi bi-truck"></i><div><strong>Free Delivery</strong><span>On orders above ₹499</span></div></div>
          <div class="feat-badge"><i class="bi bi-shield-check"></i><div><strong>Secure Payment</strong><span>100% safe & encrypted</span></div></div>
          <div class="feat-badge"><i class="bi bi-arrow-return-left"></i><div><strong>Easy Returns</strong><span>30-day return policy</span></div></div>
          <div class="feat-badge"><i class="bi bi-headset"></i><div><strong>24/7 Support</strong><span>Always here to help</span></div></div>
        </div>
      </div>

      <!-- ── Featured Products ──────────────────────────────── -->
      <div class="container-shopez my-4">
        <div class="d-flex align-items-center justify-content-between mb-3">
          <h2 class="section-title mb-0"> Featured Products</h2>
          <a routerLink="/products" class="btn-shopez btn-outline-shopez">View All <i class="bi bi-arrow-right"></i></a>
        </div>

        <!-- Loading -->
        <div class="shopez-spinner" *ngIf="loading">
          <div class="spinner"></div>
          <span style="color:#757575;font-size:13px;">Loading products...</span>
        </div>

        <!-- Error -->
        <div class="alert-shopez alert-error" *ngIf="errorMessage">
          <i class="bi bi-exclamation-triangle me-2"></i>{{ errorMessage }}
        </div>

        <!-- Product Grid -->
        <div class="products-grid" *ngIf="!loading && !errorMessage">
          <div class="product-card" *ngFor="let product of featuredProducts">

            <a [routerLink]="['/products', product.ProductId]">
              <div class="product-img-wrapper">
                <img [src]="getProductImage(product)" [alt]="product.Name" class="product-card-img" />
                <span class="product-badge-new" *ngIf="product.ProductId <= 3">New</span>
              </div>
            </a>

            <div class="product-card-body">
              <h3 class="product-card-name">
                <a [routerLink]="['/products', product.ProductId]" style="color:inherit;">{{ product.Name }}</a>
              </h3>
              <p class="product-card-desc">{{ product.Description }}</p>

              <div class="product-card-rating">
                <span class="stars">★★★★★</span>
                <span class="rating-count">({{ 120 + product.ProductId * 37 }})</span>
              </div>

              <div class="product-card-price">
                <span class="currency">₹</span>{{ product.Price | number:'1.0-0' }}
              </div>
              <div class="price-original text-muted small" style="text-decoration:line-through;">
                ₹{{ (product.Price * 1.2) | number:'1.0-0' }}
                <span class="text-success ms-1">20% off</span>
              </div>

              <div class="mt-2">
                <span class="badge-stock-in"  *ngIf="product.Stock > 0">✓ In Stock</span>
                <span class="badge-stock-out" *ngIf="product.Stock === 0">Out of Stock</span>
              </div>

              <div class="product-card-actions mt-3">
                <button
                  class="btn-shopez flex-fill"
                  [class.btn-secondary-shopez]="product.Stock > 0"
                  [class.btn-disabled-shopez]="product.Stock === 0"
                  (click)="addToCart(product)"
                  [disabled]="product.Stock === 0"
                >
                  <i class="bi bi-cart-plus"></i>
                  {{ product.Stock === 0 ? 'Out of Stock' : 'Add to Cart' }}
                </button>
                <a [routerLink]="['/products', product.ProductId]"
                   class="btn-shopez btn-primary-shopez flex-fill">
                  <i class="bi bi-eye"></i> View
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Deal Banners ───────────────────────────────────── -->
      <div class="container-shopez my-4">
        <div class="deal-banners">
          <div class="deal-card deal-blue">
            <div>
              <h3>Up to 40% Off</h3>
              <p>On all Laptops & Desktops</p>
              <a routerLink="/products" [queryParams]="{q:'laptop'}" class="btn-shopez btn-primary-shopez mt-2">Shop Now</a>
            </div>
            <img src="assets/images/laptop.jpg" alt="Laptop Deal" />
          </div>
          <div class="deal-card deal-orange">
            <div>
              <h3>New Arrivals</h3>
              <p>Latest Mobiles & Gadgets</p>
              <a routerLink="/products" [queryParams]="{q:'mobile'}" class="btn-shopez btn-secondary-shopez mt-2">Explore</a>
            </div>
            <img src="assets/images/mobile.jpg" alt="Mobile Deal" />
          </div>
        </div>
      </div>

      <!-- ── All Products Section ───────────────────────────── -->
      <div class="container-shopez my-4">
        <div class="d-flex align-items-center justify-content-between mb-3">
          <h2 class="section-title mb-0"> All Products</h2>
        </div>

        <div class="products-grid" *ngIf="!loading">
          <div class="product-card" *ngFor="let product of allProducts">
            <a [routerLink]="['/products', product.ProductId]">
              <div class="product-img-wrapper">
                <img [src]="getProductImage(product)" [alt]="product.Name" class="product-card-img" />
              </div>
            </a>
            <div class="product-card-body">
              <h3 class="product-card-name">
                <a [routerLink]="['/products', product.ProductId]" style="color:inherit;">{{ product.Name }}</a>
              </h3>
              <div class="product-card-rating">
                <span class="stars">★★★★☆</span>
                <span class="rating-count">({{ 80 + product.ProductId * 23 }})</span>
              </div>
              <div class="product-card-price"><span class="currency">₹</span>{{ product.Price | number:'1.0-0' }}</div>
              <div class="mt-2">
                <span class="badge-stock-in"  *ngIf="product.Stock > 0">✓ In Stock</span>
                <span class="badge-stock-out" *ngIf="product.Stock === 0">Out of Stock</span>
              </div>
              <div class="product-card-actions mt-3">
                <button
                  class="btn-shopez flex-fill"
                  [class.btn-secondary-shopez]="product.Stock > 0"
                  [class.btn-disabled-shopez]="product.Stock === 0"
                  (click)="addToCart(product)"
                  [disabled]="product.Stock === 0"
                >
                  <i class="bi bi-cart-plus"></i>
                  {{ product.Stock === 0 ? 'Out of Stock' : 'Add to Cart' }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Footer ─────────────────────────────────────────── -->
      <footer class="shopez-footer">
        <div class="footer-grid">
          <div class="footer-section">
            <h4>About ShopEZ</h4>
            <ul>
              <li><a href="#">About Us</a></li>
              <li><a href="#">Careers</a></li>
              <li><a href="#">Press</a></li>
              <li><a href="#">Blog</a></li>
            </ul>
          </div>
          <div class="footer-section">
            <h4>Help</h4>
            <ul>
              <li><a href="#">Payments</a></li>
              <li><a href="#">Shipping</a></li>
              <li><a href="#">Returns</a></li>
              <li><a href="#">FAQ</a></li>
            </ul>
          </div>
          <div class="footer-section">
            <h4>Customer Service</h4>
            <ul>
              <li><a href="#">Contact Us</a></li>
              <li><a href="#">Track Order</a></li>
              <li><a href="#">Cancellation</a></li>
              <li><a href="#">Report Fraud</a></li>
            </ul>
          </div>
          <div class="footer-section">
            <h4>Connect With Us</h4>
            <ul>
              <li><a href="#"><i class="bi bi-facebook me-2"></i>Facebook</a></li>
              <li><a href="#"><i class="bi bi-twitter-x me-2"></i>Twitter</a></li>
              <li><a href="#"><i class="bi bi-instagram me-2"></i>Instagram</a></li>
              <li><a href="#"><i class="bi bi-youtube me-2"></i>YouTube</a></li>
            </ul>
          </div>
        </div>
        <div class="footer-bottom">
          <p>© 2026 ShopEZ Technologies Pvt. Ltd. All rights reserved.</p>
        </div>
      </footer>

    </div>

    <!-- Toast notification -->
    <div class="toast-notification" *ngIf="toastMsg" [class.show]="toastMsg">
      <i class="bi bi-check-circle-fill me-2"></i>{{ toastMsg }}
    </div>
  `,
  styles: [`
    /* Hero */
    .hero-banner {
      background: linear-gradient(135deg,#131921 0%,#1a2332 60%,#0d3b6e 100%);
      padding: 48px 0 32px; overflow: hidden;
    }
    .hero-content { display:flex; align-items:center; gap:40px; }
    .hero-text { flex:1; }
    .hero-tag { color:#ff9f00; font-size:13px; font-weight:700; letter-spacing:.5px; margin-bottom:10px; }
    .hero-title { font-size:40px; font-weight:800; color:#fff; line-height:1.2; margin-bottom:16px; }
    .hero-highlight { color:#ff9f00; }
    .hero-sub { color:#aab7c4; font-size:15px; line-height:1.7; margin-bottom:28px; }
    .hero-btns { display:flex; gap:12px; flex-wrap:wrap; }
    .btn-outline-white { background:transparent; border:2px solid #fff; color:#fff; }
    .btn-outline-white:hover { background:rgba(255,255,255,.1); }

    .hero-image-col { flex:1; display:flex; justify-content:center; }
    .hero-image-grid { display:grid; grid-template-columns:1fr 1fr; grid-template-rows:1fr 1fr; gap:10px; max-width:320px; }
    .hero-img { border-radius:10px; object-fit:cover; width:100%; box-shadow:0 4px 20px rgba(0,0,0,.4); }
    .hero-img-main { grid-row:1/3; height:200px; }
    .hero-img-sm { height:95px; }

    /* Category strip */
    .category-strip { background:#fff; border-bottom:1px solid #e0e0e0; padding:0; }
    .category-row { display:flex; gap:0; overflow-x:auto; }
    .category-row::-webkit-scrollbar { display:none; }
    .cat-chip {
      display:flex; align-items:center; gap:6px;
      padding:12px 18px; white-space:nowrap;
      font-size:13px; font-weight:600; color:#212121;
      border-bottom:3px solid transparent;
      transition:all .2s;
    }
    .cat-chip:hover { color:#2874f0; border-bottom-color:#2874f0; background:#f5f8ff; }

    /* Feature badges */
    .feature-badges {
      display:grid; grid-template-columns:repeat(4,1fr);
      gap:12px;
    }
    .feat-badge {
      background:#fff; border:1px solid #e0e0e0;
      border-radius:8px; padding:14px 16px;
      display:flex; align-items:center; gap:12px;
    }
    .feat-badge i { font-size:24px; color:#2874f0; }
    .feat-badge div { display:flex; flex-direction:column; }
    .feat-badge strong { font-size:13px; font-weight:700; color:#212121; }
    .feat-badge span   { font-size:11px; color:#757575; }

    /* Product img wrapper */
    .product-img-wrapper { position:relative; overflow:hidden; }
    .product-badge-new {
      position:absolute; top:8px; left:8px;
      background:#2874f0; color:#fff;
      font-size:11px; font-weight:700;
      padding:3px 8px; border-radius:3px;
    }

    /* Deal banners */
    .deal-banners { display:grid; grid-template-columns:1fr 1fr; gap:16px; }
    .deal-card {
      border-radius:12px; padding:28px;
      display:flex; align-items:center; justify-content:space-between;
      overflow:hidden; gap:16px;
    }
    .deal-blue   { background:linear-gradient(135deg,#2874f0,#0d47a1); color:#fff; }
    .deal-orange { background:linear-gradient(135deg,#ff7043,#e64a19); color:#fff; }
    .deal-card h3 { font-size:22px; font-weight:800; margin-bottom:6px; }
    .deal-card p  { font-size:14px; opacity:.85; margin-bottom:0; }
    .deal-card img { width:120px; height:100px; object-fit:cover; border-radius:8px; flex-shrink:0; }

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
      .hero-content { flex-direction:column; }
      .hero-image-col { display:none; }
      .hero-title { font-size:28px; }
      .feature-badges { grid-template-columns:1fr 1fr; }
      .deal-banners { grid-template-columns:1fr; }
    }
  `]
})
export class HomeComponent implements OnInit {
  featuredProducts: Product[] = [];
  allProducts: Product[] = [];
  loading = true;
  errorMessage = '';
  isLoggedIn = false;
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

  constructor(private productService: ProductService, private cartService: CartService) {}

  ngOnInit(): void {
    this.productService.getAllProducts().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          this.featuredProducts = res.data.slice(0, 4);
          this.allProducts = res.data;
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.status === 0 ? 'Cannot connect to API. Is the backend running?' : 'Failed to load products.';
      }
    });
  }

  getProductImage(product: Product): string {
    const name = product.Name.toLowerCase();
    for (const key of Object.keys(this.imageMap)) {
      if (name.includes(key)) return this.imageMap[key];
    }
    return product.ImageUrl || 'assets/images/mouse.jpg';
  }

  addToCart(product: Product): void {
    if (product.Stock === 0) return;
    this.cartService.addToCart({ ProductId: product.ProductId, Name: product.Name, Price: product.Price, ImageUrl: this.getProductImage(product), Stock: product.Stock, Quantity: 1 });
    this.showToast(`${product.Name} added to cart!`);
  }

  showToast(msg: string): void {
    this.toastMsg = msg;
    setTimeout(() => this.toastMsg = '', 2500);
  }
}