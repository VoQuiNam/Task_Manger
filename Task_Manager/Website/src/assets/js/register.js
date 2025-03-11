import axios from 'axios';
import Swal from 'sweetalert2';

const API_URL = 'http://localhost:5260/api/users'; // Cấu hình API

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
            return true; // Trả về true nếu đăng ký thành công
        }
    } catch (error) {
        throw error.response?.data?.message || "Đăng ký thất bại. Vui lòng thử lại!";
    }
}
