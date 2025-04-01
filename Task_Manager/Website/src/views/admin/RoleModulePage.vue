<template>
  <div class="d-flex">
    <!-- Sidebar -->
    <div class="admin-layout">
      <AdminLayout />
    </div>

     <!-- Main Content -->
     <div class="content-container flex-grow-1 p-3">
        <h1 class="mb-3">Role Module Page</h1>
  
        <!-- Add User Button -->
        <button type="button" id="addRoleModule" class="btn btn-primary mb-3" @click="openModal(null)">
          Add Role Module
        </button>
  
        <!-- Search Bar -->
        <div class="d-flex justify-content-between align-items-center mb-3">
          <select id="roleFilter" class="form-control w-25" v-model="selectedRole">
            <option value="">-- All Roles --</option>
            <option v-for="role in roles" :key="role.RoleID" :value="role.RoleID">
              {{ role.RoleName }}
            </option>
          </select>
          <input type="text" class="form-control w-25" placeholder="Search..." v-model="searchQuery" />
          
        </div>
        
  
  
        <!-- User Table -->
        <div class="table-responsive">
          <table class="table table-striped table-hover">
            <thead class="table-light">
              <tr>
                <th>Module</th>
                <th>View</th>
                <th>Create</th>
                <th>Edit</th>
                <th>Delete</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="rolemodule in paginatedRoleModule" :key="rolemodule.rolemoduleID">
                <td>{{ getModuleName(rolemodule.ModuleID) }}</td>
        
                <!-- Checkbox cho View -->
                <td>
                  <label class="container">
                    <input type="checkbox" v-model="rolemodule.CanView" @change="updateCheckboxRoleModule(rolemodule)"/>
                    <span class="checkmark"></span>
                  </label>
                </td>
        
                <!-- Checkbox cho Create -->
                <td>
                  <label class="container">
                    <input type="checkbox" v-model="rolemodule.CanCreate" @change="updateCheckboxRoleModule(rolemodule)"/>
                    <span class="checkmark"></span>
                  </label>
                </td>
        
                <!-- Checkbox cho Edit -->
                <td>
                  <label class="container">
                    <input type="checkbox" v-model="rolemodule.CanEdit" @change="updateCheckboxRoleModule(rolemodule)"/>
                    <span class="checkmark"></span>
                  </label>
                </td>
        
                <!-- Checkbox cho Delete -->
                <td>
                  <label class="container">
                    <input type="checkbox" v-model="rolemodule.CanDelete" @change="updateCheckboxRoleModule(rolemodule)"/>
                    <span class="checkmark"></span>
                  </label>
                </td>
        
                <td>
                  <button class="btn btn-sm btn-outline-primary me-2" @click="openModal(rolemodule)">
                    <font-awesome-icon icon="edit" />
                  </button>
                  <button class="btn btn-sm btn-outline-danger" @click="deleteRoleModule(rolemodule.RoleModuleID)">
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
    <div class="modal fade" id="addRoleModuleModal" tabindex="-1" aria-labelledby="addRoleModuleModalLabel" aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="addRoleModuleModalLabel">
              {{ isEditing ? "Edit Role Module" : "Add New Role Module" }}
            </h5>

            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <form @submit.prevent="handleSubmit">
              <div class="mb-3">
                <label for="role" class="col-form-label">Role:</label>
                <select class="form-control" id="RoleID" v-model="newrolemodules.RoleID" required>
                  <option value="" disabled>-- Chọn vai trò --</option>
                  <option v-for="role in roles" :key="role.RoleID" :value="role.RoleID">
                    {{ role.RoleName }}
                  </option>
                </select>
              </div>

              <div class="mb-3">
                <label for="module" class="col-form-label">Module:</label>
                <select class="form-control" id="ModuleID" v-model="newrolemodules.ModuleID" required>
                  <option value="" disabled>-- Chọn module --</option>
                  <option v-for="module in modules" :key="module.ModuleID" :value="module.ModuleID">
                    {{ module.ModuleName }}
                  </option>
                </select>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                <button type="submit" class="btn btn-primary">
                  {{ isEditing ? "Save Changes" : "Add Module" }}
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
   import RoleModulePage from '@/assets/js/rolemodulepage.js';

  export default {
    components: {
      AdminLayout,
    },
    //Mixins trong Vue.js là một cách để tái sử dụng logic giữa các component
    mixins: [RoleModulePage],
  };

</script>

<style>
  @import '/src/assets/style/rolemodulepage.css';

  
</style>