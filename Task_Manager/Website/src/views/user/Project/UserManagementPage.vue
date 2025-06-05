<template>
  <div class="nav-bar">
    <UserLayout />
  </div>

  <ProjectLayout />

  <div class="container py-4 ml-20">
    <!-- Tiêu đề + Toolbar -->
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h4 class="fw-bold">Users</h4>
      <div class="d-flex align-items-center gap-2">
        <input type="text" class="form-control" placeholder="Search for user name" v-model="searchQuery"
          style="width: 250px" />
        <button class="btn btn-primary" @click="openModal(null)"  v-if="currentUserRole === 'Administrator'">
          <i class="fas fa-plus me-1"></i> Add User
        </button>
      </div>
    </div>

    <!-- Table -->
    <div class="table-responsive">
      <table class="table align-middle">
        <thead>
          <tr>
            <th>User Name</th>
            <th>Email</th>
            <th>Role</th>
            <th class="text-end" v-if="currentUserRole === 'Administrator'">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in userManagement" :key="user.UserID">
            <td>
              {{ getUserInfo(user.UserID).FullName || '-' }}
            </td>
            <td>{{ getUserInfo(user.UserID).Email || '-' }}</td>
            <td>{{ user.RoleInProject }}</td>
            <td class="text-end">
              <button class="btn btn-sm btn-outline-primary me-1" title="Edit" @click="openModal(user)" v-if="currentUserRole === 'Administrator'">
                <i class="fas fa-pen"></i>
              </button>
              <button class="btn btn-sm btn-outline-danger" title="Delete"  @click="deleteProjectUser(user.ProjectID, user.UserID)" v-if="currentUserRole === 'Administrator'">
                <i class="fas fa-trash"></i>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="modal fade" id="addUserModal" tabindex="-1" aria-labelledby="addUserModalLabel" aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              {{ isEditing ? "Edit User" : "Add New User to Project" }}
            </h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>

          <div class="modal-body">
            <form @submit.prevent="handleSubmit">
              <!-- User select -->
              <div class="mb-3">
                <label class="form-label">Select User:</label>
                <select v-model="newUser.UserID" class="form-select" required>
                  <option disabled value="">-- Choose a user --</option>
                  <option v-for="user in users" :key="user.User_ID" :value="user.User_ID">
                    {{ user.FullName }} ({{ user.Email }})
                  </option>
                </select>
              </div>

              <!-- Role select -->
              <div class="mb-3">
                <label class="form-label">Role in Project:</label>
                <select v-model="newUser.RoleInProject" class="form-select" required>
                  <option disabled value="">-- Choose a role --</option>
                  <option value="Member">Member</option>
                  <option value="Administrator">Administrator</option>
                  <option value="Viewer">Viewer</option>
                </select>
              </div>

              <!-- Footer -->
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
  import UserLayout from '@/components/UserLayout.vue'
  import ProjectLayout from '@/components/ProjectLayout.vue'
  import UserManagement from '@/assets/js/userManagement.js';
  export default {
    components: {
      UserLayout,
      ProjectLayout
    },
    mixins: [UserManagement],

  };
</script>

<style scoped>
  @import '/src/assets/style/usermanagement.css';

  .table td,
  .table th {
    vertical-align: middle;
  }
</style>