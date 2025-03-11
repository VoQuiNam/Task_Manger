import axios from 'axios';

const API_URL = 'http://localhost:5260/api/users/login';

export async function loginUser(email, password) {
  try {
    const response = await axios.post(API_URL, { email, password });

    if (response.data.success) {
      localStorage.setItem('userToken', response.data.token);
      localStorage.setItem('userRole', response.data.role);
      return response.data.roleId; // Trả về roleId để điều hướng
    } else {
      throw new Error(response.data.message);
    }
  } catch (error) {
    console.error('Login error:', error);
    if (error.response && error.response.status === 401) {
      throw new Error(error.response.data.message || 'Email hoặc mật khẩu không đúng.');
    } else {
      throw new Error('Error logging in, please try again later.');
    }
  }
}
