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
    name: "RoleModulePage",
    data() {
        return {
            rolemodules: [], // Danh sách roles
            roles: [], // Danh sách roles
            modules: [],
            selectedRole: "",
            modalInstance: null,
            newrolemodules: {  // Khởi tạo đối tượng newUser
                RoleModuleID: "",
                RoleID: "",
                ModuleID: "",
                CanView: "",
                CanCreate: "",
                CanEdit: "",
                CanDelete: ""
            },
            isEditing: false,  // Biến xác định chế độ chỉnh sửa
            selectedRoleModuleId: null,  // Lưu ID người dùng đang chỉnh sửa
            searchQuery: "",
            currentPage: 1, // Trang hiện tại
            itemsPerPage: 5, // Số user mỗi trang
        };
    },
    methods: {
        async fetchRoleModule() {
            try {
                const [rolemoduleResponse, rolesResponse, modulesResponse] = await Promise.all([
                    axios.get("http://localhost:5260/api/rolemodules/GetRoleModule"),
                    axios.get("http://localhost:5260/api/roles/GetRoles"),
                    axios.get("http://localhost:5260/api/modules/GetModules"),
                ]);

                this.rolemodules = rolemoduleResponse.data || [];
                this.roles = rolesResponse.data || [];
                this.modules = modulesResponse.data || [];
                // Kết hợp dữ liệu từ rolemodules và modules dựa trên ModuleID
                this.menuItems = this.rolemodules
                    .filter(roleModule => roleModule.CanView) // Chỉ lấy module có CanView = true
                    //Dùng để biến đổi từng phần tử của mảng thành giá trị mới.
                    //Trả về một mảng mới với các phần tử đã được thay đổi.
                    .map(roleModule => {
                        const module = this.modules.find(m => m.ModuleID == roleModule.ModuleID); // Tìm module tương ứng
                        return module ? {
                            path: module.Link || "#", // Đảm bảo không bị undefined
                            icon: module.Icon || "fa-folder", // Dùng icon mặc định nếu không có
                            label: module.ModuleName || "No Name",
                            CanView: roleModule.CanView,
                            CanCreate: roleModule.CanCreate,
                            CanEdit: roleModule.CanEdit,
                            CanDelete: roleModule.CanDelete
                        } : null;
                    })
                    .filter(item => item !== null); // Loại bỏ phần tử null nếu không tìm thấy module
                this.updateActiveIndex();
            } catch (error) {
                console.error("Error fetching data:", error);
            }
        },

        updateActiveIndex() {
            const currentPath = this.$route.path;
            const foundIndex = this.menuItems.findIndex(item => item.path === currentPath);
            if (foundIndex !== -1) {
                this.activeIndex = foundIndex;
            } else {
                // Nếu path hiện tại không nằm trong menu do bị phân quyền → chuyển hướng về /dashboard
                this.$router.push('/dashboard');
            }
        },
        openModal(rolemodule = null) {
            if (rolemodule) {
                // Chế độ chỉnh sửa
                this.isEditing = true;
                this.selectedRoleModuleId = rolemodule.RoleModuleID;
                this.newrolemodules = {
                    RoleID: rolemodule.RoleID,
                    ModuleID: rolemodule.ModuleID,
                };
            } else {
                // Chế độ thêm mới
                this.isEditing = false;
                this.selectedRoleModuleId = null;
                this.resetForm();
            }

            this.modalInstance = new Modal(document.getElementById("addRoleModuleModal"));
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

        getModuleName(moduleId) {
            if (!this.modules || this.modules.length === 0) return "N/A"; // Kiểm tra roles có dữ liệu không
            const module = this.modules.find(r => r.ModuleID === moduleId);
            return module ? module.ModuleName : "N/A";
        },

        async addRoleModule() {
            try {
                // Kiểm tra dữ liệu đầu vào
                if (!this.newrolemodules.RoleID || !this.newrolemodules.ModuleID) {
                    toast.error("Please enter full RoleID and ModuleID!");
                    return;
                }

                // Kiểm tra xem RoleID và ModuleID đã tồn tại chưa
                const checkResponse = await axios.get(
                    `http://localhost:5260/api/rolemodules/CheckRoleAndModuleExists?roleid=${this.newrolemodules.RoleID}&moduleid=${this.newrolemodules.ModuleID}`
                );

                if (checkResponse.data.exists) {
                    toast.error("Role and Module already exist!");
                    return;
                }

                // Chuẩn bị dữ liệu gửi lên API
                const newRoleModulePayload = {
                    RoleID: this.newrolemodules.RoleID,
                    ModuleID: this.newrolemodules.ModuleID,
                    CanView: this.newrolemodules.CanView == true,   // Chuyển đổi sang boolean
                    CanCreate: this.newrolemodules.CanCreate == true,
                    CanEdit: this.newrolemodules.CanEdit == true,
                    CanDelete: this.newrolemodules.CanDelete == true
                };

                console.log("Dữ liệu gửi lên API:", newRoleModulePayload);

                // Gửi request POST đến API
                const response = await axios.post(
                    `http://localhost:5260/api/rolemodules/AddRoleModule`,
                    newRoleModulePayload,
                    { headers: { "Content-Type": "application/json" } }
                );

                // Kiểm tra phản hồi từ API
                if (response.status === 200 && response.data.success) {
                    toast.success("Role Module added successfully!");
                    await this.fetchRoleModule();  // Load lại danh sách sau khi thêm

                    // Tìm vị trí user mới bằng email thay vì User_ID
                    const newRoleModuleIndex = this.rolemodules.findIndex(roleModule => roleModule.RoleID === newRoleModulePayload.RoleID);

                    // khác -1 là user đó đã có
                    if (newRoleModuleIndex != -1) {
                        // Xác định trang mới chứa user vừa thêm(công thức tính đúng số trang)
                        this.currentPage = Math.ceil((newRoleModuleIndex + 1) / this.itemsPerPage);
                    }

                    this.modalInstance.hide(); // Đóng modal nếu có
                    this.resetForm(); // Reset form về mặc định
                } else {
                    toast.error(response.data.message || "An error occurred while adding the Role Module!");
                }
            } catch (error) {
                console.error("Error adding Role Module:", error);
                toast.error("An error occurred, please try again!");
            }
        },


        async updateRoleModule() {
            try {
                if (!this.newrolemodules || !this.newrolemodules.RoleID || !this.newrolemodules.ModuleID) {
                    alert("Please enter complete information!");
                    return;
                }

                if (!this.selectedRoleModuleId || this.selectedRoleModuleId == 0) {
                    alert("RoleModule ID not found to update!");
                    return;
                }

                const roleModuleResponse = await axios.get(
                    `http://localhost:5260/api/rolemodules/GetRoleModuleById?RoleModuleID=${this.selectedRoleModuleId}`
                );


                // Kiểm tra cấu trúc dữ liệu
                const roleModuleArray = roleModuleResponse.data?.rolemodule;
                const roleModule = roleModuleArray && roleModuleArray.length > 0 ? roleModuleArray[0] : null;

                console.log(roleModule);

                if (!roleModule || !roleModule.RoleModuleID) {
                    alert("RoleModule not found!");
                    return;
                }


                console.log("✅ RoleModule found:", roleModule);

                const updatePayload = {
                    RoleModuleID: roleModule.RoleModuleID,
                    RoleID: this.newrolemodules.RoleID || roleModule.RoleID,
                    ModuleID: this.newrolemodules.ModuleID || roleModule.ModuleID,
                    CanView: this.newrolemodules.CanView ?? roleModule.CanView,
                    CanCreate: this.newrolemodules.CanCreate ?? roleModule.CanCreate,
                    CanEdit: this.newrolemodules.CanEdit ?? roleModule.CanEdit,
                    CanDelete: this.newrolemodules.CanDelete ?? roleModule.CanDelete
                };

              

                const response = await axios.put(
                    `http://localhost:5260/api/rolemodules/UpdateModuleRole`,
                    updatePayload,
                    { headers: { "Content-Type": "application/json" } }
                );

                if (response.status === 200 && response.data.success) {
                    alert("RoleModule update successful!");
                    this.fetchRoleModule();
                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    alert(response.data.message || "An error occurred while updating RoleModule!");
                }
            } catch (error) {
                alert("An error occurred, please try again!");
            }
        },

        async updateCheckboxRoleModule(rolemodule) {
            try {
                if (!rolemodule || !rolemodule.RoleModuleID) {
                    toast.error("RoleModule ID not found to update!");
                    return;
                }

                // Chỉ cập nhật quyền (checkbox)
                const updatePayload = {
                    RoleModuleID: rolemodule.RoleModuleID,
                    RoleID: rolemodule.RoleID,
                    ModuleID: rolemodule.ModuleID,
                    CanView: rolemodule.CanView ?? false,
                    CanCreate: rolemodule.CanCreate ?? false,
                    CanEdit: rolemodule.CanEdit ?? false,
                    CanDelete: rolemodule.CanDelete ?? false
                };

                // Gửi yêu cầu cập nhật
                const response = await axios.put(
                    `http://localhost:5260/api/rolemodules/UpdateModuleRole`,
                    updatePayload,
                    { headers: { "Content-Type": "application/json" } }
                );

                if (response.status === 200) {
                    toast.success("Role module update successful");
                    this.fetchRoleModule();
                } else {
                    toast.error("An error occurred while updating RoleModule!");
                }
            } catch (error) {
                alert("An error occurred, please try again!");
            }
        },


        resetForm() {
            this.newrolemodules = {  // Khởi tạo đối tượng newUser
                RoleID: "",
                ModuleID: "",
                CanView: "",
                CanCreate: "",
                CanEdit: "",
                CanDelete: ""
            };
        },

        async deleteRoleModule(id) {
            const result = await Swal.fire({
                title: "Are you sure you want to delete?",
                text: "This action cannot be undone!",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Delete",
                cancelButtonText: "Cancel",
                customClass: {
                    confirmButton: "btn-confirm-delete",  // Thêm class tùy chỉnh
                    cancelButton: "btn-cancel"
                },
            });

            if (!result.isConfirmed) return;
            try {
                const response = await axios.delete(`http://localhost:5260/api/rolemodules/DeleteRoleModule?rolemoduleID=${id}`);

                if (response.status === 200 && response.data.success) {
                    toast.success("Delete module role successfully!");
                    await this.fetchRoleModule();

                    // Kiểm tra nếu trang hiện tại không còn vai trò nào, thì quay về trang trước
                    const totalPagesAfterDelete = Math.ceil(this.rolemodules.length / this.itemsPerPage);
                    if (this.currentPage > totalPagesAfterDelete) {
                        this.currentPage = Math.max(1, totalPagesAfterDelete);
                    }
                } else {
                    toast.error(response.data.message || "Cannot delete role!");
                }
            } catch (error) {
                toast.error("An error occurred, please try again!");
            }
        },

        async handleSubmit() {
            if (this.isEditing) {
                await this.updateRoleModule();
            } else {
                await this.addRoleModule();
            }
        },

        filterRoleModule() {
            if (!this.searchQuery) {
                return this.rolemodules;
            }

            return this.rolemodules.filter(rolemodule => {
                const roleName = rolemodule?.RoleName || this.getRoleName(rolemodule.RoleID);
                const moduleName = rolemodule?.ModuleName || this.getModuleName(rolemodule.ModuleID);

                return (
                    roleName?.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
                    moduleName?.toLowerCase().includes(this.searchQuery.toLowerCase())
                );
            });
        },


        goToPage(page) {
            if (page >= 1 && page <= this.totalPages) {
                this.currentPage = page;
            }
        }


    },

    //Lọc danh sách, tính toán giá trị động
    computed: {
        filteredMenuItems() {
            return this.menuItems.filter(item => item.CanView); // Chỉ lấy các mục có CanView === true
        },

        filteredRoleModule() {
            return this.rolemodules.filter(rolemodule => {
                // Lọc theo role nếu có selectedRole
                const matchesRole = this.selectedRole ? rolemodule.RoleID === this.selectedRole : true;

                // Lọc theo từ khóa tìm kiếm
                const roleName = this.getRoleName(rolemodule.RoleID);
                const moduleName = this.getModuleName(rolemodule.ModuleID);
                const matchesSearch = this.searchQuery
                    ? roleName.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
                    moduleName.toLowerCase().includes(this.searchQuery.toLowerCase())
                    : true;

                return matchesRole && matchesSearch;
            });
        },
        // Tính danh sách user cho trang hiện tại
        paginatedRoleModule() {
            const start = (this.currentPage - 1) * this.itemsPerPage;
            return this.filteredRoleModule.slice(start, start + this.itemsPerPage);
        },
        // Tính tổng số trang
        totalPages() {
            return Math.ceil(this.filteredRoleModule.length / this.itemsPerPage);
        },
    },
    //Gọi API, thao tác DOM, đăng ký sự kiện
    mounted() {
        this.fetchRoleModule(); // Gọi API khi component được mount 

        // Kiểm tra liên tục mỗi 10 giây (có thể điều chỉnh thời gian)
        this.interval = setInterval(() => {
            this.fetchRoleModule();
        }, 5000);
    },
    beforeUnmount() {
        // Xóa interval khi component bị hủy để tránh rò rỉ bộ nhớ
        clearInterval(this.interval);
    }
};