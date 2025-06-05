import axios from 'axios';
import UserLayout from "@/components/UserLayout";
import { Modal } from "bootstrap";
import { toast } from "vue3-toastify";
import "vue3-toastify/dist/index.css";

export default {
    components: {
        UserLayout,
    },
    data() {
        return {
            projects: [],
            users: [],
            project: {
                Name: "",
            },
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
                const storedUser = localStorage.getItem("currentUser");
                const currentUser = storedUser ? JSON.parse(storedUser) : null;
                const userId = currentUser?.User_ID;

                if (!userId) {
                    toast.error("Không tìm thấy người dùng đăng nhập!");
                    return;
                }

                // Gọi API chỉ lấy những project mà user tham gia hoặc tạo
                const [usersResponse, projectsResponse] = await Promise.all([
                    axios.get("http://localhost:5260/api/users/GetUsers"),
                    axios.get(`http://localhost:5260/api/projects/GetProjectsByUserId?userId=${userId}`),
                ]);

                this.users = usersResponse.data || [];
                this.projects = projectsResponse.data || [];
            } catch (error) {
                console.error("Lỗi khi tải project theo user:", error);
                toast.error("Không thể tải danh sách dự án!");
            }
        },

        async fetchProjectDetail(projectId) {
            try {
                const response = await axios.get(`http://localhost:5260/api/projects/GetProjectsById?ProjectID=${projectId}`);

                if (response.data && response.data.project && response.data.project.length > 0) {
                    this.project = response.data.project[0]; // ← Fix ở đây
                }
            } catch (error) {
                console.error("Lỗi khi lấy thông tin project:", error);
            }
        },

        openModal(project = null) {
            let currentUser = null;
            try {
                const storedUser = localStorage.getItem("currentUser");
                if (storedUser) {
                    currentUser = JSON.parse(storedUser);
                }
            } catch (e) {
                console.error("Lỗi khi parse currentUser từ localStorage:", e);
            }

            console.log("Người dùng đăng nhập:", currentUser);

            if (project) {
                // Chế độ chỉnh sửa
                this.isEditing = true;
                this.selectedProjectId = project.ProjectID;
                this.newProject = {
                    Name: project.Name,
                    Description: project.Description,
                    CreatedBy: project.CreatedBy,
                    CreatedAt: project.CreatedAt,
                };
            } else {
                // Chế độ thêm mới
                this.isEditing = false;
                this.selectedProjectId = null;

                this.newProject = {
                    Name: "",
                    Description: "",
                    CreatedBy: currentUser?.User_ID || "", // Gán ID người dùng đăng nhập
                    CreatedAt: "",
                };
            }

            this.modalInstance = new Modal(document.getElementById("addProjectModal"));
            this.modalInstance.show();
        },



        async addProject() {
            try {
                const { Name, Description, CreatedBy } = this.newProject;

                // Kiểm tra dữ liệu bắt buộc
                if (!Name || !Description || !CreatedBy) {
                    toast.error("Vui lòng nhập đầy đủ thông tin!");
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
                    toast.error("Tên dự án không được chứa ký tự đặc biệt!");
                    return;
                }

                // Kiểm tra trùng tên dự án
                const checkResponse = await axios.get(
                    `http://localhost:5260/api/projects/CheckProjectExists?name=${encodeURIComponent(Name)}`
                );

                if (checkResponse.data.exists) {
                    toast.error("Tên dự án đã tồn tại, vui lòng nhập tên khác!");
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
                    toast.success("Thêm dự án thành công!");
                    await this.fetchProjects(); // Gọi lại hàm load danh sách project

                    // Xác định vị trí dự án mới và set lại trang hiện tại nếu cần
                    const newIndex = this.projects.findIndex(p => p.Name === newProjectPayload.Name);
                    if (newIndex !== -1) {
                        this.currentPage = Math.ceil((newIndex + 1) / this.itemsPerPage);
                    }

                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    toast.error(response.data.message || "Đã xảy ra lỗi khi thêm dự án!");
                }
            } catch (error) {
                console.error("Lỗi khi thêm dự án:", error);
                toast.error("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        },

        formatDate(dateStr) {
            const date = new Date(dateStr);
            return date.toLocaleDateString("en-GB"); // → dd/mm/yyyy
        },

        getUserName(User_ID) {
            if (!this.users || this.users.length === 0) return "N/A"; // Kiểm tra roles có dữ liệu không
            const user = this.users.find(r => r.User_ID === User_ID);
            return user ? user.FullName : "N/A";
        },

        resetForm() {
            this.newProject = {  // Khởi tạo đối tượng newUser
                Name: "",
                Description: "",
                CreatedBy: "",
                CreatedAt: "",
            };
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
    mounted() {
        const projectId = this.$route.query.projectId;
        if (projectId) {
            this.fetchProjectDetail(projectId);
        }
        this.fetchProjects();
    },

};
