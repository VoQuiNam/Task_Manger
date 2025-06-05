import axios from 'axios';
import { Modal } from "bootstrap";
import { useRoute } from 'vue-router';
import { toast } from "vue3-toastify";
import Swal from "sweetalert2";

export default {
    data() {
        return {
            userManagement: [],
            users: [],
            modalInstance: null,
            newUser: {
                ProjectID: this.projectId,
                UserID: "",
                RoleInProject: "",
            },
            isEditing: false,  // Biến xác định chế độ chỉnh sửa
            selectedTypeId: null,  // Lưu ID người dùng đang chỉnh sửa
            projectId: null,  // <-- Thêm dòng này
        };
    },
    mounted() {
        const route = useRoute();
        this.projectId = route.query.projectId;
        const storedUser = localStorage.getItem("currentUser");
        const currentUser = storedUser ? JSON.parse(storedUser) : null;
        this.currentUserId = currentUser?.User_ID || null;

        this.fetchUserManagement();
        this.newUser.ProjectID = this.projectId;
    },

    methods: {
        async fetchUserManagement() {
            try {
                // Gọi API lấy người dùng theo ProjectID và toàn bộ danh sách users
                const [userManagementResponse, usersResponse] = await Promise.all([
                    axios.get(`http://localhost:5260/api/ProjectUsers/GetProjectUsersByProjectId?projectId=${this.projectId}`),
                    axios.get("http://localhost:5260/api/users/GetUsers"),
                ]);

                // Kiểm tra dữ liệu có tồn tại không trước khi gán
                this.userManagement = userManagementResponse.data || [];
                this.users = usersResponse.data || [];

                // ⛳ Tìm role của currentUser trong project này
                const myRecord = this.userManagement.find(u => u.UserID == this.currentUserId);
                this.currentUserRole = myRecord?.RoleInProject || "";
            } catch (error) {
                console.error("Error fetching data:", error);
            }
        },


        getUserInfo(userID) {
            return this.users.find(u => u.User_ID === userID) || {};
        },

        openModal(userManagement = null) {
            if (userManagement) {
                this.isEditing = true;
                this.originalUserID = userManagement.UserID; // 👈 lưu lại ID cũ

                this.newUser = {
                    ProjectID: this.projectId,
                    UserID: userManagement.UserID,
                    RoleInProject: userManagement.RoleInProject
                };
            } else {
                this.isEditing = false;
                this.resetForm();
                this.newUser.ProjectID = this.projectId;
            }

            this.modalInstance = new Modal(document.getElementById("addUserModal"));
            this.modalInstance.show();
        },


        async addProjectUser() {
            try {
                // Kiểm tra dữ liệu đầu vào
                if (!this.newUser.ProjectID || !this.newUser.UserID || !this.newUser.RoleInProject) {
                    toast.error("Please fill in all fields: Project, User, and Role!");
                    return;
                }

                // 2. Kiểm tra xem người dùng đã có trong dự án chưa
                const checkResponse = await axios.get(
                    `http://localhost:5260/api/ProjectUsers/CheckUserExists?projectId=${this.newUser.ProjectID}&userId=${this.newUser.UserID}`
                );

                if (checkResponse.data.exists) {
                    toast.error("This user is already in the project!");
                    return;
                }

                // Tạo payload để gửi lên API
                const payload = {
                    ProjectID: this.newUser.ProjectID,
                    UserID: this.newUser.UserID,
                    RoleInProject: this.newUser.RoleInProject,
                };

                console.log("Payload gửi lên API:", payload);

                // Gửi POST request
                const response = await axios.post(
                    "http://localhost:5260/api/ProjectUsers/AddProjectUser",
                    payload,
                    {
                        headers: {
                            "Content-Type": "application/json",
                        },
                    }
                );

                // Xử lý phản hồi
                if (response.status === 200 && response.data.success) {
                    toast.success("User added to project successfully!");
                    await this.fetchUserManagement(); // Load lại danh sách user trong project

                    this.modalInstance.hide();  // Đóng modal
                    this.resetForm();           // Reset lại form
                } else {
                    toast.error(response.data.message || "Failed to add user to project.");
                }

            } catch (error) {
                console.error("Lỗi khi thêm user vào project:", error);
                toast.error("An error occurred, please try again.");
            }
        },

        async deleteProjectUser(projectId, userId) {
            try {
                const result = await Swal.fire({
                    title: "Are you sure you want to delete this user?",
                    text: "This action cannot be undone!",
                    icon: "warning",
                    showCancelButton: true,
                    confirmButtonText: "Delete",
                    cancelButtonText: "Cancel",
                    customClass: {
                        confirmButton: "btn-confirm-delete",  // Optional custom class
                        cancelButton: "btn-cancel"
                    },
                });

                if (!result.isConfirmed) return;

                const response = await axios.delete(
                    `http://localhost:5260/api/ProjectUsers/DeleteProjectUser`,
                    {
                        params: {
                            ProjectID: projectId,
                            UserID: userId
                        }
                    }
                );

                if (response.status === 200 && response.data.success) {
                    toast.success("User removed from project successfully!");
                    await this.fetchUserManagement(); // Refresh the user list
                } else {
                    toast.error(response.data.message || "Failed to remove user from project.");
                }

            } catch (error) {
                console.error("Error while deleting user:", error);
                toast.error("An error occurred while deleting. Please try again.");
            }
        },


        async updateProjectUser() {
            try {
                // Kiểm tra dữ liệu đầu vào
                if (!this.newUser.ProjectID || !this.newUser.UserID || !this.newUser.RoleInProject) {
                    toast.error("Please fill in all fields: Project, User, and Role!");
                    return;
                }

                // 2. Kiểm tra xem người dùng đã có trong dự án chưa
                const checkResponse = await axios.get(
                    `http://localhost:5260/api/ProjectUsers/CheckUserExists?projectId=${this.newUser.ProjectID}&userId=${this.newUser.UserID}&excludeUserId=${this.originalUserID}`
                );


                if (checkResponse.data.exists) {
                    toast.error("This user is already in the project!");
                    return;
                }

                // Gửi dữ liệu cập nhật lên API
                const payload = {
                    ProjectID: this.newUser.ProjectID,
                    UserID: this.newUser.UserID,
                    RoleInProject: this.newUser.RoleInProject
                };

                console.log("Payload for update:", payload);

                const response = await axios.put(
                    `http://localhost:5260/api/ProjectUsers/UpdateProjectUser`,
                    payload,
                    {
                        headers: {
                            "Content-Type": "application/json"
                        }
                    }
                );

                if (response.status === 200 && response.data.success) {
                    toast.success("Project user updated successfully!");
                    await this.fetchUserManagement(); // Load lại danh sách người dùng trong dự án
                    this.modalInstance.hide();        // Đóng modal
                    this.resetForm();                 // Reset form
                } else {
                    toast.error(response.data.message || "Failed to update project user.");
                }
            } catch (error) {
                console.error("Error updating project user:", error);
                toast.error("An error occurred while updating, please try again.");
            }
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
            this.newUser, {
                ProjectID: this.projectId || "",  // đảm bảo luôn có giá trị
                UserID: "",
                RoleInProject: "",
            };
        },



        async handleSubmit() {
            if (this.isEditing) {
                await this.updateProjectUser();
            } else {
                await this.addProjectUser();
            }
        },
    }
};
