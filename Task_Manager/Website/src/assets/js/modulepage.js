import AdminLayout from "@/components/AdminLayout.vue";
import { Modal } from "bootstrap";
import axios from "axios"; // Import axios

export default {
    components: {
        AdminLayout,
    },
    name: "ModulePage",
    data() {
        return {
            modules: [], // Danh sách roles
            modalInstance: null,
            newModule: {  // Khởi tạo đối tượng newUser
                ModuleID: "",
                ModuleName: "",
                ParentID: "",
                Link: "",
                Icon: "",
                OrderNumber: "",
            },
            isEditing: false,  // Biến xác định chế độ chỉnh sửa
            selectedModuleId: null,  // Lưu ID người dùng đang chỉnh sửa
            searchQuery: "",
            currentPage: 1, // Trang hiện tại
            itemsPerPage: 5, // Số user mỗi trang
        };
    },
    methods: {
        async fetchModules() {
            try {
                console.log('được gọi r');
                // Gọi API users và roles cùng lúc
                const modulesResponse = await axios.get("http://localhost:5260/api/modules/GetModules");

                // Kiểm tra dữ liệu có tồn tại không trước khi gán
                this.modules = modulesResponse.data || [];
            } catch (error) {
                console.error("Error fetching data:", error);
            }
        },
        getStatusClass(status) {
            return status === "Active" ? "badge bg-success" : "badge bg-danger";
        },

        openModal(module = null) {
            if (module) {
                // Chế độ chỉnh sửa
                this.isEditing = true;
                this.selectedModuleId = module.ModuleID;
                this.newModule = {
                    ModuleName: module.ModuleName,
                    ParentID: module.ParentID,
                    Link: module.Link,
                    Icon: module.Icon,
                    OrderNumber: module.OrderNumber,
                    CreatedAt: module.CreatedAt,
                    UpdatedAt: module.UpdatedAt,
                };
            } else {
                // Chế độ thêm mới
                this.isEditing = false;
                this.selectedModuleId = null;
                this.resetForm();
            }

            this.modalInstance = new Modal(document.getElementById("addModuleModal"));
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

        async addModule() {
            try {
                // Kiểm tra dữ liệu trước khi gửi
                if (!this.newModule.ModuleName?.trim() || !this.newModule.Link?.trim() ||
                    !this.newModule.Icon?.trim() ||
                    this.newModule.OrderNumber === null || this.newModule.OrderNumber === undefined) {
                    alert("Vui lòng nhập đầy đủ thông tin!");
                    return;
                }

                // Đảm bảo ParentID là null nếu không có giá trị
                const newModulePayload = {
                    ModuleName: this.newModule.ModuleName.trim(),
                    ParentID: this.newModule.ParentID ? parseInt(this.newModule.ParentID) : null,
                    Link: this.newModule.Link.trim(),
                    Icon: this.newModule.Icon.trim(),
                    OrderNumber: parseInt(this.newModule.OrderNumber), // Chắc chắn là số
                    CreatedAt: new Date().toISOString() // Lấy ngày tạo hiện tại
                };

                console.log("Dữ liệu gửi lên API:", newModulePayload);

                const response = await axios.post(
                    "http://localhost:5260/api/modules/AddModule",
                    newModulePayload,
                    { headers: { "Content-Type": "multipart/form-data" } }
                );

                if (response.status === 200 && response.data.success) {
                    alert("Thêm module thành công!");
                    this.fetchModules(); // Load lại danh sách modules
                    this.modalInstance.hide(); // Ẩn modal
                    this.resetForm(); // Xóa form sau khi thêm
                } else {
                    alert(response.data.message || "Đã xảy ra lỗi khi thêm module!");
                }
            } catch (error) {
                console.error("Lỗi khi thêm module:", error.response?.data || error.message);
                alert("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },

        async updateModule() {
            try {
                if (!this.newModule.ModuleName?.trim() || !this.newModule.Link?.trim() ||
                    !this.newModule.Icon?.trim() || 
                    this.newModule.OrderNumber === null || this.newModule.OrderNumber === undefined) {
                    alert("Vui lòng nhập đầy đủ thông tin!");
                    return;
                }

                // Lấy User_ID từ API dựa vào email
                const moduleResponse = await axios.get(`http://localhost:5260/api/modules/GetModuleById?ModuleID=${this.selectedModuleId}`);
                console.log("Phản hồi từ API GetModuleById:", moduleResponse.data);
                if (!moduleResponse.data.success || !moduleResponse.data.module.length) {
                    alert("Không tìm thấy module!");
                    return;
                }

                const module = moduleResponse.data.module[0]; // Lấy thông tin user đầu tiên
                if (!module?.ModuleID) {
                    alert("Module không hợp lệ!");
                    return;
                }

                const moduleId = module.ModuleID; 
                console.log('newModule: ', moduleId);
                const updatePayload = {
                    ModuleID: moduleId,
                    ModuleName: this.newModule.ModuleName.trim(),
                    ParentID: this.newModule.ParentID ? parseInt(this.newModule.ParentID) : null,
                    Link: this.newModule.Link.trim(),
                    Icon: this.newModule.Icon.trim(),
                    OrderNumber: parseInt(this.newModule.OrderNumber), // Chắc chắn là số
                    UpdatedAt: new Date().toISOString(), // Lấy ngày tạo hiện tại
                };

                console.log("Dữ liệu gửi lên API:", updatePayload);

                const response = await axios.put(
                    `http://localhost:5260/api/modules/UpdateModule?ModuleID=${this.selectedModuleId}`,
                    updatePayload,
                    { headers: { "Content-Type": "multipart/form-data" } }
                );

                if (response.status === 200 && response.data.success) {
                    alert("Cập nhật module thành công!");
                    this.fetchModules();
                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    alert(response.data.message || "Đã xảy ra lỗi khi cập nhật module!");
                }
            } catch (error) {
                console.error("Lỗi khi cập nhật module:", error);
                alert("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },

        resetForm() {
            this.newModule = {
                ModuleName: "", // Reset về giá trị mặc định
                ParentID: "",
                Link: "",
                Icon: "",
                OrderNumber: "", // Chắc chắn là số
                UpdatedAt: "", // Lấy ngày tạo hiện tại
            };
        },

        async deleteModule(id) {
            if (!id || id <= 0) {
                alert("ModuleID không hợp lệ!");
                return;
            }
            if (!confirm("Bạn có chắc chắn muốn xóa module này không?")) return;

            console.log("ModuleID:", id);
            try {
                const response = await axios.delete(`http://localhost:5260/api/modules/DeleteModule?moduleID=${id}`);

                if (response.status === 200 && response.data.success) {
                    alert("Xóa module thành công!");
                    this.fetchModules();
                } else {
                    alert(response.data.message || "Không thể xóa module!");
                }
            } catch (error) {
                console.error("Lỗi khi xóa module:", error);
                alert("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },

        async handleSubmit() {
            if (this.isEditing) {
                await this.updateModule();
            } else {
                await this.addModule();
            }
        },

        filterModules() {
            if (!this.searchQuery) {
                return this.modules;
            }
            return this.modules.filter(module =>
                module.ModuleName.toLowerCase().includes(this.searchQuery.toLowerCase())
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
        filteredModules() {
            return this.filterModules();
        },
        // Tính danh sách user cho trang hiện tại
        paginatedModules() {
            const start = (this.currentPage - 1) * this.itemsPerPage;
            return this.filteredModules.slice(start, start + this.itemsPerPage);
        },
        // Tính tổng số trang
        totalPages() {
            return Math.ceil(this.filteredModules.length / this.itemsPerPage);
        }
    },
    //Gọi API, thao tác DOM, đăng ký sự kiện
    mounted() {
        this.fetchModules(); // Gọi API khi component được mount
    },
};