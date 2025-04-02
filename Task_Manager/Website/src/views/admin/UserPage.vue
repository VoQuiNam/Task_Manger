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
      <button type="button" class="btn btn-primary mb-3" @click="openModal(null)">
        Add User
      </button>

      <!-- Search Bar -->
      <div class="d-flex justify-content-end mb-2">
        <input type="text" class="form-control w-25" placeholder="Search..." v-model="searchQuery" />
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
            <tr v-for="user in paginatedUsers" :key="user.id">
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
          <!-- Nút Previous -->
          <li class="page-item" :class="{ disabled: currentPage === 1 }">
            <a class="page-link" href="#" @click.prevent="goToPage(currentPage - 1)">Previous</a>
          </li>

          <!-- Hiển thị số trang -->
          <li class="page-item" v-for="page in totalPages" :key="page" :class="{ active: currentPage === page }">
            <a class="page-link" href="#" @click.prevent="goToPage(page)">{{ page }}</a>
          </li>

          <!-- Nút Next -->
          <li class="page-item" :class="{ disabled: currentPage === totalPages }">
            <a class="page-link" href="#" @click.prevent="goToPage(currentPage + 1)">Next</a>
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
                <input type="text" class="form-control" id="fullname" v-model="newUser.FullName">
              </div>
              <div class="mb-3">
                <label for="email" class="col-form-label">Email:</label>
                <input type="email" class="form-control" id="email" v-model="newUser.email">
              </div>
              <div class="mb-3">
                <label for="password" class="col-form-label">Password:</label>
                <input type="password" class="form-control" id="password" v-model="newUser.password">
              </div>

              <div class="mb-3">
                <label for="role" class="col-form-label">Role:</label>
                <select class="form-control" id="RoleID" v-model="newUser.RoleID">
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
  import UserPage from '@/assets/js/userpage.js';

  export default {
    components: {
      AdminLayout,
    },
    //Mixins trong Vue.js là một cách để tái sử dụng logic giữa các component
    mixins: [UserPage],
  };

</script>


<style>
  @import '/src/assets/style/userpage.css';
</style>