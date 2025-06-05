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
    name: "StatusPage",
    data() {
        return {
            statuses: [], // Danh sách roles
            modalInstance: null,
            newStatus: {  // Khởi tạo đối tượng newUser
                StatusID: "",
                Name: "",
                ColorCode: "",
            },
            isEditing: false,  // Biến xác định chế độ chỉnh sửa
            selectedStatusId: null,  // Lưu ID người dùng đang chỉnh sửa
            searchQuery: "",
            currentPage: 1, // Trang hiện tại
            itemsPerPage: 5, // Số user mỗi trang
        };
    },
    methods: {
        async fetchStatus() {
            try {
                const statusResponse = await axios.get("http://localhost:5260/api/taskstatus/GetTaskStatus");

               
                // Kiểm tra dữ liệu có tồn tại không trước khi gán
                this.statuses = statusResponse.data || [];
            } catch (error) {
                console.error("Error fetching data:", error);
            }
        },

        getStatusClass(status) {
            return status === "Active" ? "badge bg-success" : "badge bg-danger";
        },

        openModal(status = null) {
            if (status) {
                // Chế độ chỉnh sửa
                this.isEditing = true;
                this.selectedStatusId = status.StatusID;
                this.newStatus = {
                    Name: status.Name,
                    ColorCode: status.ColorCode,
                };
            } else {
                // Chế độ thêm mới
                this.isEditing = false;
                this.selectedStatusId = null;
                this.resetForm();
            }

            this.modalInstance = new Modal(document.getElementById("addStatusModal"));
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

        async addStatus() {
            try {
                // Kiểm tra thông tin bắt buộc
                if (!this.newStatus.Name || !this.newStatus.ColorCode) {
                    toast.error("Vui lòng nhập đầy đủ thông tin!");
                    return;
                }
        
                // Giới hạn độ dài tên
                if (this.newStatus.Name.length > 20) {
                    toast.error("Tên trạng thái không được quá 20 ký tự!");
                    return;
                }
        
                // Kiểm tra ký tự đặc biệt
                const specialCharRegex = /[^a-zA-Z0-9 ]/;
                if (specialCharRegex.test(this.newStatus.Name)) {
                    toast.error("Tên trạng thái không được chứa ký tự đặc biệt!");
                    return;
                }
        
                // Kiểm tra trạng thái đã tồn tại chưa (giả sử Check dựa theo Name)
                const checkResponse = await axios.get(
                    `http://localhost:5260/api/taskstatus/CheckTaskStatusExists?name=${encodeURIComponent(this.newStatus.Name)}`
                );
        
                if (checkResponse.data.exists) {
                    toast.error("Trạng thái đã tồn tại, vui lòng chọn tên khác!");
                    return;
                }
        
                // Tạo payload
                const newStatusPayload = {
                    Name: this.newStatus.Name.trim(),
                    ColorCode: this.newStatus.ColorCode
                };
        
                console.log("Dữ liệu gửi lên API:", newStatusPayload);
        
                // Gửi request thêm mới
                const response = await axios.post(
                    `http://localhost:5260/api/taskstatus/AddTaskStatus`,
                    newStatusPayload,
                    { headers: { "Content-Type": "application/json" } }
                );
        
                if (response.status === 200 && response.data.success) {
                    toast.success("Thêm trạng thái thành công!");
                    await this.fetchStatus();
        
                    // Xác định vị trí và chuyển đến trang tương ứng nếu có phân trang
                    const newIndex = this.statuses.findIndex(s => s.Name === newStatusPayload.Name);
                    if (newIndex !== -1) {
                        this.currentPage = Math.ceil((newIndex + 1) / this.itemsPerPage);
                    }
        
                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    toast.error(response.data.message || "Đã xảy ra lỗi khi thêm trạng thái!");
                }
            } catch (error) {
                console.error("Lỗi khi thêm trạng thái:", error);
                toast.error("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },
        

        async updateStatus() {
            try {
                if (!this.newStatus.Name || !this.newStatus.ColorCode) {
                    toast.error("Vui lòng nhập đầy đủ thông tin!");
                    return;
                }
        
                // Kiểm tra độ dài tên (không quá 15 ký tự)
                if (this.newStatus.Name.length > 15) {
                    toast.error("Tên trạng thái không được quá 15 ký tự!");
                    return;
                }
        
                // Kiểm tra trùng tên (nếu tên thay đổi)
                const checkResponse = await axios.get(
                    `http://localhost:5260/api/taskstatus/CheckTaskStatusExists?name=${encodeURIComponent(this.newStatus.Name)}&excludeId=${this.selectedStatusId}`
                );
        
                if (checkResponse.data.exists) {
                    toast.error("Trạng thái đã tồn tại, vui lòng chọn tên khác!");
                    return;
                }
        
                // Lấy thông tin trạng thái hiện tại từ API
                const statusResponse = await axios.get(
                    `http://localhost:5260/api/taskstatus/GetTaskStatusById?StatusID=${this.selectedStatusId}`
                );
        
                if (!statusResponse.data.success || !statusResponse.data.status.length) {
                    toast.error("Không tìm thấy trạng thái!");
                    return;
                }
        
                const status = statusResponse.data.status[0];
                const statusId = status.StatusID;
        
                const updatePayload = {
                    StatusID: statusId,
                    Name: this.newStatus.Name.trim(),
                    ColorCode: this.newStatus.ColorCode.trim()
                };
        
                console.log("Dữ liệu gửi lên API:", updatePayload);
        
                const response = await axios.put(
                    `http://localhost:5260/api/taskstatus/UpdateTaskStatus?TaskStatusID=${this.selectedStatusId}`,
                    updatePayload,
                    { headers: { "Content-Type": "application/json" } }
                );
        
                if (response.status === 200 && response.data.success) {
                    toast.success("Cập nhật trạng thái thành công!");
                    this.fetchStatus(); // Hàm load lại danh sách trạng thái
                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    toast.error(response.data.message || "Đã xảy ra lỗi khi cập nhật trạng thái!");
                }
            } catch (error) {
                console.error("Lỗi khi cập nhật trạng thái:", error);
                toast.error("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },
        


        resetForm() {
            this.newStatus = {
                Name: "", // Reset về giá trị mặc định
                ColorCode: ""
            };
        },

        async deleteStatus(id) {
            const result = await Swal.fire({
                title: "Bạn có chắc chắn muốn xóa không?",
                text: "Hành động này không thể hoàn tác!",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Xóa",
                cancelButtonText: "Hủy",
                customClass: {
                    confirmButton: "btn-confirm-delete",
                    cancelButton: "btn-cancel"
                },
            });
        
            if (!result.isConfirmed) return;
        
            try {
                const response = await axios.delete(`http://localhost:5260/api/taskstatus/DeleteTaskStatus?TaskStatusID=${id}`);
        
                console.log("response: ", response);
                if (response.status === 200 && response.data.success) {
                    toast.success("Xóa trạng thái thành công!");
                    await this.fetchStatus(); // Gọi lại danh sách trạng thái
        
                    // Kiểm tra nếu trang hiện tại không còn trạng thái nào, thì quay về trang trước
                    const totalPagesAfterDelete = Math.ceil(this.statuses.length / this.itemsPerPage);
                    if (this.currentPage > totalPagesAfterDelete) {
                        this.currentPage = Math.max(1, totalPagesAfterDelete);
                    }
                } else {
                    toast.error(response.data.message || "Không thể xóa trạng thái!");
                }
            } catch (error) {
                console.error("Lỗi khi xóa trạng thái:", error);
                toast.error("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },
        

        async handleSubmit() {
            if (this.isEditing) {
                await this.updateStatus();
            } else {
                await this.addStatus();
            }
        },

        filterStatuses() {
            if (!this.searchQuery) {
                return this.statuses;
            }
            return this.statuses.filter(status =>
                status.Name.toLowerCase().includes(this.searchQuery.toLowerCase())
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
        filteredStatuses() {
            return this.filterStatuses();
        },

        // Tính danh sách user cho trang hiện tại
        paginatedStatuses() {
            const start = (this.currentPage - 1) * this.itemsPerPage;
            return this.filteredStatuses.slice(start, start + this.itemsPerPage);
        },

        // Tính tổng số trang
        totalPages() {
            return Math.ceil(this.filteredStatuses.length / this.itemsPerPage);
        }
    },
    //Gọi API, thao tác DOM, đăng ký sự kiện
    mounted() {
        this.fetchStatus(); // Gọi API khi component được mount
    },
};