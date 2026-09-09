import axios from "axios";

const API_URL = "http://localhost:5018/api/Auth";

export const loginApi = async (
    email: string,
    password: string
) => {
    const response = await axios.post(
        `${API_URL}/login`,
        null,
        {
            params: {
                email: email,
                password: password
            }
        }
    );

    return response.data;
};