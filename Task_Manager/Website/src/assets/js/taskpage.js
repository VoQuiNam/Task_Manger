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
    name: "TaskPage",
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
            itemsPerPage: 5, // Số user mỗi trang
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

        getStatusClass(status) {
            return status === "Active" ? "badge bg-success" : "badge bg-danger";
        },

        openModal(task = null) {
            if (task) {
                // Chế độ chỉnh sửa
                this.isEditing = true;
                this.selectedTaskId = task.TaskID;
                this.newTask = {
                    Title: task.Title,
                    Description: task.Description,
                    ProjectID: task.ProjectID,
                    AssignedTo: task.AssignedTo,
                    StatusID: task.StatusID,
                    DueDate: task.DueDate,
                    ParentTaskID: task.ParentTaskID,
                    ProjectIssueTypeID: task.ProjectIssueTypeID,
                };
            } else {
                // Chế độ thêm mới
                this.isEditing = false;
                this.selectedTaskId = null;
                this.resetForm();
            }

            this.modalInstance = new Modal(document.getElementById("addTaskModal"));
            this.modalInstance.show();
        },

        formatDate(dateString) {
            return new Date(dateString).toLocaleDateString("vi-VN");
        },

        getProjectName(projectid) {
            if (!this.projects || this.projects.length === 0) return "N/A"; // Kiểm tra roles có dữ liệu không
            const project = this.projects.find(r => r.ProjectID == projectid);
            return project ? project.Name : "N/A";
        },

        getUserName(User_ID) {
            if (!this.users || this.users.length === 0) return "N/A"; // Kiểm tra roles có dữ liệu không
            const user = this.users.find(r => r.User_ID === User_ID);
            return user ? user.FullName : "N/A";
        },

        getStatus(statusid) {
            if (!this.statuses || this.statuses.length === 0) return "N/A"; // Kiểm tra roles có dữ liệu không
            const status = this.statuses.find(r => r.StatusID == statusid);
            return status ? status.Name : "N/A";
        },

        async addTask() {
            try {
                // Validate required fields
                if (
                    !this.newTask.Title?.trim() ||
                    !this.newTask.Description?.trim() ||
                    !this.newTask.ProjectID ||
                    !this.newTask.AssignedTo ||
                    !this.newTask.StatusID ||
                    !this.newTask.DueDate ||
                    !this.newTask.ProjectIssueTypeID
                ) {
                    toast.error("Please fill in all required fields!");
                    return;
                }

                // Validate Title length
                if (this.newTask.Title.length > 100) {
                    toast.error("Title must not exceed 100 characters!");
                    return;
                }

                // Validate special characters in Title
                const specialCharRegex = /[^a-zA-Z0-9À-ỹ\s.,-]/;
                if (specialCharRegex.test(this.newTask.Title)) {
                    toast.error("Title must not contain special characters!");
                    return;
                }

                // Validate Description length
                if (this.newTask.Description.length > 500) {
                    toast.error("Description must not exceed 500 characters!");
                    return;
                }

                // Prepare payload
                const newTaskPayload = {
                    Title: this.newTask.Title.trim(),
                    Description: this.newTask.Description.trim(),
                    ProjectID: parseInt(this.newTask.ProjectID),
                    AssignedTo: this.newTask.AssignedTo,
                    StatusID: parseInt(this.newTask.StatusID),
                    DueDate: this.newTask.DueDate,
                    ParentTaskID: this.newTask.ParentTaskID ? parseInt(this.newTask.ParentTaskID) : null,
                    CreatedAt: new Date().toISOString(),
                    ProjectIssueTypeID: parseInt(this.newTask.ProjectIssueTypeID)
                };

                console.log("Payload sent to API:", newTaskPayload);

                const response = await axios.post(
                    "http://localhost:5260/api/tasks/AddTasks",
                    newTaskPayload,
                    { headers: { "Content-Type": "application/json" } }
                );

                if (response.status === 200 && response.data.success) {
                    toast.success("Task added successfully!");
                    await this.fetchTasks(); // Reload task list

                    // Calculate page for new task
                    const newTaskIndex = this.tasks.findIndex(task => task.Title === newTaskPayload.Title);
                    if (newTaskIndex !== -1) {
                        this.currentPage = Math.ceil((newTaskIndex + 1) / this.itemsPerPage);
                    }

                    this.modalInstance.hide(); // Close modal
                    this.resetForm(); // Reset form
                } else {
                    toast.error(response.data.message || "Failed to add task!");
                }
            } catch (error) {
                console.error("Error adding task:", error.response?.data || error.message);
                toast.error("An error occurred, please try again!");
            }
        },


        async updateTask() {
            try {
                // Validate required fields
                if (
                    !this.newTask.Title?.trim() ||
                    !this.newTask.Description?.trim() ||
                    !this.newTask.ProjectID ||
                    !this.newTask.AssignedTo ||
                    !this.newTask.StatusID ||
                    !this.newTask.DueDate ||
                    !this.newTask.ProjectIssueTypeID
                ) {
                    toast.error("Please fill in all required fields!");
                    return;
                }

                // Validate Title length
                if (this.newTask.Title.length > 100) {
                    toast.error("Title must not exceed 100 characters!");
                    return;
                }

                // Validate special characters in Title
                const specialCharRegex = /[^a-zA-Z0-9À-ỹ\s.,-]/;
                if (specialCharRegex.test(this.newTask.Title)) {
                    toast.error("Title must not contain special characters!");
                    return;
                }

                // Validate Description length
                if (this.newTask.Description.length > 500) {
                    toast.error("Description must not exceed 500 characters!");
                    return;
                }

                // Fetch current task information
                const taskRes = await axios.get(`http://localhost:5260/api/tasks/GetTaskById?TaskID=${this.selectedTaskId}`);
                console.log("Response from GetTaskById:", taskRes.data);

                if (!taskRes.data.success || !taskRes.data.task) {
                    toast.error("Task not found for update!");
                    return;
                }

                const task = taskRes.data.task[0];

                const updatePayload = {
                    TaskID: task.TaskID,
                    Title: this.newTask.Title.trim(),
                    Description: this.newTask.Description.trim(),
                    ProjectID: parseInt(this.newTask.ProjectID),
                    AssignedTo: this.newTask.AssignedTo,
                    StatusID: parseInt(this.newTask.StatusID),
                    DueDate: this.newTask.DueDate,
                    ParentTaskID: this.newTask.ParentTaskID ? parseInt(this.newTask.ParentTaskID) : 0,
                    ProjectIssueTypeID: parseInt(this.newTask.ProjectIssueTypeID),
                    UpdatedAt: new Date().toISOString(),
                };

                console.log("Payload sent to API:", updatePayload);

                const response = await axios.put(
                    `http://localhost:5260/api/tasks/UpdateTasks?TaskID=${this.selectedTaskId}`,
                    updatePayload,
                    { headers: { "Content-Type": "application/json" } }
                );

                if (response.status === 200 && response.data.success) {
                    toast.success("Task updated successfully!");
                    this.fetchTasks();
                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    toast.error(response.data.message || "Failed to update task!");
                }
            } catch (error) {
                console.error("Error updating task:", error.response?.data || error.message);
                toast.error("An error occurred while updating, please try again!");
            }
        },
          

        resetForm() {
            this.newTask = {
                Title: "", // Reset về giá trị mặc định
                Description: "",
                ProjectID: "",
                AssignedTo: "",
                StatusID: "", // Chắc chắn là số
                DueDate: "", // Lấy ngày tạo hiện tại
                ParentTaskID: "", // Lấy ngày tạo hiện tại
                ProjectIssueTypeID: ""
            };
        },

        async deleteTask(taskID) {
            if (!taskID || taskID <= 0) {
                toast.error("TaskID không hợp lệ!");
                return;
            }

            const result = await Swal.fire({
                title: "Bạn có chắc chắn muốn xóa task này không?",
                text: "Hành động này không thể hoàn tác!",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Xóa",
                cancelButtonText: "Hủy",
                customClass: {
                    confirmButton: "btn-confirm-delete",
                    cancelButton: "btn-cancel"
                }
            });

            if (!result.isConfirmed) return;

            console.log("TaskID:", taskID);

            try {
                const response = await axios.delete(`http://localhost:5260/api/tasks/DeleteTasks?TaskID=${taskID}`);

                if (response.status === 200 && response.data.success) {
                    toast.success("Xóa task thành công!");
                    await this.fetchTasks(); // Load lại danh sách sau khi xóa

                    // Cập nhật trang nếu cần
                    const totalPagesAfterDelete = Math.ceil(this.tasks.length / this.itemsPerPage);
                    if (this.currentPage > totalPagesAfterDelete) {
                        this.currentPage = Math.max(1, totalPagesAfterDelete);
                    }
                } else {
                    toast.error(response.data.message || "Không thể xóa task!");
                }
            } catch (error) {
                console.error("Lỗi khi xóa task:", error.response?.data || error.message);
                toast.error("Đã xảy ra lỗi khi xóa, vui lòng thử lại!");
            }
        },

        async handleSubmit() {
            if (this.isEditing) {
                await this.updateTask();
            } else {
                await this.addTask();
            }
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
    //Lọc danh sách, tính toán giá trị động
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
    //Gọi API, thao tác DOM, đăng ký sự kiện
    mounted() {
        this.fetchTasks(); // Gọi API khi component được mount
    },
};