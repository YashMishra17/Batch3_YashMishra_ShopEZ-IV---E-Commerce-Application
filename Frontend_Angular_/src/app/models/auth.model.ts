// Matches LoginDTO.cs exactly
export interface LoginRequest {
  Email: string;
  Password: string;
}

// Matches RegisterDTO.cs exactly
export interface RegisterRequest {
  Name: string;
  Email: string;
  Password: string;
  Role: string;
}

// Matches AuthResponseDTO.cs exactly
export interface AuthResponse {
  UserId: number;
  Name: string;
  Email: string;
  Role: string;
  Token: string;
  ExpiresAt: string;
}