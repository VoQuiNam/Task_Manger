import axios from 'axios';
import { Modal } from "bootstrap";
import { useRoute } from 'vue-router';
import { toast } from "vue3-toastify";
import Swal from "sweetalert2";

export default {
    data() {
        return {
            issueTypes: [],
            modalInstance: null,
            newIssue: {  // Khởi tạo đối tượng newUser
                Name: "",
            },
            isEditing: false,  // Biến xác định chế độ chỉnh sửa
            selectedTypeId: null,  // Lưu ID người dùng đang chỉnh sửa
            projectId: null,  // <-- Thêm dòng này
        };
    },
    mounted() {
        const route = useRoute();
        this.projectId = route.query.projectId; // <-- Lấy projectId từ URL
        this.fetchIssueTypes();
    },

    methods: {
        async fetchIssueTypes() {
            try {
                 const response = await axios.get(`http://localhost:5260/api/Project_Issue_Types/GetProject_Issue_TypesByProjectId`, {
                params: {
                    projectId: this.projectId
                }
            });
                this.issueTypes = response.data;
            } catch (error) {
                console.error('Failed to fetch issue types:', error);
            }
        },

        openModalIssue(issuetype = null) {

            if (issuetype) {
                // Chế độ chỉnh sửa
                this.isEditing = true;
                this.selectedTypeId = issuetype.TypeID;
                this.newIssue = {
                    Name: issuetype.Name,
                };
            } else {
                // Chế độ thêm mới
                this.isEditing = false;
                this.selectedTypeId = null;
                this.resetForm(); // reset newLabel về mặc định
            }

            this.modalInstance = new Modal(document.getElementById("addIssueModal")); // sửa đúng ID modal
            this.modalInstance.show();
        },

        async addIssue() {
            if (!this.newIssue.Name) {
                toast.error("Please enter the issue type name!");
                return;
            }
        
            if (!this.projectId) {
                toast.error("No project selected!");
                return;
            }
        
            try {
                // Fetch project details trước để lấy project name
                const projectResponse = await axios.get(`http://localhost:5260/api/projects/GetProjectsById?ProjectID=${this.projectId}`);
                const projectData = projectResponse.data;
                const projectName = projectData?.project?.[0]?.Name || "this project";
        
                // Xác nhận trước khi tạo
                if (!confirm(`Do you want to create the issue type and allocate it to project "${projectName}"?`)) {
                    toast.info("Creation canceled.");
                    return; // Nếu cancel thì dừng luôn, không tạo
                }
        
                // Nếu OK thì mới tiến hành tạo IssueType
                console.log("Sending this to AddIssueType API:", this.newIssue);
        
                const addResponse = await axios.post(
                    'http://localhost:5260/api/issue_types/AddIssueType',
                    new URLSearchParams({
                        Name: this.newIssue.Name
                    }),
                    {
                        headers: {
                            'Content-Type': 'application/x-www-form-urlencoded'
                        }
                    }
                );
        
                if (addResponse.data.success) {
                    const createdTypeId = addResponse.data.typeID;
                    console.log('API response:', addResponse.data);
        
                    // Gán issue type vào project
                    const assignResponse = await axios.post('http://localhost:5260/api/Project_Issue_Types/AddProjectIssueType', {
                        ProjectID: this.projectId,
                        TypeID: createdTypeId
                    });
        
                    console.log('Assign response:', assignResponse.data);
        
                    if (assignResponse.data.success) {
                        toast.success("Issue type created and assigned to the project successfully!");
                    } else {
                        toast.error(assignResponse.data.message || "Failed to assign issue type to project.");
                    }
        
                    // Reset form và reload danh sách
                    this.resetForm();
                    this.modalInstance.hide();
                    this.fetchIssueTypes();
        
                } else {
                    toast.error(addResponse.data.message || "Failed to create issue type.");
                }
            } catch (error) {
                console.error("Error adding issue type:", error);
                toast.error("An error occurred while creating the issue type.");
            }
        },
        

        async deleteIssue(id) {
            const result = await Swal.fire({
                title: "Are you sure you want to delete this issue?",
                text: "This action cannot be undone!",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Delete",
                cancelButtonText: "Cancel",
                customClass: {
                    confirmButton: "btn-confirm-delete",
                    cancelButton: "btn-cancel"
                },
            });
        
            if (!result.isConfirmed) return;
        
            try {
                // 🔥 Bước 1: Xóa tất cả project_issue_types có typeID này
                await axios.delete(`http://localhost:5260/api/Project_Issue_Types/DeleteProjectIssueType`, {
                    params: {
                        ProjectID: this.projectId,  // ⚡ lấy projectId ở đâu? bạn cần có this.projectId
                        TypeID: id
                    }
                });
        
                // 🔥 Bước 2: Xóa issue type khỏi bảng Issue_Types
                const response = await axios.delete(`http://localhost:5260/api/issue_types/DeleteIssueTypes?typeID=${id}`);
        
                console.log("response:", response);
        
                if (response.status === 200 && response.data.success) {
                    toast.success("Deleted successfully!");
                    await this.fetchIssueTypes(); // Reload danh sách
                } else {
                    toast.error(response.data.message || "Unable to delete issue type!");
                }
            } catch (error) {
                console.error("Error:", error);
                toast.error("An error occurred, please try again!");
            }
        },
        

        resetForm() {
            this.newIssue = {
                Name: "",
            };
        },

        async handleSubmitIssue() {
            if (this.isEditing) {
                await this.updateLabel();
            } else {
                await this.addIssue();
            }
        },
    }
};
