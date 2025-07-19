import axios from 'axios';
import { useRoute } from 'vue-router';
import { Modal } from "bootstrap";
import { toast } from "vue3-toastify";
import Swal from 'sweetalert2';

export default {
    data() {
        return {
            tasks: [],
            users: [],
            issues: [],
            statuses: [],
            projects: [],
            projectIssues: [],
            labels: [],
            comments: [],
            tasklabels: [],            // Danh sách tất cả label có sẵn
            userId: null,
            currentUserRole: "",

            searchKeyword: '',
            filterIssueTypeId: null,
            selectedIssueTypeName: null,

            selectedLabels: [],
            previousLabels: [], // lưu label cũ để so sánh khi thay đổi
            editLabels: false,

            modalInstance: null,
            newTask: {
                Title: "",
                Description: "",
                ProjectID: "",
                ProjectIssueTypeID: "",
                StatusID: "",
                AssignedTo: "",
                ParentTaskID: "",
                DueDate: "",
            },
            newLabel: {  // Khởi tạo đối tượng newUser
                Name: "",
                IsActive: "",
                CreatedBy: "",
            },
            newComment: {  // Khởi tạo đối tượng newUser
                TaskID: "",
                UserID: "",
                Content: "",
            },
            selectedTask: {
                Title: "",
                Description: "",
                ProjectIssueTypeID: null,
                StatusID: null,
                AssignedTo: null,
                ParentTaskID: null,
                DueDate: null,
                CreatedAt: null
            },

            // Inline Editing States
            isEditingTitle: false,
            isEditingDescription: false,
            isEditingAssignee: false,
            isEditingParent: false,
            isEditingDueDate: false,
            isEditingStartDate: false,

            isEditing: false,  // Biến xác định chế độ chỉnh sửa
            selectedTaskId: null,  // Lưu ID người dùng đang chỉnh sửa
            projectId: null,  // <-- Thêm dòng này
            selectedFiles: [],
            isSaving: false,
            editingCommentId: null,
            editedContent: {},
            replyingToCommentId: null,
            replyContents: {},

            now: new Date(), // thời gian hiện tại để dùng trong tính toán
            timeInterval: null // để lưu interval ID nếu cần clear sau này

        };
    },
    mounted() {
        const route = useRoute();
        this.projectId = route.query.projectId; // <-- Lấy projectId từ URL
        const currentUser = JSON.parse(localStorage.getItem('currentUser'));

        if (currentUser && currentUser.User_ID) {
            this.userId = currentUser.User_ID;
            this.fetchTasks();
            this.fetchUserRole();
        }

        // Cập nhật mỗi 30 giây hoặc 60 giây tùy bạn muốn
        this.timeInterval = setInterval(() => {
            this.now = new Date(); // cập nhật lại `now`, timeAgo sẽ tính toán lại
        }, 60000); // mỗi 60 giây
    },

    beforeUnmount() {
        // Dọn dẹp interval khi component bị hủy
        clearInterval(this.timeInterval);
    },

    computed: {
        getDragGroup() {
            if (this.currentUserRole === 'Viewer') {
                return { name: 'tasks', pull: false, put: false }; // Không được kéo và không được nhận item
            }
            return { name: 'tasks', pull: true, put: true }; // Các role khác thì được phép kéo thả
        },

        filteredTasksByStatus() {
            const keyword = this.searchKeyword.trim().toLowerCase();
            return (statusId) => {
                const tasks = this.getTasksByStatus(statusId);
                return tasks.filter(task => {
                    const titleMatch = task.Title?.toLowerCase().includes(keyword);
                    const issueTypeName = this.getIssueTypeName(task.ProjectIssueTypeID)?.toLowerCase();
                    const issueMatch = issueTypeName?.includes(keyword);
                    const matchesKeyword = !keyword || titleMatch || issueMatch;
                    const matchesIssueType = !this.filterIssueTypeId || task.ProjectIssueTypeID === this.filterIssueTypeId;
                    return matchesKeyword && matchesIssueType;
                });
            };
        }


    },


    methods: {
        async fetchTasks() {
            try {
                // Gọi API users và roles cùng lúc
                const [tasksResponse, projectResponse, usersResponse, statusesResponse, issuesResponse, projectIssuesResponse, labelsResponse, taskLabelsResponse] = await Promise.all([
                    axios.get(`http://localhost:5260/api/tasks/GetTasksByProjectId?projectId=${this.projectId}`), // 👈 Sử dụng API mới
                    axios.get(`http://localhost:5260/api/projects/GetProjectsByUserId?userId=${this.userId}`),
                    axios.get(`http://localhost:5260/api/ProjectUsers/GetAllUsersInProject?projectId=${this.projectId}`), // 👈 cập nhật ở đây,
                    axios.get("http://localhost:5260/api/taskstatus/GetTaskStatus"),
                    axios.get("http://localhost:5260/api/issue_types/GetIssueTypes"),
                    axios.get(`http://localhost:5260/api/Project_Issue_Types/GetProject_Issue_TypesByProjectId?projectId=${this.projectId}`),
                    axios.get("http://localhost:5260/api/labels/GetLabels"),
                    axios.get("http://localhost:5260/api/TaskLabels/GetTaskLabels") // <-- thêm dòng này
                ]);

                // Kiểm tra dữ liệu có tồn tại không trước khi gán
                this.tasks = tasksResponse.data || [];
                this.projects = projectResponse.data || [];
                this.users = usersResponse.data || [];
                this.statuses = statusesResponse.data || [];
                this.issues = issuesResponse.data || [];
                this.projectIssues = projectIssuesResponse.data || [];
                this.labels = labelsResponse.data || [];
                this.tasklabels = taskLabelsResponse.data || [];
            } catch (error) {
                console.error("Error fetching data:", error);
            }
        },

        async fetchUserRole() {
            try {
                const res = await axios.get(`http://localhost:5260/api/ProjectUsers/GetProjectUsersByProjectId?projectId=${this.projectId}`);
                const myRole = res.data.find(u => u.UserID == this.userId);
                this.currentUserRole = myRole?.RoleInProject || "";
            } catch (error) {
                console.error("Error fetching user role:", error);
            }
        },

        buildCommentTree(flatComments) {
            const commentMap = {};
            const tree = [];

            // Tạo map từ CommentID đến comment
            flatComments.forEach(comment => {
                comment.children = [];
                commentMap[comment.CommentID] = comment;
            });

            // Duyệt lại và gán children vào parent
            flatComments.forEach(comment => {
                if (comment.ParentCommentID && commentMap[comment.ParentCommentID]) {
                    commentMap[comment.ParentCommentID].children.push(comment);
                } else {
                    tree.push(comment); // top-level
                }
            });

            return tree;
        },

        async fetchCommentsByTask(taskId) {
            try {
                const response = await axios.get(`http://localhost:5260/api/comments/GetCommentsByTask?taskID=${taskId}`);
                const flatComments = response.data?.comments || [];

                this.comments = this.buildCommentTree(flatComments); // chuyển sang cây
                console.log("Nested comments:", this.comments);
            } catch (error) {
                console.error("Error fetching comments:", error);
            }
        },

        timeAgo(datetime) {
            const now = this.now; // lấy từ data
            const commentDate = new Date(datetime);
            const seconds = Math.floor((now - commentDate) / 1000);

            const intervals = {
                year: 31536000,
                month: 2592000,
                week: 604800,
                day: 86400,
                hour: 3600,
                minute: 60,
            };

            for (const [unit, value] of Object.entries(intervals)) {
                const amount = Math.floor(seconds / value);
                if (amount >= 1) {
                    return `${amount} ${unit}${amount > 1 ? 's' : ''} ago`;
                }
            }

            return 'Just now';
        },

        canEdit() {
            return this.currentUserRole !== "Viewer";
        },

        async openTaskDetail(task) {
            this.selectedTaskId = task.TaskID;
            //toán tử trải để tránh thay đổi dữ liệu
            this.selectedTask = { ...task };
            this.selectedLabels = this.getLabelsForSelectedTask();
            this.previousLabels = [...this.selectedLabels]; // lưu trạng thái cũ để xử lý thay đổi
            this.editLabels = false; // luôn bắt đầu ở chế độ xem
            await Promise.all([
                this.fetchCommentsByTask(task.TaskID),
                this.loadAttachments(task.TaskID)
            ]);
        },

        async handleCreateLabel(newLabelName) {
            try {
                const res = await axios.post("http://localhost:5260/api/labels/AddLabels", {
                    name: newLabelName,
                    isActive: true,
                    createdBy: this.userId
                });

                const createdLabel = res.data.label; // ✅ đúng key trả về

                if (!createdLabel.LabelID) {
                    throw new Error("LabelID is undefined!");
                }

                // Cập nhật danh sách
                this.labels.push(createdLabel);
                this.selectedLabels.push(createdLabel);

                // Gắn vào task
                await axios.post("http://localhost:5260/api/TaskLabels/AddTaskLabel", {
                    TaskID: this.selectedTask.TaskID,
                    LabelID: createdLabel.LabelID
                });

                toast.success("Label created and attached!");
                await this.fetchTasks();
            } catch (err) {
                console.error("❌ Error:", err);
                toast.error("Failed to create or attach label.");
            }
        },

        async handleLabelChange() {
            const current = this.selectedLabels.map(l => l.LabelID);
            const previous = this.previousLabels.map(l => l.LabelID);

            const added = current.filter(id => !previous.includes(id));
            const removed = previous.filter(id => !current.includes(id));

            // Gắn label mới
            for (const labelID of added) {
                try {
                    await axios.post("http://localhost:5260/api/TaskLabels/AddTaskLabel", {
                        TaskID: this.selectedTask.TaskID,
                        LabelID: labelID
                    });
                    toast.success("Label added");
                } catch (err) {
                    console.error("Add label failed", err);
                    toast.error("Add failed");
                }
            }

            // Gỡ label
            for (const labelID of removed) {
                try {
                    const res = await axios.delete("http://localhost:5260/api/TaskLabels/DeleteTaskLabel", {
                        params: {
                            TaskID: this.selectedTask.TaskID,
                            LabelID: labelID
                        }
                    });

                    if (res.status === 200 && res.data.success) {
                        toast.success("Label removed!");
                    } else {
                        toast.error(res.data.message || "Không thể xóa label!");
                    }
                } catch (err) {
                    console.error("Remove label failed", err);
                    toast.error("Xóa label thất bại");
                }
            }

            // Load lại task labels và cập nhật chế độ hiển thị
            await this.fetchTasks();
            this.previousLabels = JSON.parse(JSON.stringify(this.selectedLabels));
            this.editLabels = false; // Đặt lại chế độ xem SAU KHI đã cập nhật xong
        },

        async onRemoveLabel(removedLabel) {
            try {
                await axios.delete("http://localhost:5260/api/TaskLabels/DeleteTaskLabel", {
                    params: {
                        TaskID: this.selectedTask.TaskID,
                        LabelID: removedLabel.LabelID,
                    },
                });

                toast.success("Label removed!");
                await this.fetchTasks();
            } catch (err) {
                console.error("Remove label failed", err);
                toast.error("Xóa label thất bại");
            }

            this.editLabels = false; // 👈 Thoát chế độ chỉnh sửa
            this.previousLabels = JSON.parse(JSON.stringify(this.selectedLabels));
        },

        async onLabelBlur() {
            if (!this.editLabels) return; // tránh gọi khi đang ở chế độ xem
            await this.handleLabelChange(); // sẽ gán lại this.editLabels = false trong đó
        },

        startReply(comment) {
            this.replyingToCommentId = comment.CommentID;
            this.replyContents[comment.CommentID] = ''; // ✅ gán phần tử thay vì gán cả object
        },

        cancelReply() {
            this.replyingToCommentId = null;
            this.replyContents = '';
        },

        async submitReply(parentCommentId) {
            const content = this.replyContents[parentCommentId]?.trim();

            if (!content) return;

            const currentUser = JSON.parse(localStorage.getItem("currentUser"));
            const userId = currentUser?.User_ID;

            const newComment = {
                TaskID: this.selectedTask.TaskID,
                UserID: userId,
                Content: content,
                ParentCommentID: parentCommentId
            };


            try {
                const response = await axios.post("http://localhost:5260/api/comments/AddComment", newComment);
                if (response.data?.success) {
                    toast.success("Reply added!");
                    this.replyContents[parentCommentId] = '';
                    this.replyingToCommentId = null;
                    await this.fetchCommentsByTask(this.selectedTask.TaskID);
                }
            } catch (error) {
                console.error("Reply failed:", error);
                alert("Failed to reply.");
            }
        },

        async submitComment() {
            if (!this.newComment.Content.trim()) return;

            // Lấy user từ localStorage
            const currentUser = JSON.parse(localStorage.getItem("currentUser"));
            const userId = currentUser?.User_ID;
            if (!userId) {
                toast.error("Current user not found!");
                return;
            }

            // Gán user và task vào comment
            this.newComment.UserID = userId;
            this.newComment.TaskID = this.selectedTaskId;

            try {
                const response = await axios.post("http://localhost:5260/api/comments/AddComment", this.newComment);

                if (response.data?.success) {
                    // Sau khi thêm thành công, load lại comment của task
                    toast.success("Comment added successfully!");
                    await this.fetchCommentsByTask(this.selectedTaskId);
                    this.newComment.Content = ""; // reset input
                } else {
                    toast.error("Không thêm được comment.");
                }

            } catch (error) {
                console.error("Error adding comment:", error);
                toast.error("Lỗi khi gửi comment.");
            }
        },

        async deleteComment(commentId) {
            const result = await Swal.fire({
                title: 'Delete confirmation',
                text: 'Are you sure you want to delete this comment and all its replies?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Delete',
                cancelButtonText: 'Cancel',
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6'
            });

            if (!result.isConfirmed) return;

            try {
                const response = await axios.delete(`http://localhost:5260/api/comments/DeleteComment?commentId=${commentId}`);

                if (response.data?.success) {
                    this.comments = this.removeCommentRecursively(this.comments, commentId);
                    toast.success('Comment and its replies have been deleted successfully!');
                } else {
                    Swal.fire('Error', response.data?.message || 'Unable to delete the comment.', 'error');
                }
            } catch (error) {
                console.error("Error deleting comment:", error);
                Swal.fire('Error', 'An error occurred while deleting the comment.', 'error');
            }
        },

        removeCommentRecursively(comments, commentIdToRemove) {
            return comments
                .filter(comment => comment.CommentID !== commentIdToRemove)
                .map(comment => ({
                    ...comment,
                    children: comment.children
                        ? this.removeCommentRecursively(comment.children, commentIdToRemove)
                        : []
                }))
                .filter(comment =>
                    comment.CommentID !== commentIdToRemove &&
                    (!comment.children || comment.children.length > 0 || comment.Content)
                );
        },

        startEditing(comment) {
            this.editingCommentId = comment.CommentID;
            this.editedContent[comment.CommentID] = comment.Content;
        },

        updateComment(commentId, newContent) {
            if (!newContent || !newContent.trim()) return;

            axios.put(`http://localhost:5260/api/comments/UpdateComment?commentID=${commentId}`, {
                CommentID: commentId,
                Content: newContent
            })
                .then(() => {
                    const updateContentRecursive = (comments) => {
                        for (const c of comments) {
                            if (c.CommentID === commentId) {
                                c.Content = newContent;
                                toast.success('Updated successfully');
                                return true;
                            }
                            if (c.children && updateContentRecursive(c.children)) {
                                return true;
                            }
                        }
                        return false;
                    };

                    updateContentRecursive(this.comments);
                    this.editingCommentId = null;
                    this.editedContent[commentId] = '';
                })
                .catch(err => console.error(err));
        },

        replaceCommentInTree(comments, commentId, updatedComment) {
            for (let i = 0; i < comments.length; i++) {
                if (comments[i].CommentID === commentId) {
                    comments[i] = { ...comments[i], ...updatedComment };
                    return true;
                }
                if (comments[i].children && comments[i].children.length) {
                    const found = this.replaceCommentInTree(comments[i].children, commentId, updatedComment);
                    if (found) return true;
                }
            }
            return false;
        },


        cancelEdit() {
            this.editingCommentId = null;
            this.editedContent = "";
        },

        async saveField(fieldName, value) {
            if (!this.selectedTask?.TaskID || this.isSaving) return;

            this.isSaving = true;
            try {
                const payload = {
                    ...this.selectedTask, // gửi full thông tin task
                    [fieldName]: value    // ghi đè field cần cập nhật
                };


                await axios.put(`http://localhost:5260/api/tasks/UpdateTasks?TaskID=${this.selectedTask.TaskID}`, payload);

                toast.success("Updated successfully!");
                this.fetchTasks();

            } catch (error) {
                console.error(`Error while updating ${fieldName}:`, error);
                toast.error("Update failed!");
            } finally {
                this.isSaving = false;
            }
        },


        // === Inline Editing Methods ===
        enableEdit(field) {
            if (field === 'title') this.isEditingTitle = true
            else if (field === 'description') this.isEditingDescription = true
            else if (field === 'assignee') this.isEditingAssignee = true
            if (field === 'parent') this.isEditingParent = true;
            if (field === 'dueDate') this.isEditingDueDate = true;
            if (field === 'startDate') this.isEditingStartDate = true;
        },

        disableEdit(field) {
            if (field === 'title') this.isEditingTitle = false
            else if (field === 'description') this.isEditingDescription = false
            else if (field === 'assignee') this.isEditingAssignee = false
            if (field === 'parent') this.isEditingParent = false;
            if (field === 'dueDate') this.isEditingDueDate = false;
            if (field === 'startDate') this.isEditingStartDate = false;
        },

        formatDateForInput(date) {
            if (!date) return '';
            const d = new Date(date);
            const year = d.getFullYear();
            const month = String(d.getMonth() + 1).padStart(2, '0'); // tháng từ 0-11 nên +1
            const day = String(d.getDate()).padStart(2, '0');
            return `${year}-${month}-${day}`; // "YYYY-MM-DD"
        },


        async loadAttachments(taskID) {
            if (!taskID) {
                this.selectedTask.Attachments = []
                return
            }

            try {
                const response = await axios.get('http://localhost:5260/api/attachments/GetAttachmentsByTaskID', {
                    params: { taskID }
                })

                this.selectedTask.Attachments = response.data || []
            } catch (error) {
                console.error("Lỗi khi tải file đính kèm:", error)
                this.selectedTask.Attachments = []
            }
        },


        isImageFile(filePath) {
            //i: cờ để kiểm tra không phân biệt chữ hoa/chữ thường.
            return /\.(jpg|jpeg|png|gif|bmp|webp)$/i.test(filePath);
        },

        getAttachmentUrl(filePath) {
            const fileName = filePath.split('/').pop(); // lấy tên file từ đường dẫn
            return `http://localhost:5260/api/attachments/DownloadAttachment?fileName=${fileName}`;
        },

        getFileName(filePath) {
            return filePath.split('/').pop();
        },


        formatDate(dateStr) {
            const date = new Date(dateStr);
            return date.toLocaleDateString('en-GB', {
                day: '2-digit',
                month: 'short'
            }).toUpperCase(); // Ví dụ: 24 APR
        },

        // Lấy danh sách label tương ứng với task
        getLabelsForSelectedTask() {
            const labelIds = this.tasklabels
                .filter(tl => tl.TaskID === this.selectedTask.TaskID)
                .map(tl => tl.LabelID);

            return labelIds
                .map(id => this.labels.find(l => l.LabelID === id))
                .filter(label => label);
        },


        getIssueTypeName(projectIssueTypeId) {
            // Tìm ProjectIssueType theo ProjectIssueTypeID
            const projectIssueType = this.projectIssues.find(pit => pit.ProjectIssueTypeID == projectIssueTypeId);
            if (!projectIssueType) return 'Unknown';

            // Từ ProjectIssueType lấy TypeID, rồi tìm Name trong issues
            const issue = this.issues.find(i => i.TypeID === projectIssueType.TypeID);
            return issue ? issue.Name : 'Unknown';
        },

        getStatusName(statusId) {
            const status = this.statuses.find(s => s.StatusID === statusId);
            return status ? status.Name : '';
        },

        getUserName(User_ID) {
            if (!this.users || this.users.length === 0) return "N/A";
            const user = this.users.find(u => u.User_ID === User_ID);
            if (!user) return "N/A";

            const names = user.FullName.trim().split(' ').filter(word => word); // Tách và lọc từ rỗng
            if (names.length === 1) {
                return names[0][0].toUpperCase(); // Nếu chỉ có 1 từ, lấy chữ cái đầu tiên
            }

            const firstInitial = names[0][0].toUpperCase();//Chữ cái đầu tiên của từ đầu tiên
            const lastInitial = names[names.length - 1][0].toUpperCase();

            return firstInitial + lastInitial;
        },

        getTitleTask(taskId) {
            const task = this.tasks.find(t => t.TaskID === taskId);
            return task ? task.Title : "Unknown Task";
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

            this.modalInstance = new Modal(document.getElementById("createTaskModal"));
            this.modalInstance.show();
        },

        handleFileUpload(event) {
            this.selectedFiles = Array.from(event.target.files); // chuyển từ FileList sang Array
            console.log("Check selectedFiles:", this.selectedFiles); // để debug
        },

        triggerFileInput() {
            this.$refs.fileInput.click();
        },

        async handleUpdateFileUpload(event) {
            //Lấy danh sách file mà người dùng đã chọn (FileList object)
            const files = event.target.files;
            const currentUser = JSON.parse(localStorage.getItem("currentUser"));
            const userId = currentUser?.User_ID;
            console.log("User ID is:", userId);

            if (!files.length || !this.selectedTask || !userId) {
                alert("Missing required data.");
                return;
            }

            let uploadedCount = 0;

            for (const file of files) {
                //Tạo form dữ liệu để gửi lên server.
                const formData = new FormData();
                formData.append("File", file);
                formData.append("TaskID", this.selectedTask.TaskID);
                formData.append("UploadedBy", userId);
                formData.append("CreatedAt", new Date().toISOString());

                try {
                    const response = await axios.post(
                        "http://localhost:5260/api/attachments/AddAttachments",
                        formData,
                        { headers: { "Content-Type": "multipart/form-data" } }
                    );

                    if (response.data && response.data.path) {
                        this.selectedTask.Attachments.push({
                            FilePath: response.data.path
                        });
                        uploadedCount++;
                    } else {
                        console.warn('Invalid attachment response:', response.data);
                    }

                } catch (error) {
                    console.error(`Upload failed for ${file.name}:`, error.response?.data || error.message);
                    alert(`Upload failed for ${file.name}`);
                }
            }

            // Hiển thị toast sau khi hoàn thành tất cả
            if (uploadedCount > 0) {
                toast.success(`Uploaded ${uploadedCount} file(s) successfully!`);
            } else {
                alert("No files were uploaded.");
            }

            // Reset input
            //Xóa giá trị trong input file, để người dùng có thể chọn lại cùng file nếu cần.
            this.$refs.fileInput.value = "";
        },


        scrollAttachments(direction) {
            const wrapper = this.$refs.attachmentWrapper;
            const scrollAmount = 200;
            if (wrapper) {
                wrapper.scrollLeft += direction === 'right' ? scrollAmount : -scrollAmount;
            }
        },


        async addTask() {
            try {
                const currentUser = JSON.parse(localStorage.getItem("currentUser"));
                const userId = currentUser?.User_ID;
                if (!userId) {
                    toast.error("Cannot find current user ID.");
                    return;
                }

                const plainDescription = this.newTask.Description.replace(/<[^>]*>?/gm, "").trim();

                if (
                    !this.newTask.Title?.trim() ||
                    !plainDescription ||
                    !this.newTask.ProjectID ||
                    !this.newTask.AssignedTo ||
                    !this.newTask.StatusID ||
                    !this.newTask.DueDate ||
                    !this.newTask.ProjectIssueTypeID
                ) {
                    toast.error("Please fill in all required fields!");
                    return;
                }

                if (this.newTask.Title.length > 100) {
                    toast.error("Title must not exceed 100 characters!");
                    return;
                }

                const specialCharRegex = /[^a-zA-Z0-9À-ỹ\s.,-]/;
                if (specialCharRegex.test(this.newTask.Title)) {
                    toast.error("Title must not contain special characters!");
                    return;
                }

                if (plainDescription.length > 500) {
                    toast.error("Description must not exceed 500 characters!");
                    return;
                }

                const newTaskPayload = {
                    Title: this.newTask.Title.trim(),
                    Description: plainDescription,
                    ProjectID: parseInt(this.newTask.ProjectID),
                    AssignedTo: this.newTask.AssignedTo,
                    StatusID: parseInt(this.newTask.StatusID),
                    DueDate: this.newTask.DueDate,
                    ParentTaskID: this.newTask.ParentTaskID ? parseInt(this.newTask.ParentTaskID) : null,
                    CreatedAt: new Date().toISOString(),
                    ProjectIssueTypeID: parseInt(this.newTask.ProjectIssueTypeID),
                    UploadedBy: userId // ✅ Gửi người tạo task
                };

                const response = await axios.post(
                    "http://localhost:5260/api/tasks/AddTasks",
                    newTaskPayload,
                    { headers: { "Content-Type": "application/json" } }
                );

                if (response.status === 200 && response.data.success) {
                    const createdTaskID = response.data.task?.TaskID;
                    toast.success("Task added successfully!");

                    // ✅ Gửi label nếu có
                    if (this.selectedLabels && this.selectedLabels.length > 0 && createdTaskID) {
                        for (const label of this.selectedLabels) {
                            try {
                                const labelResponse = await axios.post("http://localhost:5260/api/TaskLabels/AddTaskLabel", {
                                    taskID: createdTaskID,
                                    labelID: label.LabelID
                                }, {
                                    headers: { "Content-Type": "application/json" }
                                });

                                console.log('voday: ', labelResponse.data); // ✅ không còn lỗi

                            } catch (labelErr) {
                                console.error("Error adding label to task:", labelErr);
                                toast.error("Gán nhãn cho task thất bại!");
                            }
                        }
                    }


                    if (this.selectedFiles && this.selectedFiles.length > 0 && createdTaskID) {
                        for (const file of this.selectedFiles) {
                            const formData = new FormData();
                            formData.append("File", file); // ✅ key phải trùng với DTO property
                            formData.append("TaskID", createdTaskID);
                            formData.append("UploadedBy", userId);
                            formData.append("CreatedAt", new Date().toISOString());

                            try {
                                const uploadResponse = await axios.post(
                                    "http://localhost:5260/api/attachments/AddAttachments",
                                    formData,
                                    { headers: { "Content-Type": "multipart/form-data" } }
                                );

                                console.log("Upload response:", uploadResponse.data);
                            } catch (uploadErr) {
                                console.error("Error uploading file:", uploadErr.response?.data || uploadErr.message);
                                toast.error("Tải lên file thất bại!");
                            }
                        }
                    }


                    await this.fetchTasks();
                    this.modalInstance.hide();
                    this.resetForm();
                } else {
                    toast.error(response.data.message || "Failed to add task!");
                }
            } catch (error) {
                console.error("Error adding task:", error.response?.data || error.message);
                toast.error("An error occurred, please try again!");
            }
        },

        async deleteAttachment(attachment) {
            const result = await Swal.fire({
                title: 'Are you sure?',
                text: "This file will be permanently deleted.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Yes, delete it!',
                cancelButtonText: 'Cancel'
            });

            if (!result.isConfirmed) return;

            try {
                const fileId = attachment.FileID || this.extractFileIdFromPath(attachment.FilePath);
                const response = await axios.delete(`http://localhost:5260/api/attachments/DeleteAttachment?fileId=${fileId}`);

                if (response.status === 200) {
                    this.selectedTask.Attachments = this.selectedTask.Attachments.filter(
                        a => a.FilePath !== attachment.FilePath
                    );
                    toast.success("Attachment deleted successfully.");
                } else {
                    toast.error("Failed to delete the attachment.");
                }
            } catch (error) {
                console.error("Delete error:", error.response?.data || error.message);
                toast.error("Failed to delete the attachment.");
            }
        },



        async quickCreateTask(statusID) {
            try {
                const currentUser = JSON.parse(localStorage.getItem("currentUser"));
                const userId = currentUser?.User_ID;
                if (!userId) {
                    toast.error("Current user not found!");
                    return;
                }

                const defaultProjectID = this.projects.length > 0 ? this.projects[0].ProjectID : null;
                if (!defaultProjectID) {
                    toast.error("No available project!");
                    return;
                }

                const payload = {
                    Title: "New Task",
                    Description: "",
                    ProjectID: defaultProjectID,
                    AssignedTo: userId,
                    StatusID: statusID,
                    DueDate: new Date().toISOString().split("T")[0],
                    ParentTaskID: null,
                    CreatedAt: new Date().toISOString(),
                    ProjectIssueTypeID: this.filterIssueTypeId || (this.projectIssues.length > 0 ? this.projectIssues[0].ProjectIssueTypeID : 1),
                    UploadedBy: userId
                };

                const response = await axios.post(
                    "http://localhost:5260/api/tasks/AddTasks",
                    payload,
                    { headers: { "Content-Type": "application/json" } }
                );

                if (response.status === 200 && response.data.success) {
                    toast.success("Task created successfully!");
                    await this.fetchTasks();
                } else {
                    toast.error("Failed to create task.");
                }
            } catch (error) {
                console.error("Error while creating task:", error.response?.data || error.message);
                toast.error("An error occurred while creating the task.");
            }
        },


        async deleteTask(TaskID) {
            const result = await Swal.fire({
                title: 'Are you sure?',
                text: 'This action cannot be undone.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, delete it!',
                cancelButtonText: 'Cancel'
            });

            if (!result.isConfirmed) return;

            try {
                const response = await axios.delete(`http://localhost:5260/api/tasks/DeleteTasks?TaskID=${TaskID}`);

                if (response.status === 200 && response.data.success) {
                    toast.success("Task deleted successfully!");
                    await this.fetchTasks(); // Reload tasks after deletion
                } else {
                    toast.error(response.data.message || "Failed to delete the task.");
                }
            } catch (error) {
                console.error("Error deleting task:", error.response?.data || error.message);
                toast.error("An error occurred while deleting the task. Please try again.");
            }
        },

        async addNewLabel(newLabelName) {
            const currentUser = JSON.parse(localStorage.getItem("currentUser"));
            const userId = currentUser?.User_ID;
            if (!userId) {
                toast.error("Current user not found!");
                return;
            }

            try {
                const payload = {
                    name: newLabelName,
                    isActive: true,
                    createdBy: userId
                };

                const response = await axios.post(
                    "http://localhost:5260/api/labels/AddLabels",
                    payload,
                    { headers: { "Content-Type": "application/json" } }
                );

                const createdLabel = response.data.label;

                if (createdLabel && createdLabel.LabelID) {
                    // Add to available options and select it
                    this.labels.push(createdLabel);
                    this.selectedLabels.push(createdLabel);
                    toast.success("New label has been created!");
                } else {
                    toast.error("Failed to create label.");
                }
            } catch (error) {
                console.error("Error while creating label:", error.response?.data || error.message);
                toast.error("An error occurred while creating the label.");
            }
        },


        resetForm() {
            this.newTask = {
                Title: "",
                Description: "",
                ProjectID: "",
                AssignedTo: "",
                StatusID: "",
                DueDate: "",
                ParentTaskID: "",
                ProjectIssueTypeID: ""
            };

            // ✨ Reset Quill editor
            if (this.$refs.editor) {
                this.$refs.editor.setContents([]);
            }

            // ✨ Reset danh sách file đính kèm
            this.selectedFiles = [];

            // ✨ Reset input file
            if (this.$refs.fileInput) {
                this.$refs.fileInput.value = null;
            }

            this.selectedLabels = [];
        },



        async handleSubmit() {
            if (this.isEditing) {
                await this.updateTask();
            } else {
                await this.addTask();
            }
        },

        async onTaskDrop(evt, newStatusId) {
            if (!evt.added) {
                // Không phải sự kiện thêm => bỏ qua
                return;
            }

            const task = evt.added.element;

            if (!task) {
                console.warn("Không lấy được task sau khi drop.");
                return;
            }

            if (task.StatusID === newStatusId) {
                console.log("Status không đổi => bỏ qua cập nhật");
                return;
            }

            const updatedTask = {
                ...task,
                StatusID: newStatusId
            };

            try {
                await axios.put(`http://localhost:5260/api/tasks/UpdateTasks?TaskID=${updatedTask.TaskID}`, updatedTask);
                task.StatusID = newStatusId;
                await this.fetchTasks();
                toast.success("Status updated successfully!");
            } catch (error) {
                console.error("Lỗi khi cập nhật task:", error);
                toast.error("Error updating status");
            }
        },

        getTasksByStatus(statusId) {
            return this.tasks.filter(task => task.StatusID == statusId);
        }

    }
};
