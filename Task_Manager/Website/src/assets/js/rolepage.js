import AdminLayout from "@/components/AdminLayout.vue";
import { Modal } from "bootstrap";
import axios from "axios"; // Import axios

export default {
    components: {
        AdminLayout,
    },
    name: "RolePage",
    data() {
        return {
            roles: [], // Danh sách roles
            modalInstance: null,
            newRole: {  // Khởi tạo đối tượng newUser
                RoleID: "",
                RoleName: "",
            },
            isEditing: false,  // Biến xác định chế độ chỉnh sửa
            selectedRoleId: null,  // Lưu ID người dùng đang chỉnh sửa
            searchQuery: "",
            currentPage: 1, // Trang hiện tại
            itemsPerPage: 5, // Số user mỗi trang
        };
    },
    methods: {
        async fetchRoles() {
            try {
                console.log('được gọi r');
                // Gọi API users và roles cùng lúc
                const rolesResponse = await axios.get("http://localhost:5260/api/roles/GetRoles");

                console.log('logra: ', rolesResponse);
                // Kiểm tra dữ liệu có tồn tại không trước khi gán
                this.roles = rolesResponse.data || [];
            } catch (error) {
                console.error("Error fetching data:", error);
            }
        },
        getStatusClass(status) {
            return status === "Active" ? "badge bg-success" : "badge bg-danger";
        },
        openModal(role = null) {
            if (role) {
                // Chế độ chỉnh sửa
                this.isEditing = true;
                this.selectedRoleId = role.RoleID;
                this.newRole = {
                    RoleName: role.RoleName,
                };
            } else {
                // Chế độ thêm mới
                this.isEditing = false;
                this.selectedRoleId = null;
                this.resetForm();
            }

            this.modalInstance = new Modal(document.getElementById("addRoleModal"));
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
        async addRole() {
            try {
                if (!this.newRole.RoleName) {
                    alert("Vui lòng nhập đầy đủ thông tin!");
                    return;
                }


                const newRolePayload = {
                    RoleName: this.newRole.RoleName.trim(),
                };

                console.log("Dữ liệu gửi lên API:", newRolePayload);



                const response = await axios.post(
                    `http://localhost:5260/api/roles/AddRoles`,
                    newRolePayload,
                    { headers: { "Content-Type": "multipart/form-data" } }
                );


                if (response.status === 200 && response.data.success) {
                    alert("Thêm vai trò thành công!");
                    this.fetchRoles();
                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    alert(response.data.message || "Đã xảy ra lỗi khi thêm vai trò!");
                }
            } catch (error) {
                console.error("Lỗi khi thêm vai trò:", error);
                alert("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },
        async updateRole() {
            try {

                

                if (!this.newRole.RoleName) {
                    console.log(this.newRole.RoleName);
                    alert("Vui lòng nhập đầy đủ thông tin!");

                    return;
                }

                // Lấy User_ID từ API dựa vào email
                const roleResponse = await axios.get(`http://localhost:5260/api/roles/GetRoleById?RoleID=${this.selectedRoleId}`);
                console.log('Phản hồi từ API GetUserById:', this.selectedRoleId);
                if (!roleResponse.data.success || !roleResponse.data.role.length) {
                    alert("Không tìm thấy vai trò!");
                    return;
                }

                const role = roleResponse.data.role[0]; // Lấy thông tin user đầu tiên
                const roleId = role.RoleID; // Lấy User_ID
                console.log('newRole: ', roleId);
                const updatePayload = {
                    RoleID: roleId,
                    RoleName: this.newRole.RoleName.trim(),
            
                };

                console.log("Dữ liệu gửi lên API:", updatePayload);

                const response = await axios.put(
                    `http://localhost:5260/api/roles/UpdateRoles?RoleID=${this.selectedRoleId}`,
                    updatePayload,
                    { headers: { "Content-Type": "multipart/form-data" } }
                );

                if (response.status === 200 && response.data.success) {
                    alert("Cập nhật vai trò thành công!");
                    this.fetchRoles();
                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    alert(response.data.message || "Đã xảy ra lỗi khi cập nhật vai trò!");
                }
            } catch (error) {
                console.error("Lỗi khi cập nhật vai trò:", error);
                alert("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },

        resetForm() {
            this.newRole = {
                RoleName: "", // Reset về giá trị mặc định
            };
        },

        async deleteRole(id) {
            if (!confirm("Bạn có chắc chắn muốn xóa vai trò này không?")) return;
            console.log('id: ', id);
            try {
                const response = await axios.delete(`http://localhost:5260/api/roles/DeleteRoles?roleID=${id}`);
        
                console.log("response: ", response);
                if (response.status === 200 && response.data.success) {
                    alert("Xóa vai trò thành công!");
                    this.fetchRoles();
                } else {
                    alert(response.data.message || "Không thể xóa vai trò!");
                }
            } catch (error) {
                console.error("Lỗi khi xóa vai trò:", error);
                alert("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },

        async handleSubmit() {
            if (this.isEditing) {
             await this.updateRole();
            } else {
                await this.addRole();
            }
        },

        filterRoles() {
            if (!this.searchQuery) {
                return this.roles;
            }
            return this.roles.filter(role =>
                role.RoleName.toLowerCase().includes(this.searchQuery.toLowerCase())
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
        filteredRoles() {
            return this.filterRoles();
        },
        // Tính danh sách user cho trang hiện tại
        paginatedRoles() {
            const start = (this.currentPage - 1) * this.itemsPerPage;
            return this.filteredRoles.slice(start, start + this.itemsPerPage);
        },
        // Tính tổng số trang
        totalPages() {
            return Math.ceil(this.filteredRoles.length / this.itemsPerPage);
        }
    },
    //Gọi API, thao tác DOM, đăng ký sự kiện
    mounted() {
        this.fetchRoles(); // Gọi API khi component được mount
    },
};