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
    name: "ProjectPage",
    data() {
        return {
            projects: [], // Danh sách roles
            users: [],
            modalInstance: null,
            newProject: {  // Khởi tạo đối tượng newUser
                Name: "",
                Description: "",
                CreatedBy: "",
                CreatedAt: "",
            },
            isEditing: false,  // Biến xác định chế độ chỉnh sửa
            selectedProjectId: null,  // Lưu ID người dùng đang chỉnh sửa
            searchQuery: "",
            currentPage: 1, // Trang hiện tại
            itemsPerPage: 5, // Số user mỗi trang
        };
    },
    methods: {
        async fetchProjects() {
            try {
                const [usersResponse, projectsResponse] = await Promise.all([
                    axios.get("http://localhost:5260/api/users/GetUsers"),
                    axios.get("http://localhost:5260/api/projects/GetProjects"),
                ]);

                this.users = usersResponse.data || [];
                this.projects = projectsResponse.data || [];
            } catch (error) {
                console.error("Error fetching data:", error);
            }
        },

        getStatusClass(status) {
            return status === "Active" ? "badge bg-success" : "badge bg-danger";
        },

        openModal(project = null) {
            let currentUser = null;
            const storedUser = localStorage.getItem("currentUser");
        
            if (storedUser && storedUser !== "undefined") {
                try {
                    currentUser = JSON.parse(storedUser);
                } catch (e) {
                    console.error("Lỗi khi parse currentUser từ localStorage:", e);
                }
            } else {
                console.warn("Không có currentUser trong localStorage!");
            }
        
            console.log("Người dùng đăng nhập:", currentUser);
        
            if (project) {
                this.isEditing = true;
                this.selectedProjectId = project.ProjectID;
                this.newProject = {
                    Name: project.Name,
                    Description: project.Description,
                    CreatedBy: project.CreatedBy,
                    CreatedAt: project.CreatedAt,
                };
            } else {
                this.isEditing = false;
                this.selectedProjectId = null;
                this.newProject = {
                    Name: "",
                    Description: "",
                    CreatedBy: currentUser?.User_ID || "", // lấy ID người dùng đăng nhập
                    CreatedAt: "",
                };
            }
        
            this.modalInstance = new Modal(document.getElementById("addProjectModal"));
            this.modalInstance.show();
        }
        ,

        formatDate(dateString) {
            return new Date(dateString).toLocaleDateString("vi-VN");
        },

        getUserName(User_ID) {
            if (!this.users || this.users.length === 0) return "N/A"; // Kiểm tra roles có dữ liệu không
            const user = this.users.find(r => r.User_ID === User_ID);
            return user ? user.FullName : "N/A";
        },

        async addProject() {
            try {
                const { Name, Description, CreatedBy } = this.newProject;

                // Kiểm tra dữ liệu bắt buộc
                if (!Name || !Description || !CreatedBy) {
                    toast.error("Please enter complete information!");
                    return;
                }

                // Kiểm tra độ dài tên dự án (không quá 30 ký tự)
                if (Name.length > 30) {
                    toast.error("Tên dự án không được quá 30 ký tự!");
                    return;
                }

                // Kiểm tra ký tự đặc biệt trong tên dự án
                const specialCharRegex = /[^a-zA-Z0-9 ]/;
                if (specialCharRegex.test(Name)) {
                    toast.error("Project name cannot contain special characters!");
                    return;
                }

                // Kiểm tra trùng tên dự án
                const checkResponse = await axios.get(
                    `http://localhost:5260/api/projects/CheckProjectExists?name=${encodeURIComponent(Name)}`
                );

                if (checkResponse.data.exists) {
                    toast.error("Project name already exists, please enter another name!");
                    return;
                }

                const newProjectPayload = {
                    Name: Name.trim(),
                    Description: Description.trim(),
                    CreatedBy: CreatedBy.trim(),
                    CreatedAt: new Date().toISOString()
                };

                console.log("Dữ liệu gửi lên API:", newProjectPayload);

                const response = await axios.post(
                    `http://localhost:5260/api/projects/AddProjects`,
                    newProjectPayload,
                    { headers: { "Content-Type": "application/json" } }
                );

                if (response.status === 200 && response.data.success) {
                    toast.success("Add project successfully!");
                    await this.fetchProjects(); // Gọi lại hàm load danh sách project

                    // Xác định vị trí dự án mới và set lại trang hiện tại nếu cần
                    const newIndex = this.projects.findIndex(p => p.Name === newProjectPayload.Name);
                    if (newIndex !== -1) {
                        this.currentPage = Math.ceil((newIndex + 1) / this.itemsPerPage);
                    }

                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    toast.error(response.data.message || "An error occurred while adding the project!");
                }
            } catch (error) {
                console.error("Error adding project:", error);
                toast.error("An error occurred, please try again!");
            }
        },

        async updateProject() {
            try {
                if (!this.newProject.Name || !this.newProject.Description || !this.newProject.CreatedBy) {
                    toast.error("Please enter complete information!");
                    return;
                }

                if (this.newProject.Name.length > 30) {
                    toast.error("Project name cannot exceed 30 characters!");
                    return;
                }

                const specialCharRegex = /[^a-zA-Z0-9 ]/;
                if (specialCharRegex.test(this.newProject.Name)) {
                    toast.error("Project name cannot contain special characters!");
                    return;
                }

                // Kiểm tra trùng tên
                const checkResponse = await axios.get(
                    `http://localhost:5260/api/projects/CheckProjectExists?name=${encodeURIComponent(this.newProject.Name)}&excludeId=${this.selectedProjectId}`
                );
                if (checkResponse.data.exists) {
                    toast.error("Project name already exists, please enter another name!");
                    return;
                }

                // Lấy thông tin dự án hiện tại
                const projectResponse = await axios.get(
                    `http://localhost:5260/api/projects/GetProjectsById?ProjectID=${this.selectedProjectId}`
                );

                const projectData = projectResponse.data.project;

                let project = null;

                if (Array.isArray(projectData) && projectData.length > 0) {
                    project = projectData[0];
                } else if (typeof projectData === "object" && projectData !== null) {
                    project = projectData;
                }

                if (!project || !project.ProjectID) {
                    toast.error("No project found!");
                    return;
                }

                const updatePayload = {
                    ProjectID: project.ProjectID,
                    Name: this.newProject.Name.trim(),
                    Description: this.newProject.Description.trim(),
                    CreatedBy: this.newProject.CreatedBy.trim(),
                    CreatedAt: project.CreatedAt // giữ nguyên nếu cần
                };

                console.log("Dữ liệu gửi lên API:", updatePayload);

                const response = await axios.put(
                    `http://localhost:5260/api/projects/UpdateProjects?ProjectID=${project.ProjectID}`,
                    updatePayload,
                    { headers: { "Content-Type": "application/json" } }
                );

                if (response.status === 200 && response.data.success) {
                    toast.success("Project update successful!");
                    this.fetchProjects();
                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    toast.error(response.data.message || "An error occurred while updating the project.!");
                }
            } catch (error) {
                console.error("Error while updating project:", error);
                toast.error("An error occurred, please try again!");
            }
        },



        resetForm() {
            this.newProject = {  // Khởi tạo đối tượng newUser
                Name: "",
                Description: "",
                CreatedBy: "",
                CreatedAt: "",
            };
        },

        async deleteProject(id) {
            const result = await Swal.fire({
                title: "Are you sure you want to delete this project??",
                text: "This action cannot be undone.!",
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
                const response = await axios.delete(`http://localhost:5260/api/projects/DeleteProjects?projectID=${id}`);

                console.log("response:", response);

                if (response.status === 200 && response.data.success) {
                    toast.success("Project deletion successful!");
                    await this.fetchProjects(); // Tải lại danh sách dự án

                    // Điều chỉnh phân trang nếu cần
                    const totalPagesAfterDelete = Math.ceil(this.projects.length / this.itemsPerPage);
                    if (this.currentPage > totalPagesAfterDelete) {
                        this.currentPage = Math.max(1, totalPagesAfterDelete);
                    }
                } else {
                    toast.error(response.data.message || "Unable to delete project!");
                }
            } catch (error) {
                console.error("Error while deleting project:", error);
                toast.error("An error occurred, please try again!");
            }
        },


        async handleSubmit() {
            if (this.isEditing) {
                await this.updateProject();
            } else {
                await this.addProject();
            }
        },

        filterProjects() {
            if (!this.searchQuery) {
                return this.projects;
            }
            return this.projects.filter(project =>
                project.Name.toLowerCase().includes(this.searchQuery.toLowerCase())
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
        filteredProjects() {
            return this.filterProjects();
        },

        // Tính danh sách user cho trang hiện tại
        paginatedProjects() {
            const start = (this.currentPage - 1) * this.itemsPerPage;
            return this.filteredProjects.slice(start, start + this.itemsPerPage);
        },

        // Tính tổng số trang
        totalPages() {
            return Math.ceil(this.filteredProjects.length / this.itemsPerPage);
        }
    },
    //Gọi API, thao tác DOM, đăng ký sự kiện
    mounted() {
        this.fetchProjects(); // Gọi API khi component được mount
    },
};