import axios from "axios";

const API_URL = "http://localhost:5018/api";

export interface RegisterRequest {
  name: string;
  email: string;
  passwordHash: string;
  roleId: number;
}

export interface RegisterResponse {
  success: boolean;
  message: string;
  user?: {
    id: number;
    name: string;
    email: string;
    roleId: number;
  };
}

export const registerUser = async (
  data: RegisterRequest
): Promise<RegisterResponse> => {
  const response = await axios.post<RegisterResponse>(
    `${API_URL}/Auth/register`,
    data
  );

  return response.data;
};