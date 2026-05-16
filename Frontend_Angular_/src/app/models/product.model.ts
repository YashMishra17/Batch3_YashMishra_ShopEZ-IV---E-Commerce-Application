// Matches ProductDTO.cs exactly
export interface Product {
  ProductId: number;
  Name: string;
  Description: string;
  Price: number;
  ImageUrl: string;
  Stock: number;
}

// Matches CreateProductDTO.cs exactly
export interface CreateProductRequest {
  Name: string;
  Description: string;
  Price: number;
  ImageUrl: string;
  Stock: number;
}

// Matches UpdateProductDTO.cs exactly
export interface UpdateProductRequest {
  Name: string;
  Description: string;
  Price: number;
  ImageUrl: string;
  Stock: number;
}