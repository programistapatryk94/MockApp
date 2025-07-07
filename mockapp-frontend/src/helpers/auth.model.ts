
export interface CreateUserInput {
    email: string;
    password: string;
}

export interface AuthResponse {
  token: string;
}