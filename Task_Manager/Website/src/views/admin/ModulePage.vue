<template>
  <div class="d-flex">
    <!-- Sidebar -->
    <div class="admin-layout">
      <AdminLayout />
    </div>

    <!-- Main Content -->
    <div class="content-container flex-grow-1 p-3">
      <h1 class="mb-3">Module Page</h1>

      <!-- Add User Button -->
      <button type="button" class="btn btn-primary mb-3" @click="openModal(null)">
        Add Module
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
              <th>Module name</th>
              <th>Link</th>
              <th>Icon</th>
              <th>Order</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="module in paginatedModules" :key="module.moduleID">
              <td>{{ module.ModuleName }}</td>
              <td>{{ module.Link }}</td>
              <td>{{ module.Icon }}</td>
              <td>{{ module.OrderNumber }}</td>
              <td>
                <button class="btn btn-sm btn-outline-primary me-2" @click="openModal(module)">
                  <font-awesome-icon icon="edit" />
                </button>
                <button class="btn btn-sm btn-outline-danger"  @click="deleteModule(module.ModuleID)">
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

    <!-- Add Module Modal -->
    <div class="modal fade" id="addModuleModal" tabindex="-1" aria-labelledby="addModuleModalLabel" aria-hidden="true">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="addModuleModalLabel">
              {{ isEditing ? "Edit Module" : "Add New Module" }}
            </h5>

            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <form @submit.prevent="handleSubmit">
              <div class="row">
                <div class="col-md-6">
                  <label for="ModuleName" class="col-form-label">Module Name:</label>
                  <input type="text" class="form-control" id="ModuleName" v-model="newModule.ModuleName" required>
                </div>
                <div class="col-md-6">
                  <label for="ParentID" class="col-form-label">Parent:</label>
                  <input type="text" class="form-control" id="ParentID" v-model="newModule.ParentID">
                </div>
              </div>

              <div class="row">
                <div class="col-md-6">
                  <label for="Link" class="col-form-label">Link:</label>
                  <input type="text" class="form-control" id="Link" v-model="newModule.Link" required>
                </div>
                <div class="col-md-3">
                  <label for="Icon" class="col-form-label">Icon:</label>
                  <input type="text" class="form-control" id="Icon" v-model="newModule.Icon" required>
                </div>
                <div class="col-md-3">
                  <label for="OrderNumber" class="col-form-label">Order:</label>
                  <input type="number" class="form-control" id="OrderNumber" v-model="newModule.OrderNumber" required>
                </div>
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
  import ModulePage from '@/assets/js/modulepage.js';

  export default {
    components: {
      AdminLayout,
    },
    //Mixins trong Vue.js là một cách để tái sử dụng logic giữa các component
    mixins: [ModulePage],
  };

</script>