import AdminLayout from "@/components/AdminLayout.vue";
import { Modal } from "bootstrap";
import axios from "axios"; // Import axios
import { v4 as uuidv4 } from "uuid";
import { toast } from "vue3-toastify";
import "vue3-toastify/dist/index.css";

export default {
    components: {
        AdminLayout,
    },
    name: "UserPage",
    data() {
        return {
            users: [], // Danh sách người dùng
            roles: [], // Danh sách roles
            modalInstance: null,
            newUser: {  // Khởi tạo đối tượng newUser
                FullName: "",
                email: "",
                password: "",
                RoleID: ""
            },
            isEditing: false,  // Biến xác định chế độ chỉnh sửa
            selectedUserId: null,  // Lưu ID người dùng đang chỉnh sửa
            searchQuery: "",
            currentPage: 1, // Trang hiện tại
            itemsPerPage: 5, // Số user mỗi trang
        };
    },
    methods: {
        async fetchUsers() {
            try {
                // Gọi API users và roles cùng lúc
                const [usersResponse, rolesResponse] = await Promise.all([
                    axios.get("http://localhost:5260/api/users/GetUsers"),
                    axios.get("http://localhost:5260/api/roles/GetRoles"),
                ]);

                // Kiểm tra dữ liệu có tồn tại không trước khi gán
                this.users = usersResponse.data || [];
                this.roles = rolesResponse.data || [];
            } catch (error) {
                console.error("Error fetching data:", error);
            }
        },
        getStatusClass(status) {
            return status === "Active" ? "badge bg-success" : "badge bg-danger";
        },
        openModal(user = null) {
            if (user) {
                // Chế độ chỉnh sửa
                this.isEditing = true;
                this.selectedUserId = user.User_ID;
                this.newUser = {
                    FullName: user.FullName,
                    email: user.Email,
                    password: user.Password,  // Không hiển thị mật khẩu cũ vì lý do bảo mật
                    RoleID: user.RoleID,
                };
            } else {
                // Chế độ thêm mới
                this.isEditing = false;
                this.selectedUserId = null;
                this.resetForm();
            }

            this.modalInstance = new Modal(document.getElementById("addUserModal"));
            this.modalInstance.show();
        }

        ,
        formatDate(dateString) {
            return new Date(dateString).toLocaleDateString("vi-VN");
        },
        
        getRoleName(roleId) {
            if (!this.roles || this.roles.length === 0) return "N/A"; // Kiểm tra roles có dữ liệu không
            const role = this.roles.find(r => r.RoleID === roleId);
            return role ? role.RoleName : "N/A";
        },

        async addUser() {
            try {
                if (!this.newUser.FullName || !this.newUser.email || !this.newUser.password || !this.newUser.RoleID) {
                    toast.error("Vui lòng nhập đầy đủ thông tin!");
                    return;
                }
        
                const newUserPayload = {
                    User_ID: uuidv4(), // Tạo UUID cho User_ID
                    FullName: this.newUser.FullName.trim(),
                    email: this.newUser.email.trim(),
                    password: this.newUser.password,
                    RoleID: Number(this.newUser.RoleID),
                    CreateAt: new Date().toISOString(), // Tạo ngày giờ hiện tại
                };
        
                console.log("Dữ liệu gửi lên API:", newUserPayload);
        
                const response = await axios.post(
                    `http://localhost:5260/api/users/AddUser?RoleID=${this.newUser.RoleID}`,
                    newUserPayload,
                    { headers: { "Content-Type": "application/json" } }
                );
        
                if (response.status === 200 && response.data.success) {
                    toast.success("Thêm người dùng thành công!"); // Hiển thị thông báo thành công
                    this.fetchUsers();
                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    toast.error(response.data.message || "Đã xảy ra lỗi khi thêm người dùng!");
                }
            } catch (error) {
                console.error("Lỗi khi thêm người dùng:", error);
                toast.error("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },

        async updateUser() {
            try {
                if (!this.newUser.FullName || !this.newUser.email || !this.newUser.RoleID || !this.newUser.password) {
                    console.log(this.newUser.FullName);
                    console.log(this.newUser.email);
                    console.log(this.newUser.RoleID);
                    console.log(this.newUser.password);
                    alert("Vui lòng nhập đầy đủ thông tin!");

                    return;
                }

                // Lấy User_ID từ API dựa vào email
                // 🛠 Kiểm tra API GetUserById
                const userResponse = await axios.get(`http://localhost:5260/api/users/GetUserById?User_ID=${this.selectedUserId}`);
                console.log('Phản hồi từ API GetUserById:', this.selectedUserId);
                if (!userResponse.data.success || !userResponse.data.user.length) {
                    alert("Không tìm thấy người dùng!");
                    return;
                }

                const user = userResponse.data.user[0]; // Lấy thông tin user đầu tiên
                const userId = user.User_ID; // Lấy User_ID

                const updatePayload = {
                    User_ID: userId,
                    FullName: this.newUser.FullName.trim(),
                    email: this.newUser.email.trim(),
                    RoleID: Number(this.newUser.RoleID),
                    password: this.newUser.password.trim(), // Thêm mật khẩu nếu API yêu cầu
                    CreateAt: new Date().toISOString(),
                };

                console.log("Dữ liệu gửi lên API:", updatePayload);

                const response = await axios.put(
                    `http://localhost:5260/api/users/UpdateUser?User_ID=${this.selectedUserId}&RoleID=${this.newUser.RoleID}`,
                    updatePayload,
                    { headers: { "Content-Type": "multipart/form-data" } }
                );

                if (response.status === 200 && response.data.success) {
                    alert("Cập nhật người dùng thành công!");
                    this.fetchUsers();
                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    alert(response.data.message || "Đã xảy ra lỗi khi cập nhật người dùng!");
                }
            } catch (error) {
                console.error("Lỗi khi cập nhật người dùng:", error);
                alert("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },

        resetForm() {
            this.newUser = {
                FullName: "",
                email: "",
                password: "",
                RoleID: "", // Reset về giá trị mặc định
            };
        },

        async deleteUser(id) {
            if (!confirm("Bạn có chắc chắn muốn xóa người dùng này không?")) return;

            try {
                const response = await axios.delete("http://localhost:5260/api/users/DeleteUser", {
                    params: { id: id },
                    headers: { "Content-Type": "application/json" }
                });

                if (response.status === 200) {
                    alert("Xóa người dùng thành công!");
                    this.fetchUsers();
                }
            } catch (error) {
                console.error("Lỗi khi xóa người dùng:", error);
                alert("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },
        
        async handleSubmit() {
            if (this.isEditing) {
                await this.updateUser();
            } else {
                await this.addUser();
            }
        },

        filterUsers() {
            if (!this.searchQuery) {
                return this.users;
            }
            return this.users.filter(user =>
                user.FullName.toLowerCase().includes(this.searchQuery.toLowerCase())
            );
        },

        goToPage(page) {
            if (page >= 1 && page <= this.totalPages) {
                this.currentPage = page;
            }
        }


    },
    //Lọc danh sách, tính toán giá trị động
    computed: {
        //loc danh sach tu danh sach goc
        filteredUsers() {
            return this.filterUsers();
        },
        // Tính danh sách user cho trang hiện tại
        paginatedUsers() {
            const start = (this.currentPage - 1) * this.itemsPerPage;
            return this.filteredUsers.slice(start, start + this.itemsPerPage);
        },
        // Tính tổng số trang
        //Hàm Math.ceil() làm tròn lên
        totalPages() {
            return Math.ceil(this.filteredUsers.length / this.itemsPerPage);
        }
    },
    //Gọi API, thao tác DOM, đăng ký sự kiện
    //Nếu fetchUsers() thực hiện một API call để lấy 
    // danh sách người dùng, thì component sẽ nhận dữ liệu và cập nhật giao diện.
    //khi component thêm vào dom thì mounted sẽ dc gọi 
    mounted() {
        this.fetchUsers(); // Gọi API khi component được mount
    },
};