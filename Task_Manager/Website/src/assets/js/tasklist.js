import AdminLayout from "@/components/AdminLayout.vue";
import axios from "axios"; // Import axios

export default {
    components: {
        AdminLayout,
    },
    name: "TaskList",
    data() {
        return {
            tasks: [], // Danh sách roles
            projects: [],
            users: [],
            statuses: [],
            projectissuetypes: [],
            modalInstance: null,
            newTask: {  // Khởi tạo đối tượng newUser
                TaskID: "",
                Title: "",
                Description: "",
                ProjectID: "",
                AssignedTo: "",
                StatusID: "",
                DueDate: "",
                ParentTaskID: "",
                CreatedAt: "",
                ProjectIssueTypeID: "",
            },
            isEditing: false,  // Biến xác định chế độ chỉnh sửa
            selectedTaskId: null,  // Lưu ID người dùng đang chỉnh sửa
            searchQuery: "",
            currentPage: 1, // Trang hiện tại
            itemsPerPage: 8, // Số user mỗi trang
        };
    },
    methods: {
        async fetchTasks() {
            try {
                // Gọi API users và roles cùng lúc
                const [tasksResponse, projectResponse, usersResponse, statusesResponse, projectissuetypeReponse] = await Promise.all([
                    axios.get("http://localhost:5260/api/tasks/GetTasks"),
                    axios.get("http://localhost:5260/api/projects/GetProjects"),
                    axios.get("http://localhost:5260/api/users/GetUsers"),
                    axios.get("http://localhost:5260/api/taskstatus/GetTaskStatus"),
                    axios.get("http://localhost:5260/api/Project_Issue_Types/GetProject_Issue_Types")
                ]);

                // Kiểm tra dữ liệu có tồn tại không trước khi gán
                this.tasks = tasksResponse.data || [];
                this.projects = projectResponse.data || [];
                this.users = usersResponse.data || [];
                this.statuses = statusesResponse.data || [];
                this.projectissuetypes = projectissuetypeReponse.data || [];
            } catch (error) {
                console.error("Error fetching data:", error);
            }
        },
        async fetchTasksByProjectAndUser() {
            try {
                const [tasksResponse, usersResponse, statusResponse] = await Promise.all([
                    axios.get(`http://localhost:5260/api/tasks/GetTasksByProjectAndUser`, {
                        params: {
                            projectId: this.projectId,
                            userId: this.userId
                        }
                    }),
                    axios.get("http://localhost:5260/api/users/GetUsers"),
                    axios.get("http://localhost:5260/api/taskstatus/GetTaskStatus")
                ]);

                this.tasks = tasksResponse.data || [];
                this.users = usersResponse.data || [];
                this.statuses = statusResponse.data || [];
            } catch (error) {
                console.error('Failed to fetch tasks, users or statuses:', error);
            }
        },
        getUserName(userId) {
            const user = this.users.find(u => u.User_ID === userId);
            return user ? user.FullName : "Unknown";
        },
        getStatusName(statusId) {
            const status = this.statuses.find(s => s.StatusID === statusId);
            return status ? status.Name : "Unknown";
        },


        filterTasks() {
            if (!this.searchQuery) {
                return this.tasks;
            }
            return this.tasks.filter(task =>
                task.Title.toLowerCase().includes(this.searchQuery.toLowerCase())
            );
        },

        goToPage(page) {
            if (page >= 1 && page <= this.totalPages) {
                this.currentPage = page;
            }
        }

    },
    computed: {
        filteredTasks() {
            return this.filterTasks();
        },
        // Tính danh sách user cho trang hiện tại
        paginatedTasks() {
            const start = (this.currentPage - 1) * this.itemsPerPage;
            return this.filteredTasks.slice(start, start + this.itemsPerPage);
        },
        // Tính tổng số trang
        totalPages() {
            return Math.ceil(this.filteredTasks.length / this.itemsPerPage);
        }

    },
    mounted() {
        this.projectId = this.$route.query.projectId;
        this.userId = JSON.parse(localStorage.getItem("currentUser"))?.User_ID;
        this.fetchTasksByProjectAndUser();
    },
}