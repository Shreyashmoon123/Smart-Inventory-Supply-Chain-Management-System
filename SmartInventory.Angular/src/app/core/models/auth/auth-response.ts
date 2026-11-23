export interface ApiResponse<T> {
  message: string;
  data: T;
}

export interface AuthResponseData {
  userId: number;
  fullName: string;
  email: string;
  token: string;
  expiresAt: string;
  roles: string[];
}