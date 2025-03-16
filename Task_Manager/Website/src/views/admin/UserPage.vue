<template>
  <div class="d-flex">
    <!-- Sidebar -->
    <div class="admin-layout">
      <AdminLayout />
    </div>

    <!-- Main Content -->
    <div class="content-container flex-grow-1 p-3">
      <h1 class="mb-3">User Page</h1>

      <!-- Add User Button -->
      <!-- Add User Button -->
      <button type="button" class="btn btn-primary mb-3" @click="openModal">
        Add User
      </button>

      <!-- Search Bar -->
      <div class="d-flex justify-content-end mb-2">
        <input type="text" class="form-control w-25" placeholder="Search...">
      </div>

      <!-- User Table -->
      <div class="table-responsive">
        <table class="table table-striped table-hover">
          <thead class="table-light">
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Role</th>
              <th>Create at</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in users" :key="user.id">
              <td>{{ user.FullName }}</td>
              <td>{{ user.Email }}</td>
              <td>{{ getRoleName(user.RoleID) }}</td>
              <td>{{ formatDate(user.CreatedAt) }}</td>
              <td>
                <button class="btn btn-sm btn-outline-primary me-2" @click="openModal(user)">
                  <font-awesome-icon icon="edit" />
                </button>

                <button class="btn btn-sm btn-outline-danger" @click="deleteUser(user.id)">
                  <font-awesome-icon icon="trash" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <nav class="mt-4 mb-4">
        <ul class="pagination justify-content-end">
          <li class="page-item">
            <a class="page-link" href="#">Previous</a>
          </li>
          <li class="page-item active">
            <a class="page-link" href="#">1</a>
          </li>
          <li class="page-item">
            <a class="page-link" href="#">Next</a>
          </li>
        </ul>
      </nav>
    </div>

    <!-- Add User Modal -->
    <div class="modal fade" id="addUserModal" tabindex="-1" aria-labelledby="addUserModalLabel" aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="addUserModalLabel">
              {{ isEditing ? "Edit User" : "Add New User" }}
            </h5>

            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <form @submit.prevent="handleSubmit">
              <div class="mb-3">
                <label for="fullname" class="col-form-label">Full Name:</label>
                <input type="text" class="form-control" id="fullname" v-model="newUser.FullName" required>
              </div>
              <div class="mb-3">
                <label for="email" class="col-form-label">Email:</label>
                <input type="email" class="form-control" id="email" v-model="newUser.email" required>
              </div>
              <div class="mb-3">
                <label for="password" class="col-form-label">Password:</label>
                <input type="password" class="form-control" id="password" v-model="newUser.password" required>
              </div>

              <div class="mb-3">
                <label for="role" class="col-form-label">Role:</label>
                <select class="form-control" id="RoleID" v-model="newUser.RoleID" required>
                  <option value="" disabled>-- Chọn vai trò --</option>
                  <option v-for="role in roles" :key="role.RoleID" :value="role.RoleID">
                    {{ role.RoleName }}
                  </option>
                </select>
              </div>


              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                <button type="submit" class="btn btn-primary">
                  {{ isEditing ? "Save Changes" : "Add User" }}
                </button>

              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
  import AdminLayout from "@/components/AdminLayout.vue";
  import { Modal } from "bootstrap";
  import axios from "axios"; // Import axios
  import { v4 as uuidv4 } from "uuid";

  export default {
    components: {
      AdminLayout,
    },
    name: "UserPage",
    data() {
      return {
        users: [], // Danh sách người dùng
        roles: [], // Danh sách roles
        modalInstance: null,
        newUser: {  // Khởi tạo đối tượng newUser
          FullName: "",
          email: "",
          password: "",
          RoleID: ""
        },
        isEditing: false,  // Biến xác định chế độ chỉnh sửa
        selectedUserId: null,  // Lưu ID người dùng đang chỉnh sửa
      };
    },
    methods: {
      async fetchUsers() {
        try {
          // Gọi API users và roles cùng lúc
          const [usersResponse, rolesResponse] = await Promise.all([
            axios.get("http://localhost:5260/api/users/GetUsers"),
            axios.get("http://localhost:5260/api/roles/GetRoles"),
          ]);

          // Kiểm tra dữ liệu có tồn tại không trước khi gán
          this.users = usersResponse.data || [];
          this.roles = rolesResponse.data || [];
        } catch (error) {
          console.error("Error fetching data:", error);
        }
      },
      getStatusClass(status) {
        return status === "Active" ? "badge bg-success" : "badge bg-danger";
      },
      openModal(user = null) {
        this.isEditing = !!user; // Nếu có user thì là chỉnh sửa
        this.selectedUserId = user ? user.User_ID : null;

        if (user) {
          this.newUser = {
            FullName: user.FullName,
            email: user.Email,
            password: user.Password,  // Không hiển thị mật khẩu cũ vì lý do bảo mật
            RoleID: user.RoleID,
          };
        } else {
          this.resetForm();
        }

        this.modalInstance = new Modal(document.getElementById("addUserModal"));
        this.modalInstance.show();
      }
      ,
      formatDate(dateString) {
        return new Date(dateString).toLocaleDateString("vi-VN");
      },
      getRoleName(roleId) {
        if (!this.roles || this.roles.length === 0) return "N/A"; // Kiểm tra roles có dữ liệu không
        const role = this.roles.find(r => r.RoleID === roleId);
        return role ? role.RoleName : "N/A";
      },
      async addUser() {
        try {
          if (!this.newUser.FullName || !this.newUser.email || !this.newUser.password || !this.newUser.RoleID) {
            alert("Vui lòng nhập đầy đủ thông tin!");
            return;
          }


          const newUserPayload = {
            User_ID: uuidv4(), // Tạo UUID cho User_ID
            FullName: this.newUser.FullName.trim(),
            email: this.newUser.email.trim(),
            password: this.newUser.password,
            RoleID: Number(this.newUser.RoleID),
            CreateAt: new Date().toISOString(), // Tạo ngày giờ hiện tại
          };

          console.log("Dữ liệu gửi lên API:", newUserPayload);



          const response = await axios.post(
            `http://localhost:5260/api/users/AddUser?RoleID=${this.newUser.RoleID}`,
            newUserPayload,
            { headers: { "Content-Type": "application/json" } }
          );


          if (response.status === 200 && response.data.success) {
            alert("Thêm người dùng thành công!");
            this.fetchUsers();
            this.modalInstance.hide();
            this.resetForm();
          } else {
            alert(response.data.message || "Đã xảy ra lỗi khi thêm người dùng!");
          }
        } catch (error) {
          console.error("Lỗi khi thêm người dùng:", error);
          alert("Đã xảy ra lỗi, vui lòng thử lại!");
        }
      },
      async updateUser() {
        try {
          if (!this.newUser.FullName || !this.newUser.email || !this.newUser.RoleID || !this.newUser.password) {
            console.log(this.newUser.FullName);
            console.log(this.newUser.email);
            console.log(this.newUser.RoleID);
            console.log(this.newUser.password);
            alert("Vui lòng nhập đầy đủ thông tin!");

            return;
          }

          // Lấy User_ID từ API dựa vào email
          // 🛠 Kiểm tra API GetUserById
          const userResponse = await axios.get(`http://localhost:5260/api/users/GetUserById?User_ID=${this.selectedUserId}`);
          console.log('Phản hồi từ API GetUserById:',  this.selectedUserId);
          if (!userResponse.data.success || !userResponse.data.user.length) {
            alert("Không tìm thấy người dùng!");
            return;
          }

          const user = userResponse.data.user[0]; // Lấy thông tin user đầu tiên
          const userId = user.User_ID; // Lấy User_ID

          const updatePayload = {
            User_ID: userId,
            FullName: this.newUser.FullName.trim(),
            email: this.newUser.email.trim(),
            RoleID: Number(this.newUser.RoleID),
            password: this.newUser.password.trim(), // Thêm mật khẩu nếu API yêu cầu
            CreateAt: new Date().toISOString(),
          };

          console.log("Dữ liệu gửi lên API:", updatePayload);

          const response = await axios.put(
            `http://localhost:5260/api/users/UpdateUser?User_ID=${this.selectedUserId}&RoleID=${this.newUser.RoleID}`,
            updatePayload,
            { headers: { "Content-Type": "multipart/form-data" } }
          );

          if (response.status === 200 && response.data.success) {
            alert("Cập nhật người dùng thành công!");
            this.fetchUsers();
            this.modalInstance.hide();
            this.resetForm();
          } else {
            alert(response.data.message || "Đã xảy ra lỗi khi cập nhật người dùng!");
          }
        } catch (error) {
          console.error("Lỗi khi cập nhật người dùng:", error);
          alert("Đã xảy ra lỗi, vui lòng thử lại!");
        }
      },

      resetForm() {
        this.newUser = {
          FullName: "",
          email: "",
          password: "",
          roleId: this.roles.length > 0 ? this.roles[0].RoleID : "", // Reset về giá trị mặc định
        };
      },

      async deleteUser(id) {
        if (!confirm("Bạn có chắc chắn muốn xóa người dùng này không?")) return;

        try {
          const response = await axios.delete("http://localhost:5260/api/users/DeleteUser", {
            params: { id: id },
            headers: { "Content-Type": "application/json" }
          });

          if (response.status === 200) {
            alert("Xóa người dùng thành công!");
            this.fetchUsers();
          }
        } catch (error) {
          console.error("Lỗi khi xóa người dùng:", error);
          alert("Đã xảy ra lỗi, vui lòng thử lại!");
        }
      },
      async handleSubmit() {
        if (this.isEditing) {
          await this.updateUser();
        } else {
          await this.addUser();
        }
      }


    },
    mounted() {
      this.fetchUsers(); // Gọi API khi component được mount
    },
  };
</script>


<style>
  @import '/src/assets/style/userpage.css';
</style>