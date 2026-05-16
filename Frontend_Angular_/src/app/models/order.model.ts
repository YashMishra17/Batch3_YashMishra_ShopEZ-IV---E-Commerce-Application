// Matches CartItemDTO.cs exactly
export interface CartItem {
  ProductId: number;
  Quantity: number;
}

// Matches CreateOrderDTO.cs exactly
export interface CreateOrderRequest {
  UserId: number;
  CartItems: CartItem[];
}

// Matches OrderItemDTO.cs exactly
export interface OrderItem {
  OrderItemId: number;
  ProductId: number;
  ProductName: string;
  Quantity: number;
  Price: number;
  Subtotal: number;
}

// Matches OrderDTO.cs exactly
export interface Order {
  OrderId: number;
  UserId: number;
  UserName: string;
  OrderDate: string;
  TotalAmount: number;
  OrderItems: OrderItem[];
}

// Cart item stored in localStorage (includes product details)
export interface CartProduct {
  ProductId: number;
  Name: string;
  Price: number;
  ImageUrl: string;
  Stock: number;
  Quantity: number;
}