import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../services/product.service';
import { Product, CreateProductRequest } from '../../models/product.model';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-wrapper">
      <div class="container-shopez py-4">

        <!-- Header -->
        <div class="admin-header">
          <div>
            <h2 class="admin-title"><i class="bi bi-gear-fill me-2"></i>Admin Panel</h2>
            <p class="admin-sub">Manage your product catalog</p>
          </div>
          <div class="admin-stats">
            <div class="stat-card">
              <span class="stat-num">{{ products.length }}</span>
              <span class="stat-label">Total Products</span>
            </div>
            <div class="stat-card stat-green">
              <span class="stat-num">{{ inStockCount }}</span>
              <span class="stat-label">In Stock</span>
            </div>
            <div class="stat-card stat-red">
              <span class="stat-num">{{ outOfStockCount }}</span>
              <span class="stat-label">Out of Stock</span>
            </div>
          </div>
        </div>

        <!-- Alerts -->
        <div class="alert-shopez alert-error mb-3" *ngIf="formError">
          <i class="bi bi-exclamation-triangle me-2"></i>{{ formError }}
        </div>
        <div class="alert-shopez alert-success mb-3" *ngIf="formSuccess">
          <i class="bi bi-check-circle me-2"></i>{{ formSuccess }}
        </div>

        <div class="admin-layout">

          <!-- LEFT: Form -->
          <div class="admin-form-col">
            <div class="shopez-card">
              <div class="shopez-card-header" [style.background]="editingProduct ? '#ff8f00' : '#2874f0'" style="color:#fff;">
                <i [class]="editingProduct ? 'bi bi-pencil-square me-2' : 'bi bi-plus-circle me-2'"></i>
                {{ editingProduct ? 'Edit Product' : 'Add New Product' }}
              </div>
              <div class="shopez-card-body">

                <div class="shopez-form-group">
                  <label class="shopez-label">Product Name *</label>
                  <input type="text" class="shopez-input" [(ngModel)]="form.Name" placeholder="e.g. Wireless Mouse" />
                </div>

                <div class="shopez-form-group">
                  <label class="shopez-label">Description</label>
                  <textarea class="shopez-input" [(ngModel)]="form.Description" rows="3" placeholder="Product description..."></textarea>
                </div>

                <div class="row g-3">
                  <div class="col-6">
                    <div class="shopez-form-group">
                      <label class="shopez-label">Price (₹) *</label>
                      <input type="number" class="shopez-input" [(ngModel)]="form.Price" placeholder="0.00" step="0.01" min="0" />
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="shopez-form-group">
                      <label class="shopez-label">Stock *</label>
                      <input type="number" class="shopez-input" [(ngModel)]="form.Stock" placeholder="0" min="0" />
                    </div>
                  </div>
                </div>

                <div class="shopez-form-group">
                  <label class="shopez-label">Image URL</label>
                  <input type="text" class="shopez-input" [(ngModel)]="form.ImageUrl" placeholder="https://..." />
                </div>

                <!-- Image Preview -->
                <div class="img-preview-box" *ngIf="form.ImageUrl">
                  <img [src]="form.ImageUrl" alt="Preview" class="img-preview" />
                </div>

                <div class="form-action-row">
                  <button
                    class="btn-shopez flex-fill"
                    [class.btn-primary-shopez]="!editingProduct"
                    [class.btn-secondary-shopez]="!!editingProduct"
                    (click)="saveProduct()"
                    [disabled]="saving"
                  >
                    <span *ngIf="!saving">
                      <i [class]="editingProduct ? 'bi bi-check-circle me-1' : 'bi bi-plus-circle me-1'"></i>
                      {{ editingProduct ? 'Update Product' : 'Add Product' }}
                    </span>
                    <span *ngIf="saving">
                      <span class="spinner-border spinner-border-sm me-2"></span>Saving...
                    </span>
                  </button>
                  <button class="btn-shopez btn-outline-shopez" *ngIf="editingProduct" (click)="cancelEdit()">
                    <i class="bi bi-x-circle me-1"></i>Cancel
                  </button>
                </div>

              </div>
            </div>
          </div>

          <!-- RIGHT: Products Table -->
          <div class="admin-table-col">
            <div class="shopez-card">
              <div class="shopez-card-header d-flex align-items-center justify-content-between">
                <span><i class="bi bi-table me-2"></i>Product Catalog ({{ products.length }})</span>
                <input
                  type="text" class="table-search"
                  [(ngModel)]="tableSearch"
                  placeholder="Search products..."
                />
              </div>

              <!-- Loading -->
              <div class="shopez-spinner" *ngIf="loading">
                <div class="spinner"></div>
              </div>

              <!-- Table -->
              <div class="table-responsive" *ngIf="!loading">
                <table class="shopez-table">
                  <thead>
                    <tr>
                      <th style="width:50px">#</th>
                      <th style="width:70px">Image</th>
                      <th>Product Name</th>
                      <th style="width:100px">Price</th>
                      <th style="width:80px">Stock</th>
                      <th style="width:130px">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let product of filteredTableProducts">
                      <td class="text-muted small">{{ product.ProductId }}</td>
                      <td>
                        <img
                          [src]="getProductImage(product)"
                          [alt]="product.Name"
                          class="table-product-img"
                        />
                      </td>
                      <td>
                        <div class="table-product-name">{{ product.Name }}</div>
                        <div class="table-product-desc text-muted small">{{ product.Description | slice:0:50 }}...</div>
                      </td>
                      <td class="fw-bold">₹{{ product.Price | number:'1.0-0' }}</td>
                      <td>
                        <span class="badge-stock-in"  *ngIf="product.Stock > 0">{{ product.Stock }}</span>
                        <span class="badge-stock-out" *ngIf="product.Stock === 0">Out</span>
                      </td>
                      <td>
                        <div class="table-actions">
                          <button class="action-btn action-edit" (click)="startEdit(product)" title="Edit">
                            <i class="bi bi-pencil-fill"></i>
                          </button>
                          <button class="action-btn action-delete" (click)="deleteProduct(product.ProductId, product.Name)" title="Delete">
                            <i class="bi bi-trash-fill"></i>
                          </button>
                        </div>
                      </td>
                    </tr>
                    <tr *ngIf="filteredTableProducts.length === 0">
                      <td colspan="6" class="text-center text-muted py-4">No products found</td>
                    </tr>
                  </tbody>
                </table>
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
    /* Header */
    .admin-header { display:flex; align-items:flex-start; justify-content:space-between; flex-wrap:wrap; gap:16px; margin-bottom:24px; }
    .admin-title  { font-size:22px; font-weight:800; color:#212121; margin:0; }
    .admin-sub    { font-size:13px; color:#757575; margin:4px 0 0; }
    .admin-stats  { display:flex; gap:12px; }
    .stat-card {
      background:#fff; border:1px solid #e0e0e0; border-radius:8px;
      padding:12px 20px; text-align:center; min-width:90px;
      box-shadow:0 1px 4px rgba(0,0,0,.08);
    }
    .stat-card.stat-green { border-color:#c8e6c9; background:#f1f8e9; }
    .stat-card.stat-red   { border-color:#ffcdd2; background:#fff3f3; }
    .stat-num   { display:block; font-size:22px; font-weight:800; color:#212121; }
    .stat-label { display:block; font-size:11px; color:#757575; margin-top:2px; }

    /* Layout */
    .admin-layout { display:grid; grid-template-columns:360px 1fr; gap:16px; align-items:start; }
    .admin-form-col  { position:sticky; top:80px; }
    .admin-table-col {}

    /* Image preview */
    .img-preview-box { background:#f8f9fa; border:1px solid #e0e0e0; border-radius:8px; padding:8px; margin-bottom:12px; text-align:center; }
    .img-preview { height:80px; object-fit:contain; }

    /* Form actions */
    .form-action-row { display:flex; gap:10px; }

    /* Table search */
    .table-search {
      padding:6px 12px; border:1.5px solid #e0e0e0; border-radius:4px;
      font-size:13px; outline:none; width:200px;
    }
    .table-search:focus { border-color:#2874f0; }

    /* Table items */
    .table-product-img { width:48px; height:48px; object-fit:contain; border:1px solid #f0f0f0; border-radius:4px; }
    .table-product-name { font-size:14px; font-weight:600; color:#212121; }
    .table-product-desc { font-size:11px; }

    /* Action buttons */
    .table-actions { display:flex; gap:6px; }
    .action-btn {
      width:32px; height:32px; border:none; border-radius:6px;
      display:flex; align-items:center; justify-content:center;
      font-size:14px; transition:all .2s;
    }
    .action-edit   { background:#e3f2fd; color:#1565c0; }
    .action-edit:hover   { background:#1565c0; color:#fff; }
    .action-delete { background:#ffebee; color:#c62828; }
    .action-delete:hover { background:#c62828; color:#fff; }

    @media(max-width:900px){
      .admin-layout { grid-template-columns:1fr; }
      .admin-form-col { position:static; }
    }
  `]
})
export class AdminComponent implements OnInit {
  products: Product[] = [];
  loading = true;
  saving = false;
  formError = '';
  formSuccess = '';
  editingProduct: Product | null = null;
  tableSearch = '';

  form: CreateProductRequest = { Name:'', Description:'', Price:0, ImageUrl:'', Stock:0 };

  private imageMap: {[key:string]:string} = {
    mouse:'assets/images/mouse.jpg', keyboard:'assets/images/keyboard.jpg',
    hub:'assets/images/harddrive.jpg', usb:'assets/images/harddrive.jpg',
    hard:'assets/images/harddrive.jpg', drive:'assets/images/harddrive.jpg',
    laptop:'assets/images/laptop.jpg', monitor:'assets/images/monitor.jpg',
    headphone:'assets/images/headphones.jpg', speaker:'assets/images/speaker.jpg',
    mobile:'assets/images/mobile.jpg', phone:'assets/images/mobile.jpg',
    tablet:'assets/images/tablet.jpg', watch:'assets/images/watch.jpg',
  };

  constructor(private productService: ProductService) {}

  ngOnInit(): void { this.loadProducts(); }

  get inStockCount():    number { return this.products.filter(p => p.Stock > 0).length; }
  get outOfStockCount(): number { return this.products.filter(p => p.Stock === 0).length; }
  get filteredTableProducts(): Product[] {
    if (!this.tableSearch) return this.products;
    const q = this.tableSearch.toLowerCase();
    return this.products.filter(p => p.Name.toLowerCase().includes(q));
  }

  loadProducts(): void {
    this.loading = true;
    this.productService.getAllProducts().subscribe({
      next: (res) => { this.loading = false; if (res.success && res.data) this.products = res.data; },
      error: ()    => { this.loading = false; }
    });
  }

  saveProduct(): void {
    if (!this.form.Name || this.form.Price <= 0 || this.form.Stock < 0) {
      this.formError = 'Name (required), Price (> 0), Stock (≥ 0) are mandatory.'; return;
    }
    this.saving = true; this.formError = ''; this.formSuccess = '';
    const obs = this.editingProduct
      ? this.productService.updateProduct(this.editingProduct.ProductId, this.form)
      : this.productService.createProduct(this.form);

    obs.subscribe({
      next: (res) => {
        this.saving = false;
        if (res.success) {
          this.formSuccess = this.editingProduct ? `"${this.form.Name}" updated successfully!` : `"${this.form.Name}" added successfully!`;
          this.cancelEdit(); this.loadProducts();
          setTimeout(() => this.formSuccess = '', 3000);
        }
      },
      error: (err) => { this.saving = false; this.formError = err.error?.message ?? 'Operation failed. Please try again.'; }
    });
  }

  startEdit(product: Product): void {
    this.editingProduct = product;
    this.form = { Name:product.Name, Description:product.Description, Price:product.Price, ImageUrl:product.ImageUrl, Stock:product.Stock };
    this.formError = ''; this.formSuccess = '';
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancelEdit(): void {
    this.editingProduct = null;
    this.form = { Name:'', Description:'', Price:0, ImageUrl:'', Stock:0 };
  }

  deleteProduct(id: number, name: string): void {
    if (!confirm(`Are you sure you want to delete "${name}"?\nThis action cannot be undone.`)) return;
    this.productService.deleteProduct(id).subscribe({
      next: () => {
        this.formSuccess = `"${name}" deleted successfully.`;
        this.loadProducts();
        setTimeout(() => this.formSuccess = '', 3000);
      },
      error: (err) => { this.formError = err.error?.message ?? 'Delete failed.'; }
    });
  }

  getProductImage(product: Product): string {
    const name = product.Name.toLowerCase();
    for (const key of Object.keys(this.imageMap)) {
      if (name.includes(key)) return this.imageMap[key];
    }
    return product.ImageUrl || 'assets/images/mouse.jpg';
  }
}