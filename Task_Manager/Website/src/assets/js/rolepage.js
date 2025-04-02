import AdminLayout from "@/components/AdminLayout.vue";
import { Modal } from "bootstrap";
import axios from "axios"; // Import axios
import { toast } from "vue3-toastify";
import "vue3-toastify/dist/index.css";
import Swal from "sweetalert2";

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
        },

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
                    toast.error("Vui lòng nhập đầy đủ thông tin!");
                    return;
                }

                // Kiểm tra độ dài (không quá 10 ký tự)
                if (this.newRole.RoleName.length > 10) {
                    toast.error("Tên vai trò không được quá 10 ký tự!");
                    return;
                }

                // Kiểm tra ký tự đặc biệt (chỉ cho phép chữ cái và số)
                const specialCharRegex = /[^a-zA-Z0-9 ]/;
                if (specialCharRegex.test(this.newRole.RoleName)) {
                    toast.error("Tên vai trò không được chứa ký tự đặc biệt!");
                    return;
                }

                const rolenameCheckResponse = await axios.get(
                    `http://localhost:5260/api/roles/CheckRoleExists?rolename=${encodeURIComponent(this.newRole.RoleName)}`
                );

                if (rolenameCheckResponse.data.exists) {
                    toast.error("Role name đã tồn tại, vui lòng nhập role khác!");
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
                    toast.success("Thêm vai trò thành công!");
                    await this.fetchRoles();

                    // Tìm vị trí user mới bằng email thay vì User_ID
                    const newRoleIndex = this.roles.findIndex(role => role.RoleName == newRolePayload.RoleName);

                    // khác -1 là user đó đã có
                    if (newRoleIndex != -1) {
                        // Xác định trang mới chứa user vừa thêm(công thức tính đúng số trang)
                        this.currentPage = Math.ceil((newRoleIndex + 1) / this.itemsPerPage);
                    }

                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    toast.error(response.data.message || "Đã xảy ra lỗi khi thêm vai trò!");
                }
            } catch (error) {
                console.error("Lỗi khi thêm vai trò:", error);
                toast.error("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },

        async updateRole() {
            try {
                if (!this.newRole.RoleName) {
                    toast.error("Vui lòng nhập đầy đủ thông tin!");
                    return;
                }

                // Kiểm tra độ dài (không quá 10 ký tự)
                if (this.newRole.RoleName.length > 10) {
                    toast.error("Tên vai trò không được quá 10 ký tự!");
                    return;
                }

                // Kiểm tra ký tự đặc biệt (chỉ cho phép chữ cái và số)
                const specialCharRegex = /[^a-zA-Z0-9 ]/;
                if (specialCharRegex.test(this.newRole.RoleName)) {
                    toast.error("Tên vai trò không được chứa ký tự đặc biệt!");
                    return;
                }

                // Kiểm tra trùng tên vai trò
                const rolenameCheckResponse = await axios.get(
                    `http://localhost:5260/api/roles/CheckRoleExists?rolename=${encodeURIComponent(this.newRole.RoleName)}&excludeId=${this.selectedRoleId}`
                );

                if (rolenameCheckResponse.data.exists) {
                    toast.error("Role name đã tồn tại, vui lòng nhập role khác!");
                    return;
                }

                // Lấy thông tin vai trò hiện tại từ API
                const roleResponse = await axios.get(`http://localhost:5260/api/roles/GetRoleById?RoleID=${this.selectedRoleId}`);
                if (!roleResponse.data.success || !roleResponse.data.role.length) {
                    toast.error("Không tìm thấy vai trò!");
                    return;
                }

                const role = roleResponse.data.role[0];
                const roleId = role.RoleID;

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
                    toast.success("Cập nhật vai trò thành công!");
                    this.fetchRoles();
                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    toast.error(response.data.message || "Đã xảy ra lỗi khi cập nhật vai trò!");
                }
            } catch (error) {
                console.error("Lỗi khi cập nhật vai trò:", error);
                toast.error("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },


        resetForm() {
            this.newRole = {
                RoleName: "", // Reset về giá trị mặc định
            };
        },

        async deleteRole(id) {
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
                const response = await axios.delete(`http://localhost:5260/api/roles/DeleteRoles?roleID=${id}`);

                console.log("response: ", response);
                if (response.status === 200 && response.data.success) {
                    toast.success("Xóa vai trò thành công!");
                    await this.fetchRoles();

                    // Kiểm tra nếu trang hiện tại không còn vai trò nào, thì quay về trang trước
                    const totalPagesAfterDelete = Math.ceil(this.roles.length / this.itemsPerPage);
                    if (this.currentPage > totalPagesAfterDelete) {
                        this.currentPage = Math.max(1, totalPagesAfterDelete);
                    }
                } else {
                    toast.error(response.data.message || "Không thể xóa vai trò!");
                }
            } catch (error) {
                console.error("Lỗi khi xóa vai trò:", error);
                toast.error("Đã xảy ra lỗi, vui lòng thử lại!");
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