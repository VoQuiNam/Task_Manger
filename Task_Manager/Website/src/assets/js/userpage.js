import AdminLayout from "@/components/AdminLayout.vue";
import { Modal } from "bootstrap";
import axios from "axios"; // Import axios
import { v4 as uuidv4 } from "uuid";
import { toast } from "vue3-toastify";
import "vue3-toastify/dist/index.css";
import Swal from "sweetalert2";


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

                // Kiểm tra định dạng email hợp lệ
                const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailPattern.test(this.newUser.email)) {
                    toast.error("Email không hợp lệ!");
                    return;
                }

                // Kiểm tra độ dài mật khẩu tối thiểu 6 ký tự
                const passwordPattern = /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/;
                if (!passwordPattern.test(this.newUser.password)) {
                    toast.error("Mật khẩu phải có ít nhất 6 ký tự, chứa chữ hoa, chữ thường, số và ký tự đặc biệt!");
                    return;
                }

                // Kiểm tra RoleID phải là số hợp lệ và lớn hơn 0
                if (isNaN(this.newUser.RoleID) || Number(this.newUser.RoleID) <= 0) {
                    toast.error("Vui lòng chọn vai trò hợp lệ!");
                    return;
                }

                // Kiểm tra xem email đã tồn tại chưa
                const emailCheckResponse = await axios.get(
                    `http://localhost:5260/api/users/CheckEmailExists?email=${encodeURIComponent(this.newUser.email)}`
                );

                if (emailCheckResponse.data.exists) {
                    toast.error("Email đã tồn tại, vui lòng chọn email khác!");
                    return;
                }

                // Kiểm tra độ dài tên (giới hạn 100 ký tự)
                if (this.newUser.FullName.length > 20) {
                    toast.error("Tên quá dài, tối đa 20 ký tự!");
                    return;
                }

                if (this.newUser.email.length > 30) {
                    toast.error("Email quá dài, tối đa 30 ký tự!");
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
                    await this.fetchUsers();

                    // Tìm vị trí user mới bằng email thay vì User_ID
                    const newUserIndex = this.users.findIndex(user => user.Email === newUserPayload.email);

                    // khác -1 là user đó đã có
                    if (newUserIndex != -1) {
                        // Xác định trang mới chứa user vừa thêm(công thức tính đúng số trang)
                        this.currentPage = Math.ceil((newUserIndex + 1) / this.itemsPerPage);
                    }

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
                    toast.error("Vui lòng nhập đầy đủ thông tin!");
                    return;
                }

                // Kiểm tra định dạng email hợp lệ
                const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailPattern.test(this.newUser.email)) {
                    toast.error("Email không hợp lệ!");
                    return;
                }

                // Kiểm tra độ dài mật khẩu tối thiểu 6 ký tự và phải chứa chữ hoa, chữ thường, số và ký tự đặc biệt
                const passwordPattern = /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/;
                if (!passwordPattern.test(this.newUser.password)) {
                    toast.error("Mật khẩu phải có ít nhất 6 ký tự, chứa chữ hoa, chữ thường, số và ký tự đặc biệt!");
                    return;
                }

                // Kiểm tra RoleID phải là số hợp lệ và lớn hơn 0
                if (isNaN(this.newUser.RoleID) || Number(this.newUser.RoleID) <= 0) {
                    toast.error("Vui lòng chọn vai trò hợp lệ!");
                    return;
                }

                // Kiểm tra độ dài tên và email
                if (this.newUser.FullName.length > 20) {
                    toast.error("Tên quá dài, tối đa 20 ký tự!");
                    return;
                }

                if (this.newUser.email.length > 30) {
                    toast.error("Email quá dài, tối đa 30 ký tự!");
                    return;
                }

                // Lấy User_ID từ API dựa vào email
                // 🛠 Kiểm tra API GetUserById
                const userResponse = await axios.get(`http://localhost:5260/api/users/GetUserById?User_ID=${this.selectedUserId}`);
                console.log('Phản hồi từ API GetUserById:', this.selectedUserId);
                if (!userResponse.data.success || !userResponse.data.user.length) {
                    toast.error("Không tìm thấy người dùng!");
                    return;
                }

                const user = userResponse.data.user[0]; // Lấy thông tin user đầu tiên
                const userId = user.User_ID; // Lấy User_ID

                // Nếu email thay đổi, kiểm tra email có tồn tại chưa
                if (this.newUser.email !== user.email) {
                    const emailCheckResponse = await axios.get(
                        `http://localhost:5260/api/users/CheckEmailExists?email=${encodeURIComponent(this.newUser.email)}&excludeId=${this.selectedUserId}`
                    );
                    if (emailCheckResponse.data.exists) {
                        toast.error("Email đã tồn tại, vui lòng chọn email khác!");
                        return;
                    }
                }

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
                    toast.success("Cập nhật người dùng thành công!");
                    this.fetchUsers();
                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    toast.error(response.data.message || "Đã xảy ra lỗi khi cập nhật người dùng!");
                }
            } catch (error) {
                console.error("Lỗi khi cập nhật người dùng:", error);
                toast.error("Đã xảy ra lỗi, vui lòng thử lại!");
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
            const result = await Swal.fire({
                title: "Bạn có chắc chắn muốn xóa không?",
                text: "Hành động này không thể hoàn tác!",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Xóa",
                cancelButtonText: "Hủy",
                customClass: {
                    confirmButton: "btn-confirm-delete",  // Thêm class tùy chỉnh
                    cancelButton: "btn-cancel"
                },
            });

            if (!result.isConfirmed) return;

            try {
                const response = await axios.delete("http://localhost:5260/api/users/DeleteUser", {
                    params: { id: id },
                    headers: { "Content-Type": "application/json" }
                });

                if (response.status === 200) {
                    toast.success("Xóa người dùng thành công!");
                    await this.fetchUsers();

                     // Kiểm tra nếu trang hiện tại không còn vai trò nào, thì quay về trang trước
                     const totalPagesAfterDelete = Math.ceil(this.users.length / this.itemsPerPage);
                     if (this.currentPage > totalPagesAfterDelete) {
                         this.currentPage = Math.max(1, totalPagesAfterDelete);
                     }
                }
            } catch (error) {
                console.error("Lỗi khi xóa người dùng:", error);
                toast.error("Đã xảy ra lỗi, vui lòng thử lại!");
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