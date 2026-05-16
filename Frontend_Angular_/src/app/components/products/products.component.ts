import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { Product } from '../../models/product.model';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="page-wrapper">
      <div class="container-shopez py-4">

        <!-- Breadcrumb -->
        <nav class="shopez-breadcrumb mb-3">
          <a routerLink="/">Home</a>
          <i class="bi bi-chevron-right"></i>
          <span>All Products</span>
          <span class="ms-2 text-muted small" *ngIf="searchQuery">&nbsp;· Results for "{{ searchQuery }}"</span>
        </nav>

        <div class="catalog-layout">

          <!-- ── Sidebar Filters ──────────────────────────── -->
          <aside class="filter-sidebar">
            <div class="shopez-card mb-3">
              <div class="shopez-card-header">
                <i class="bi bi-funnel me-2"></i>Filters
              </div>
              <div class="shopez-card-body p-0">

                <!-- Sort -->
                <div class="filter-section">
                  <h6 class="filter-title">Sort By</h6>
                  <div class="filter-options">
                    <label class="filter-radio" *ngFor="let opt of sortOptions">
                      <input type="radio" name="sort" [value]="opt.value" [(ngModel)]="sortBy" (change)="applyFilters()" />
                      <span>{{ opt.label }}</span>
                    </label>
                  </div>
                </div>

                <!-- Availability -->
                <div class="filter-section">
                  <h6 class="filter-title">Availability</h6>
                  <div class="filter-options">
                    <label class="filter-check">
                      <input type="checkbox" [(ngModel)]="inStockOnly" (change)="applyFilters()" />
                      <span>In Stock Only</span>
                    </label>
                  </div>
                </div>

                <!-- Price Range -->
                <div class="filter-section">
                  <h6 class="filter-title">Price Range</h6>
                  <div class="filter-options">
                    <label class="filter-radio" *ngFor="let r of priceRanges">
                      <input type="radio" name="price" [value]="r.value" [(ngModel)]="priceRange" (change)="applyFilters()" />
                      <span>{{ r.label }}</span>
                    </label>
                  </div>
                </div>

              </div>
            </div>
          </aside>

          <!-- ── Product Area ────────────────────────────── -->
          <main class="product-area">

            <!-- Toolbar -->
            <div class="catalog-toolbar mb-3">
              <span class="results-count" *ngIf="!loading">
                <strong>{{ filteredProducts.length }}</strong> Products
                <span *ngIf="searchQuery"> for "{{ searchQuery }}"</span>
              </span>
              <div class="view-toggle">
                <button [class.active-view]="viewMode==='grid'" (click)="viewMode='grid'">
                  <i class="bi bi-grid"></i>
                </button>
                <button [class.active-view]="viewMode==='list'" (click)="viewMode='list'">
                  <i class="bi bi-list-ul"></i>
                </button>
              </div>
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

            <!-- ── Grid View ───────────────────────────────── -->
            <div class="products-grid" *ngIf="!loading && viewMode==='grid'">
              <div class="product-card" *ngFor="let product of filteredProducts">

                <a [routerLink]="['/products', product.ProductId]">
                  <div class="product-img-wrapper">
                    <img [src]="getProductImage(product)" [alt]="product.Name" class="product-card-img" />
                    <div class="product-img-overlay">
                      <i class="bi bi-eye-fill"></i> Quick View
                    </div>
                    <span class="product-badge-sale" *ngIf="product.ProductId % 2 === 0">SALE</span>
                  </div>
                </a>

                <div class="product-card-body">
                  <h3 class="product-card-name">
                    <a [routerLink]="['/products', product.ProductId]" style="color:inherit;">{{ product.Name }}</a>
                  </h3>
                  <p class="product-card-desc">{{ product.Description }}</p>

                  <div class="product-card-rating">
                    <span class="stars">★★★★★</span>
                    <span class="rating-count">({{ 100 + product.ProductId * 41 }})</span>
                  </div>

                  <div class="price-row">
                    <span class="product-card-price"><span class="currency">₹</span>{{ product.Price | number:'1.0-0' }}</span>
                    <span class="price-original">₹{{ (product.Price * 1.25) | number:'1.0-0' }}</span>
                    <span class="price-discount">20% off</span>
                  </div>

                  <div class="mt-1 mb-2">
                    <span class="badge-stock-in"  *ngIf="product.Stock > 0">✓ In Stock</span>
                    <span class="badge-stock-out" *ngIf="product.Stock === 0">Out of Stock</span>
                  </div>

                  <div class="product-card-actions">
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
                       class="btn-shopez btn-primary-shopez">
                      <i class="bi bi-eye"></i>
                    </a>
                  </div>
                </div>

              </div>
            </div>

            <!-- ── List View ───────────────────────────────── -->
            <div class="product-list-view" *ngIf="!loading && viewMode==='list'">
              <div class="product-list-item shopez-card mb-3" *ngFor="let product of filteredProducts">
                <a [routerLink]="['/products', product.ProductId]" class="list-img-link">
                  <img [src]="getProductImage(product)" [alt]="product.Name" class="list-img" />
                </a>
                <div class="list-body">
                  <h3 class="list-name">
                    <a [routerLink]="['/products', product.ProductId]">{{ product.Name }}</a>
                  </h3>
                  <div class="product-card-rating mb-2">
                    <span class="stars">★★★★★</span>
                    <span class="rating-count">({{ 100 + product.ProductId * 41 }})</span>
                  </div>
                  <p class="list-desc">{{ product.Description }}</p>
                  <div class="mt-1">
                    <span class="badge-stock-in"  *ngIf="product.Stock > 0">✓ In Stock ({{ product.Stock }} left)</span>
                    <span class="badge-stock-out" *ngIf="product.Stock === 0">Out of Stock</span>
                  </div>
                </div>
                <div class="list-price-col">
                  <div class="product-card-price"><span class="currency">₹</span>{{ product.Price | number:'1.0-0' }}</div>
                  <div class="price-original mb-1">₹{{ (product.Price * 1.25) | number:'1.0-0' }}</div>
                  <div class="price-discount mb-3">20% off</div>
                  <button
                    class="btn-shopez btn-block mb-2"
                    [class.btn-secondary-shopez]="product.Stock > 0"
                    [class.btn-disabled-shopez]="product.Stock === 0"
                    (click)="addToCart(product)"
                    [disabled]="product.Stock === 0"
                  >
                    <i class="bi bi-cart-plus"></i>
                    {{ product.Stock === 0 ? 'Out of Stock' : 'Add to Cart' }}
                  </button>
                  <a [routerLink]="['/products', product.ProductId]"
                     class="btn-shopez btn-primary-shopez btn-block">
                    <i class="bi bi-eye"></i> View Details
                  </a>
                </div>
              </div>
            </div>

            <!-- No results -->
            <div class="no-results" *ngIf="!loading && filteredProducts.length === 0 && !errorMessage">
              <i class="bi bi-search" style="font-size:48px;color:#ccc;"></i>
              <h4>No products found</h4>
              <p class="text-muted">Try adjusting your search or filters</p>
              <button class="btn-shopez btn-primary-shopez mt-3" (click)="clearFilters()">
                Clear Filters
              </button>
            </div>

          </main>
        </div>
      </div>

      <!-- Footer -->
      <footer class="shopez-footer">
        <div class="footer-bottom" style="margin-top:0;padding-top:20px;">
          <p>© 2026 ShopEZ Technologies Pvt. Ltd. All rights reserved.</p>
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

    /* Layout */
    .catalog-layout { display:grid; grid-template-columns:220px 1fr; gap:16px; align-items:start; }

    /* Sidebar */
    .filter-sidebar { position:sticky; top:80px; }
    .filter-section { padding:14px 16px; border-bottom:1px solid #f0f0f0; }
    .filter-section:last-child { border-bottom:none; }
    .filter-title { font-size:13px; font-weight:700; color:#212121; margin-bottom:10px; text-transform:uppercase; letter-spacing:.5px; }
    .filter-options { display:flex; flex-direction:column; gap:8px; }
    .filter-radio, .filter-check {
      display:flex; align-items:center; gap:8px;
      font-size:13px; color:#424242; cursor:pointer;
    }
    .filter-radio input, .filter-check input { accent-color:#2874f0; width:14px; height:14px; }

    /* Toolbar */
    .catalog-toolbar {
      display:flex; align-items:center; justify-content:space-between;
      background:#fff; border:1px solid #e0e0e0; border-radius:4px;
      padding:10px 16px;
    }
    .results-count { font-size:14px; color:#424242; }
    .view-toggle { display:flex; gap:4px; }
    .view-toggle button {
      width:34px; height:34px; border:1px solid #e0e0e0;
      background:#fff; border-radius:4px; font-size:16px;
      display:flex; align-items:center; justify-content:center;
      color:#757575; transition:all .2s;
    }
    .view-toggle button:hover, .active-view { background:#2874f0!important; color:#fff!important; border-color:#2874f0!important; }

    /* Product image overlay */
    .product-img-overlay {
      position:absolute; inset:0;
      background:rgba(0,0,0,.4); color:#fff;
      display:flex; align-items:center; justify-content:center;
      font-size:13px; font-weight:600; gap:6px;
      opacity:0; transition:opacity .2s;
    }
    .product-img-wrapper:hover .product-img-overlay { opacity:1; }
    .product-badge-sale {
      position:absolute; top:8px; right:8px;
      background:#e53935; color:#fff;
      font-size:10px; font-weight:800; padding:3px 7px; border-radius:3px;
    }

    /* Price row */
    .price-row { display:flex; align-items:baseline; gap:8px; flex-wrap:wrap; }
    .price-original { font-size:12px; color:#9e9e9e; text-decoration:line-through; }
    .price-discount  { font-size:12px; color:#388e3c; font-weight:600; }

    /* List view */
    .product-list-view {}
    .product-list-item { display:flex; gap:0; overflow:hidden; }
    .list-img-link { flex-shrink:0; }
    .list-img { width:180px; height:160px; object-fit:cover; }
    .list-body { flex:1; padding:16px; }
    .list-name { font-size:16px; font-weight:600; margin-bottom:6px; }
    .list-name a { color:#212121; }
    .list-name a:hover { color:#2874f0; }
    .list-desc { font-size:13px; color:#757575; line-height:1.6; }
    .list-price-col { flex-shrink:0; width:180px; padding:16px; border-left:1px solid #f0f0f0; display:flex; flex-direction:column; align-items:flex-start; }

    /* No results */
    .no-results { text-align:center; padding:60px 20px; }
    .no-results h4 { margin:16px 0 8px; font-size:18px; }

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
      .catalog-layout { grid-template-columns:1fr; }
      .filter-sidebar { position:static; }
      .list-img { width:120px; height:120px; }
      .list-price-col { width:140px; }
    }
  `]
})
export class ProductsComponent implements OnInit {
  products: Product[] = [];
  filteredProducts: Product[] = [];
  loading = true;
  errorMessage = '';
  toastMsg = '';
  viewMode = 'grid';
  searchQuery = '';
  sortBy = 'default';
  inStockOnly = false;
  priceRange = 'all';

  sortOptions = [
    { label: 'Relevance', value: 'default' },
    { label: 'Price: Low to High', value: 'price_asc' },
    { label: 'Price: High to Low', value: 'price_desc' },
    { label: 'Newest First', value: 'newest' },
  ];

  priceRanges = [
    { label: 'All Prices', value: 'all' },
    { label: 'Under ₹500', value: '0-500' },
    { label: '₹500 – ₹2,000', value: '500-2000' },
    { label: '₹2,000 – ₹10,000', value: '2000-10000' },
    { label: 'Above ₹10,000', value: '10000-999999' },
  ];

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
    private productService: ProductService,
    private cartService: CartService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.productService.getAllProducts().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          this.products = res.data;
          this.route.queryParams.subscribe(params => {
            this.searchQuery = params['q'] ?? '';
            this.applyFilters();
          });
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.status === 0 ? 'Cannot connect to API. Make sure your backend is running.' : 'Failed to load products.';
      }
    });
  }

  applyFilters(): void {
    let result = [...this.products];

    // Search
    if (this.searchQuery) {
      const q = this.searchQuery.toLowerCase();
      result = result.filter(p => p.Name.toLowerCase().includes(q) || p.Description.toLowerCase().includes(q));
    }

    // Stock
    if (this.inStockOnly) result = result.filter(p => p.Stock > 0);

    // Price range
    if (this.priceRange !== 'all') {
      const [min, max] = this.priceRange.split('-').map(Number);
      result = result.filter(p => p.Price >= min && p.Price <= max);
    }

    // Sort
    if (this.sortBy === 'price_asc')  result.sort((a, b) => a.Price - b.Price);
    if (this.sortBy === 'price_desc') result.sort((a, b) => b.Price - a.Price);
    if (this.sortBy === 'newest')     result.sort((a, b) => b.ProductId - a.ProductId);

    this.filteredProducts = result;
  }

  clearFilters(): void {
    this.sortBy = 'default';
    this.inStockOnly = false;
    this.priceRange = 'all';
    this.searchQuery = '';
    this.applyFilters();
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
    this.toastMsg = `${product.Name} added to cart!`;
    setTimeout(() => this.toastMsg = '', 2500);
  }
}

// import { Component, OnInit, OnDestroy } from '@angular/core';
// import { CommonModule } from '@angular/common';
// import { RouterLink, ActivatedRoute, Router } from '@angular/router';
// import { FormsModule } from '@angular/forms';
// import { ProductService } from '../../services/product.service';
// import { CartService } from '../../services/cart.service';
// import { Product } from '../../models/product.model';
// import { Subscription } from 'rxjs';

// @Component({
//   selector: 'app-products',
//   standalone: true,
//   imports: [CommonModule, RouterLink, FormsModule],
//   template: `
//     <div class="page-wrapper">
//       <div class="container-shopez py-4">

//         <!-- Breadcrumb -->
//         <nav class="shopez-breadcrumb mb-3">
//           <a routerLink="/">Home</a>
//           <i class="bi bi-chevron-right"></i>
//           <span>All Products</span>
//           <span class="ms-2 text-muted small" *ngIf="searchQuery">&nbsp;· Results for "{{ searchQuery }}"</span>
//         </nav>

//         <div class="catalog-layout">

//           <!-- ── Sidebar Filters ──────────────────────────── -->
//           <aside class="filter-sidebar">
//             <div class="shopez-card mb-3">
//               <div class="shopez-card-header">
//                 <i class="bi bi-funnel me-2"></i>Filters
//               </div>
//               <div class="shopez-card-body p-0">

//                 <!-- Sort -->
//                 <div class="filter-section">
//                   <h6 class="filter-title">Sort By</h6>
//                   <div class="filter-options">
//                     <label class="filter-radio" *ngFor="let opt of sortOptions">
//                       <input type="radio" name="sort" [value]="opt.value" [(ngModel)]="sortBy" (change)="applyFilters()" />
//                       <span>{{ opt.label }}</span>
//                     </label>
//                   </div>
//                 </div>

//                 <!-- Availability -->
//                 <div class="filter-section">
//                   <h6 class="filter-title">Availability</h6>
//                   <div class="filter-options">
//                     <label class="filter-check">
//                       <input type="checkbox" [(ngModel)]="inStockOnly" (change)="applyFilters()" />
//                       <span>In Stock Only</span>
//                     </label>
//                   </div>
//                 </div>

//                 <!-- Price Range -->
//                 <div class="filter-section">
//                   <h6 class="filter-title">Price Range</h6>
//                   <div class="filter-options">
//                     <label class="filter-radio" *ngFor="let r of priceRanges">
//                       <input type="radio" name="price" [value]="r.value" [(ngModel)]="priceRange" (change)="applyFilters()" />
//                       <span>{{ r.label }}</span>
//                     </label>
//                   </div>
//                 </div>

//               </div>
//             </div>
//           </aside>

//           <!-- ── Product Area ────────────────────────────── -->
//           <main class="product-area">

//             <!-- Toolbar -->
//             <div class="catalog-toolbar mb-3">
//               <span class="results-count" *ngIf="!loading">
//                 <strong>{{ filteredProducts.length }}</strong> Products
//                 <span *ngIf="searchQuery"> for "{{ searchQuery }}"</span>
//               </span>
//               <div class="view-toggle">
//                 <button [class.active-view]="viewMode==='grid'" (click)="viewMode='grid'">
//                   <i class="bi bi-grid"></i>
//                 </button>
//                 <button [class.active-view]="viewMode==='list'" (click)="viewMode='list'">
//                   <i class="bi bi-list-ul"></i>
//                 </button>
//               </div>
//             </div>

//             <!-- Loading -->
//             <div class="shopez-spinner" *ngIf="loading">
//               <div class="spinner"></div>
//               <span style="color:#757575;font-size:13px;">Loading products...</span>
//             </div>

//             <!-- Error -->
//             <div class="alert-shopez alert-error" *ngIf="errorMessage">
//               <i class="bi bi-exclamation-triangle me-2"></i>{{ errorMessage }}
//             </div>

//             <!-- ── Grid View ───────────────────────────────── -->
//             <div class="products-grid" *ngIf="!loading && viewMode==='grid'">
//               <div class="product-card" *ngFor="let product of filteredProducts">

//                 <a [routerLink]="['/products', product.ProductId]">
//                   <div class="product-img-wrapper">
//                     <img [src]="getProductImage(product)" [alt]="product.Name" class="product-card-img" />
//                     <div class="product-img-overlay">
//                       <i class="bi bi-eye-fill"></i> Quick View
//                     </div>
//                     <span class="product-badge-sale" *ngIf="product.ProductId % 2 === 0">SALE</span>
//                   </div>
//                 </a>

//                 <div class="product-card-body">
//                   <h3 class="product-card-name">
//                     <a [routerLink]="['/products', product.ProductId]" style="color:inherit;">{{ product.Name }}</a>
//                   </h3>
//                   <p class="product-card-desc">{{ product.Description }}</p>

//                   <div class="product-card-rating">
//                     <span class="stars">★★★★★</span>
//                     <span class="rating-count">({{ 100 + product.ProductId * 41 }})</span>
//                   </div>

//                   <div class="price-row">
//                     <span class="product-card-price"><span class="currency">₹</span>{{ product.Price | number:'1.0-0' }}</span>
//                     <span class="price-original">₹{{ (product.Price * 1.25) | number:'1.0-0' }}</span>
//                     <span class="price-discount">20% off</span>
//                   </div>

//                   <div class="mt-1 mb-2">
//                     <span class="badge-stock-in"  *ngIf="product.Stock > 0">✓ In Stock</span>
//                     <span class="badge-stock-out" *ngIf="product.Stock === 0">Out of Stock</span>
//                   </div>

//                   <div class="product-card-actions">
//                     <button
//                       class="btn-shopez flex-fill"
//                       [class.btn-secondary-shopez]="product.Stock > 0"
//                       [class.btn-disabled-shopez]="product.Stock === 0"
//                       (click)="addToCart(product)"
//                       [disabled]="product.Stock === 0"
//                     >
//                       <i class="bi bi-cart-plus"></i>
//                       {{ product.Stock === 0 ? 'Out of Stock' : 'Add to Cart' }}
//                     </button>
//                     <a [routerLink]="['/products', product.ProductId]"
//                        class="btn-shopez btn-primary-shopez">
//                       <i class="bi bi-eye"></i>
//                     </a>
//                   </div>
//                 </div>

//               </div>
//             </div>

//             <!-- ── List View ───────────────────────────────── -->
//             <div class="product-list-view" *ngIf="!loading && viewMode==='list'">
//               <div class="product-list-item shopez-card mb-3" *ngFor="let product of filteredProducts">
//                 <a [routerLink]="['/products', product.ProductId]" class="list-img-link">
//                   <img [src]="getProductImage(product)" [alt]="product.Name" class="list-img" />
//                 </a>
//                 <div class="list-body">
//                   <h3 class="list-name">
//                     <a [routerLink]="['/products', product.ProductId]">{{ product.Name }}</a>
//                   </h3>
//                   <div class="product-card-rating mb-2">
//                     <span class="stars">★★★★★</span>
//                     <span class="rating-count">({{ 100 + product.ProductId * 41 }})</span>
//                   </div>
//                   <p class="list-desc">{{ product.Description }}</p>
//                   <div class="mt-1">
//                     <span class="badge-stock-in"  *ngIf="product.Stock > 0">✓ In Stock ({{ product.Stock }} left)</span>
//                     <span class="badge-stock-out" *ngIf="product.Stock === 0">Out of Stock</span>
//                   </div>
//                 </div>
//                 <div class="list-price-col">
//                   <div class="product-card-price"><span class="currency">₹</span>{{ product.Price | number:'1.0-0' }}</div>
//                   <div class="price-original mb-1">₹{{ (product.Price * 1.25) | number:'1.0-0' }}</div>
//                   <div class="price-discount mb-3">20% off</div>
//                   <button
//                     class="btn-shopez btn-block mb-2"
//                     [class.btn-secondary-shopez]="product.Stock > 0"
//                     [class.btn-disabled-shopez]="product.Stock === 0"
//                     (click)="addToCart(product)"
//                     [disabled]="product.Stock === 0"
//                   >
//                     <i class="bi bi-cart-plus"></i>
//                     {{ product.Stock === 0 ? 'Out of Stock' : 'Add to Cart' }}
//                   </button>
//                   <a [routerLink]="['/products', product.ProductId]"
//                      class="btn-shopez btn-primary-shopez btn-block">
//                     <i class="bi bi-eye"></i> View Details
//                   </a>
//                 </div>
//               </div>
//             </div>

//             <!-- No results -->
//             <div class="no-results" *ngIf="!loading && filteredProducts.length === 0 && !errorMessage">
//               <i class="bi bi-search" style="font-size:48px;color:#ccc;"></i>
//               <h4>No products found</h4>
//               <p class="text-muted">Try adjusting your search or filters</p>
//               <button class="btn-shopez btn-primary-shopez mt-3" (click)="clearFilters()">
//                 Clear Filters
//               </button>
//             </div>

//           </main>
//         </div>
//       </div>

//       <!-- Footer -->
//       <footer class="shopez-footer">
//         <div class="footer-bottom" style="margin-top:0;padding-top:20px;">
//           <p>© 2026 ShopEZ Technologies Pvt. Ltd. All rights reserved.</p>
//         </div>
//       </footer>
//     </div>

//     <!-- Toast -->
//     <div class="toast-notification" *ngIf="toastMsg">
//       <i class="bi bi-check-circle-fill me-2"></i>{{ toastMsg }}
//     </div>
//   `,
//   styles: [`
//     .shopez-breadcrumb {
//       display:flex; align-items:center; gap:6px;
//       font-size:13px; color:#757575;
//     }
//     .shopez-breadcrumb a { color:#2874f0; }
//     .shopez-breadcrumb a:hover { text-decoration:underline; }

//     /* Layout */
//     .catalog-layout { display:grid; grid-template-columns:220px 1fr; gap:16px; align-items:start; }

//     /* Sidebar */
//     .filter-sidebar { position:sticky; top:80px; }
//     .filter-section { padding:14px 16px; border-bottom:1px solid #f0f0f0; }
//     .filter-section:last-child { border-bottom:none; }
//     .filter-title { font-size:13px; font-weight:700; color:#212121; margin-bottom:10px; text-transform:uppercase; letter-spacing:.5px; }
//     .filter-options { display:flex; flex-direction:column; gap:8px; }
//     .filter-radio, .filter-check {
//       display:flex; align-items:center; gap:8px;
//       font-size:13px; color:#424242; cursor:pointer;
//     }
//     .filter-radio input, .filter-check input { accent-color:#2874f0; width:14px; height:14px; }

//     /* Toolbar */
//     .catalog-toolbar {
//       display:flex; align-items:center; justify-content:space-between;
//       background:#fff; border:1px solid #e0e0e0; border-radius:4px;
//       padding:10px 16px;
//     }
//     .results-count { font-size:14px; color:#424242; }
//     .view-toggle { display:flex; gap:4px; }
//     .view-toggle button {
//       width:34px; height:34px; border:1px solid #e0e0e0;
//       background:#fff; border-radius:4px; font-size:16px;
//       display:flex; align-items:center; justify-content:center;
//       color:#757575; transition:all .2s;
//     }
//     .view-toggle button:hover, .active-view { background:#2874f0!important; color:#fff!important; border-color:#2874f0!important; }

//     /* Product image overlay */
//     .product-img-overlay {
//       position:absolute; inset:0;
//       background:rgba(0,0,0,.4); color:#fff;
//       display:flex; align-items:center; justify-content:center;
//       font-size:13px; font-weight:600; gap:6px;
//       opacity:0; transition:opacity .2s;
//     }
//     .product-img-wrapper:hover .product-img-overlay { opacity:1; }
//     .product-badge-sale {
//       position:absolute; top:8px; right:8px;
//       background:#e53935; color:#fff;
//       font-size:10px; font-weight:800; padding:3px 7px; border-radius:3px;
//     }

//     /* Price row */
//     .price-row { display:flex; align-items:baseline; gap:8px; flex-wrap:wrap; }
//     .price-original { font-size:12px; color:#9e9e9e; text-decoration:line-through; }
//     .price-discount  { font-size:12px; color:#388e3c; font-weight:600; }

//     /* List view */
//     .product-list-view {}
//     .product-list-item { display:flex; gap:0; overflow:hidden; }
//     .list-img-link { flex-shrink:0; }
//     .list-img { width:180px; height:160px; object-fit:cover; }
//     .list-body { flex:1; padding:16px; }
//     .list-name { font-size:16px; font-weight:600; margin-bottom:6px; }
//     .list-name a { color:#212121; }
//     .list-name a:hover { color:#2874f0; }
//     .list-desc { font-size:13px; color:#757575; line-height:1.6; }
//     .list-price-col { flex-shrink:0; width:180px; padding:16px; border-left:1px solid #f0f0f0; display:flex; flex-direction:column; align-items:flex-start; }

//     /* No results */
//     .no-results { text-align:center; padding:60px 20px; }
//     .no-results h4 { margin:16px 0 8px; font-size:18px; }

//     /* Toast */
//     .toast-notification {
//       position:fixed; bottom:24px; right:24px;
//       background:#323232; color:#fff;
//       padding:14px 20px; border-radius:6px;
//       font-size:14px; z-index:9999;
//       display:flex; align-items:center;
//       box-shadow:0 4px 20px rgba(0,0,0,.3);
//       animation:slideIn .3s ease;
//     }
//     @keyframes slideIn { from{transform:translateX(100%);opacity:0} to{transform:translateX(0);opacity:1} }

//     @media(max-width:768px){
//       .catalog-layout { grid-template-columns:1fr; }
//       .filter-sidebar { position:static; }
//       .list-img { width:120px; height:120px; }
//       .list-price-col { width:140px; }
//     }
//   `]
// })
// export class ProductsComponent implements OnInit, OnDestroy {
//   products: Product[] = [];
//   filteredProducts: Product[] = [];
//   loading = true;
//   errorMessage = '';
//   toastMsg = '';
//   viewMode = 'grid';
//   searchQuery = '';
//   sortBy = 'default';
//   inStockOnly = false;
//   priceRange = 'all';
//   private querySub!: Subscription;

//   sortOptions = [
//     { label: 'Relevance', value: 'default' },
//     { label: 'Price: Low to High', value: 'price_asc' },
//     { label: 'Price: High to Low', value: 'price_desc' },
//     { label: 'Newest First', value: 'newest' },
//   ];

//   priceRanges = [
//     { label: 'All Prices', value: 'all' },
//     { label: 'Under ₹500', value: '0-500' },
//     { label: '₹500 – ₹2,000', value: '500-2000' },
//     { label: '₹2,000 – ₹10,000', value: '2000-10000' },
//     { label: 'Above ₹10,000', value: '10000-999999' },
//   ];

//   private imageMap: { [key: string]: string } = {
//     mouse:'assets/images/mouse.jpg', keyboard:'assets/images/keyboard.jpg',
//     hub:'assets/images/harddrive.jpg', usb:'assets/images/harddrive.jpg',
//     hard:'assets/images/harddrive.jpg', drive:'assets/images/harddrive.jpg',
//     laptop:'assets/images/laptop.jpg', monitor:'assets/images/monitor.jpg',
//     headphone:'assets/images/headphones.jpg', speaker:'assets/images/speaker.jpg',
//     mobile:'assets/images/mobile.jpg', phone:'assets/images/mobile.jpg',
//     tablet:'assets/images/tablet.jpg', watch:'assets/images/watch.jpg',
//   };

//   constructor(
//     private productService: ProductService,
//     private cartService: CartService,
//     private route: ActivatedRoute,
//     private router: Router
//   ) {}

//   ngOnInit(): void {
//     // Subscribe to query params FIRST — independently of product loading
//     this.querySub = this.route.queryParams.subscribe(params => {
//       this.searchQuery = params['q'] ?? '';
//       this.applyFilters();
//     });

//     // Load products separately
//     this.productService.getAllProducts().subscribe({
//       next: (res) => {
//         this.loading = false;
//         if (res.success && res.data) {
//           this.products = res.data;
//           this.applyFilters();
//         }
//       },
//       error: (err) => {
//         this.loading = false;
//         this.errorMessage = err.status === 0
//           ? 'Cannot connect to API. Make sure your backend is running.'
//           : 'Failed to load products.';
//       }
//     });
//   }

//   ngOnDestroy(): void {
//     this.querySub?.unsubscribe();
//   }

//   applyFilters(): void {
//     let result = [...this.products];

//     // Search
//     if (this.searchQuery) {
//       const q = this.searchQuery.toLowerCase();
//       result = result.filter(p => p.Name.toLowerCase().includes(q) || p.Description.toLowerCase().includes(q));
//     }

//     // Stock
//     if (this.inStockOnly) result = result.filter(p => p.Stock > 0);

//     // Price range
//     if (this.priceRange !== 'all') {
//       const [min, max] = this.priceRange.split('-').map(Number);
//       result = result.filter(p => p.Price >= min && p.Price <= max);
//     }

//     // Sort
//     if (this.sortBy === 'price_asc')  result.sort((a, b) => a.Price - b.Price);
//     if (this.sortBy === 'price_desc') result.sort((a, b) => b.Price - a.Price);
//     if (this.sortBy === 'newest')     result.sort((a, b) => b.ProductId - a.ProductId);

//     this.filteredProducts = result;
//   }

//   clearFilters(): void {
//     this.sortBy = 'default';
//     this.inStockOnly = false;
//     this.priceRange = 'all';
//     this.searchQuery = '';
//     this.applyFilters();
//   }

//   getProductImage(product: Product): string {
//     const name = product.Name.toLowerCase();
//     for (const key of Object.keys(this.imageMap)) {
//       if (name.includes(key)) return this.imageMap[key];
//     }
//     return product.ImageUrl || 'assets/images/mouse.jpg';
//   }

//   addToCart(product: Product): void {
//     if (product.Stock === 0) return;
//     this.cartService.addToCart({ ProductId: product.ProductId, Name: product.Name, Price: product.Price, ImageUrl: this.getProductImage(product), Stock: product.Stock, Quantity: 1 });
//     this.toastMsg = `${product.Name} added to cart!`;
//     setTimeout(() => this.toastMsg = '', 2500);
//   }
// }