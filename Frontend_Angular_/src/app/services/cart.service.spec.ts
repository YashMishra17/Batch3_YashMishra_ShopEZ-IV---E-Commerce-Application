import { TestBed } from '@angular/core/testing';
import { CartService } from './cart.service';
import { CartProduct } from '../models/order.model';

describe('CartService', () => {
  let service: CartService;

  // Mock product used across tests
  const mockProduct: CartProduct = {
    ProductId: 1,
    Name: 'Mouse',
    Price: 500,
    Stock: 5,
    Quantity: 1,
    ImageUrl: ''
  };

  // Helper to create a fresh instance
  // Needed because service reads localStorage in constructor
  function createService(): CartService {
    TestBed.resetTestingModule();

    TestBed.configureTestingModule({
      providers: [CartService]
    });

    return TestBed.inject(CartService);
  }

  beforeEach(() => {
    // Always reset storage → prevents test leakage
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [CartService]
    });

    service = TestBed.inject(CartService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  // -----------------------------
  // ADD TO CART
  // -----------------------------
  it('should add new product to cart', () => {
    // Act
    service.addToCart(mockProduct);

    // Assert
    const cart = service.getCart();
    expect(cart.length).toBe(1);
    expect(cart[0].Quantity).toBe(1);
  });

  it('should increase quantity if product already exists', () => {
    // Add same product twice
    service.addToCart(mockProduct);
    service.addToCart(mockProduct);

    // Should NOT create duplicate entry, only increase quantity
    const cart = service.getCart();
    expect(cart.length).toBe(1);
    expect(cart[0].Quantity).toBe(2);
  });

  it('should NOT exceed stock limit', () => {
    // Try adding more than available stock
    for (let i = 0; i < 10; i++) {
      service.addToCart(mockProduct);
    }

    // Quantity should be capped at stock (5)
    const cart = service.getCart();
    expect(cart[0].Quantity).toBe(5);
  });

  // -----------------------------
  // REMOVE
  // -----------------------------
  it('should remove product from cart', () => {
    service.addToCart(mockProduct);

    // Remove by product ID
    service.removeFromCart(1);

    expect(service.getCart().length).toBe(0);
  });

  // -----------------------------
  // INCREASE QUANTITY
  // -----------------------------
  it('should increase quantity manually', () => {
    service.addToCart(mockProduct);

    // Explicit increase
    service.increaseQuantity(1);

    expect(service.getCart()[0].Quantity).toBe(2);
  });

  it('should NOT increase beyond stock', () => {
    // Fill to max stock first
    for (let i = 0; i < 5; i++) service.addToCart(mockProduct);

    // Try exceeding stock
    service.increaseQuantity(1);

    expect(service.getCart()[0].Quantity).toBe(5);
  });

  // -----------------------------
  // DECREASE QUANTITY
  // -----------------------------
  it('should decrease quantity', () => {
    service.addToCart(mockProduct);
    service.addToCart(mockProduct);

    service.decreaseQuantity(1);

    expect(service.getCart()[0].Quantity).toBe(1);
  });

  it('should remove item when quantity becomes 0', () => {
    service.addToCart(mockProduct);

    // Decreasing from 1 → should remove item completely
    service.decreaseQuantity(1);

    expect(service.getCart().length).toBe(0);
  });

  // -----------------------------
  // TOTAL COUNT
  // -----------------------------
  it('should return total item count', () => {
    service.addToCart(mockProduct);
    service.addToCart(mockProduct);

    // Total = sum of quantities (not number of products)
    expect(service.getCartCount()).toBe(2);
  });

  // -----------------------------
  // TOTAL PRICE
  // -----------------------------
  it('should calculate total price', () => {
    service.addToCart(mockProduct);
    service.addToCart(mockProduct);

    // 500 * 2 = 1000
    expect(service.getCartTotal()).toBe(1000);
  });

  // -----------------------------
  // CLEAR CART
  // -----------------------------
  it('should clear cart completely', () => {
    service.addToCart(mockProduct);

    service.clearCart();

    // Should remove from both memory + localStorage
    expect(service.getCart().length).toBe(0);
    expect(localStorage.getItem('shopez_cart')).toBeNull();
  });

  // -----------------------------
  // INITIAL LOAD FROM STORAGE
  // -----------------------------
  it('should load cart from localStorage on init', () => {
    // Pre-populate storage BEFORE service creation
    localStorage.setItem('shopez_cart', JSON.stringify([mockProduct]));

    const freshService = createService();

    // Service should read stored cart on init
    expect(freshService.getCart().length).toBe(1);
  });
});
