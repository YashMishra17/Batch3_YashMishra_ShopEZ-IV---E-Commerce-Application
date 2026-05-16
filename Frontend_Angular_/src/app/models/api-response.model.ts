// Matches { success, data } envelope from all your controllers
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: any;
}