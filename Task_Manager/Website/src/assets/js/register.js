import axios from 'axios';
import Swal from 'sweetalert2';

const API_URL = 'http://localhost:5260/api/users'; // Cấu hình API

// Kiểm tra email có tồn tại không
export async function checkEmailExists(email) {
    try {
        const response = await axios.get(`${API_URL}/CheckEmailExists?email=${email}`);
        return response.data; // Trả về true nếu email tồn tại
    } catch (error) {
        return false; // Mặc định email chưa tồn tại nếu có lỗi API
    }
}

// Đăng ký người dùng
export async function registerUser(fullname, email, password) {
    try {
        const response = await axios.post(`${API_URL}/AddUser?RoleID=2`, {
            fullName: fullname,
            email,
            password
        });

        if (response.status === 200 || response.status === 201) {
            await Swal.fire({
                title: 'Đăng ký thành công!',
                text: 'Bạn sẽ được chuyển đến trang đăng nhập.',
                icon: 'success',
                confirmButtonText: 'OK',
                timer: 5000,
                timerProgressBar: true,
            });

            document.body.style.overflow = "auto"; // ✅ Khôi phục overflow
            return true;
        }
    } catch (error) {
        throw error.response?.data?.message || "Đăng ký thất bại. Vui lòng thử lại!";
    }
}
