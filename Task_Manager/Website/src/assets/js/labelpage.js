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
    name: "LabelPage",
    data() {
        return {
            users: [], // Danh sách người dùng
            labels: [], // Danh sách roles
            modalInstance: null,
            newLabel: {  // Khởi tạo đối tượng newUser
                Name: "",
                ColorCode: "",
                Description: "",
                IsActive: "",
                CreatedBy: "",
            },
            isEditing: false,  // Biến xác định chế độ chỉnh sửa
            selectedLabelId: null,  // Lưu ID người dùng đang chỉnh sửa
            searchQuery: "",
            currentPage: 1, // Trang hiện tại
            itemsPerPage: 5, // Số user mỗi trang
        };
    },
    methods: {
        async fetchLabels() {
            try {
                // Gọi API users và roles cùng lúc
                const [usersResponse, labelsResponse] = await Promise.all([
                    axios.get("http://localhost:5260/api/users/GetUsers"),
                    axios.get("http://localhost:5260/api/labels/GetLabels"),
                ]);

                // Kiểm tra dữ liệu có tồn tại không trước khi gán
                this.users = usersResponse.data || [];
                this.labels = labelsResponse.data || [];
            } catch (error) {
                console.error("Error fetching data:", error);
            }
        },
        openModal(label = null) {
            
            if (label) {
                // Chế độ chỉnh sửa
                this.isEditing = true;
                this.selectedLabelId = label.LabelID;
                this.newLabel = {
                    Name: label.Name,
                    ColorCode: label.ColorCode,
                    Description: label.Description,
                    IsActive: label.IsActive,
                    CreatedBy: label.CreatedBy
                };
            } else {
                // Chế độ thêm mới
                this.isEditing = false;
                this.selectedLabelId = null;
                this.resetForm(); // reset newLabel về mặc định
            }

            this.modalInstance = new Modal(document.getElementById("addLabelModal")); // sửa đúng ID modal
            this.modalInstance.show();
        }

        ,
        formatDate(dateString) {
            return new Date(dateString).toLocaleDateString("vi-VN");
        },

        getUserName(userId) {
            if (!this.users || this.users.length === 0) return "N/A"; // Kiểm tra roles có dữ liệu không
            const user = this.users.find(r => r.User_ID === userId);
            return user ? user.FullName : "N/A";
        },

        async addLabel() {
            try {
                // Kiểm tra dữ liệu bắt buộc
                if (!this.newLabel.Name || !this.newLabel.ColorCode || !this.newLabel.CreatedBy) {
                    toast.error("Please fill in all required information (Name, Color, CreatedBy)!");
                    return;
                }

                // Giới hạn độ dài nếu cần
                if (this.newLabel.Name.length > 50) {
                    toast.error("Label name is too long, maximum 50 characters!");
                    return;
                }

                // Chuẩn bị payload
                const labelPayload = {
                    name: this.newLabel.Name.trim(),
                    colorCode: this.newLabel.ColorCode,
                    description: this.newLabel.Description?.trim() || "",
                    isActive: this.newLabel.IsActive === "true" || this.newLabel.IsActive === true,
                    CreatedBy: this.newLabel.CreatedBy
                };

                // Gọi API thêm label
                const response = await axios.post(
                    "http://localhost:5260/api/labels/AddLabels",
                    labelPayload,
                    { headers: { "Content-Type": "application/json" } }
                );

                if (response.status === 200 && response.data.success) {
                    toast.success("Added label successfully!");
                    await this.fetchLabels(); // Giả sử bạn có phương thức để load lại danh sách

                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    toast.error(response.data.message || "An error occurred while adding the label!");
                }
            } catch (error) {
                console.error("Error:", error);
                toast.error("An error occurred, please try again!");
            }
        },


        async updateLabel() {
            try {
                if (!this.newLabel.Name || !this.newLabel.ColorCode || !this.newLabel.CreatedBy) {
                    toast.error("Please fill in all required information (Name, Color, CreatedBy)!");
                    return;
                }
        
                if (this.newLabel.Name.length > 50) {
                    toast.error("Label name is too long, maximum 50 characters!");
                    return;
                }
        
                const updatePayload = {
                    labelID: this.selectedLabelId,
                    name: this.newLabel.Name.trim(),
                    colorCode: this.newLabel.ColorCode,
                    description: this.newLabel.Description?.trim() || "",
                    isActive: this.newLabel.IsActive === "true" || this.newLabel.IsActive === true,
                    createdBy: this.newLabel.CreatedBy
                };
        
        
                const response = await axios.put(
                    `http://localhost:5260/api/labels/UpdateLabels?LabelID=${this.selectedLabelId}`,
                    updatePayload,
                    { headers: { "Content-Type": "application/json" } }
                );
        
                if (response.status === 200 && response.data.success) {
                    toast.success("Label updated successfully!");
                    await this.fetchLabels();
                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    toast.error(response.data.message || "An error occurred while updating the label!");
                }
            } catch (error) {
                console.error("Error updating label:", error);
                toast.error("An error occurred, please try again!");
            }
        },
        
        resetForm() {
            this.newLabel = {
                Name: "",
                ColorCode: "#000000", // ✅ Màu mặc định hợp lệ
                Description: "",
                IsActive: "true",
                CreatedBy: "",// ✅ Gán user đầu tiên nếu có
            };
        },


        async deleteLabel(labelId) {
            const result = await Swal.fire({
                title: "Are you sure you want to delete this label??",
                text: "This action cannot be undone!",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Delete",
                cancelButtonText: "Cancel",
                customClass: {
                    confirmButton: "btn-confirm-delete",
                    cancelButton: "btn-cancel"
                }
            });
        
            if (!result.isConfirmed) return;
        
            try {
                console.log(labelId);
                const response = await axios.delete("http://localhost:5260/api/labels/DeleteLabels", {
                    params: { labelID: labelId },
                    headers: { "Content-Type": "application/json" }
                });
        
                if (response.status === 200 && response.data.success) {
                    toast.success("Delete label successfully!");
                    await this.fetchLabels();
        
                    // Cập nhật phân trang nếu cần (tùy bạn có dùng phân trang không)
                    const totalPagesAfterDelete = Math.ceil(this.labels.length / this.itemsPerPage);
                    if (this.currentPage > totalPagesAfterDelete) {
                        this.currentPage = Math.max(1, totalPagesAfterDelete);
                    }
                } else {
                    toast.error(response.data.message || "Cannot delete label!");
                }
            } catch (error) {
                console.error("Error:", error);
                toast.error("An error occurred, please try again!");
            }
        },
        


        async handleSubmit() {
            if (this.isEditing) {
                await this.updateLabel();
            } else {
                await this.addLabel();
            }
        },

        filterLabels() {
            if (!this.searchQuery) {
                return this.labels;
            }
            return this.labels.filter(label =>
                label.Name.toLowerCase().includes(this.searchQuery.toLowerCase())
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
        filteredLabels() {
            return this.filterLabels();
        },
        // Tính danh sách user cho trang hiện tại
        paginatedLabels() {
            const start = (this.currentPage - 1) * this.itemsPerPage;
            return this.filteredLabels.slice(start, start + this.itemsPerPage);
        },
        // Tính tổng số trang
        //Hàm Math.ceil() làm tròn lên
        totalPages() {
            return Math.ceil(this.filteredLabels.length / this.itemsPerPage);
        }
    },
    //Gọi API, thao tác DOM, đăng ký sự kiện
    //Nếu fetchUsers() thực hiện một API call để lấy 
    // danh sách người dùng, thì component sẽ nhận dữ liệu và cập nhật giao diện.
    //khi component thêm vào dom thì mounted sẽ dc gọi 
    mounted() {
        this.fetchLabels(); // Gọi API khi component được mount
    },
};