import axios from 'axios';
import { getAuth, signInWithPopup, GoogleAuthProvider, FacebookAuthProvider } from "firebase/auth";
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

export async function signInWithGoogle() {
  try {
    const auth = getAuth();
    const provider = new GoogleAuthProvider();
    const result = await signInWithPopup(auth, provider);
    const user = result.user;

    // Lưu token vào localStorage
    localStorage.setItem("userToken", user.accessToken);
    localStorage.setItem("userEmail", user.email);
    localStorage.setItem("userName", user.displayName);

    // Chuyển hướng sau khi đăng nhập
    this.$router.push("/user");
  } catch (error) {
    this.errorMessage = "Google login failed: " + error.message;
  }
} 

export async function signInWithFacebook() {
  const auth = getAuth();
  const provider = new FacebookAuthProvider();

  try {
    const result = await signInWithPopup(auth, provider);
    console.log("User signed in with Facebook:", result.user);
    this.$router.push("/user");
  } catch (error) {
    this.errorMessage = error.message;
    console.error("Facebook Sign-In Error:", error);
  }
}
