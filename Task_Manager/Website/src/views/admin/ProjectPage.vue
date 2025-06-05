<template>
  <div class="d-flex">
    <!-- Sidebar -->
    <div class="admin-layout">
      <AdminLayout />
    </div>

    <!-- Main Content -->
    <div class="content-container flex-grow-1 p-3">
      <div class="header__">
        <h1 class="mb-3 user-title">Project Page</h1>

        <!-- Add User Button -->
        <button type="button" class="btn btn-primary mb-3 btn_user user-title" @click="openModal(null)">
          Add Project
        </button>

        <!-- Search Bar -->
        <div class="d-flex mb-2 float-end">
          <input type="text" class="form-control w-100 search__" placeholder="Search..." v-model="searchQuery" />
        </div>
      </div>


      <!-- User Table -->
      <div class="table-responsive d-flex justify-content-center">
        <table class="table table-striped table-mobile-responsive table-mobile-sided">
          <thead class="table-light">
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>CreatedBy</th>
              <th>CreatedAt</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="project in paginatedProjects" :key="project.projectID">
              <td data-content="Name">{{ project.Name }}</td>
              <td data-content="Description">{{ project.Description }}</td>
              <td data-content="CreatedBy">{{ getUserName(project.CreatedBy) }}</td>
              <td data-content="Create at">{{ formatDate(project.CreatedAt) }}</td>
              <td data-content="Actions">
                <button class="btn btn-sm btn-outline-primary me-2" @click="openModal(project)">
                  <font-awesome-icon icon="edit" />
                </button>
                <button class="btn btn-sm btn-outline-danger" @click="deleteProject(project.ProjectID)">
                  <font-awesome-icon icon="trash" />
                </button>
              </td>
            </tr>
          </tbody>


        </table>
      </div>
      <!-- Pagination -->
      <nav class="">
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
    <div class="modal fade" id="addProjectModal" tabindex="-1" aria-labelledby="addProjectModalLabel" aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="addRoleModalLabel">
              {{ isEditing ? "Edit Project" : "Add New Project" }}
            </h5>

            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <form @submit.prevent="handleSubmit">
              <div class="mb-3">
                <label for="Name" class="col-form-label">Name:</label>
                <input type="text" class="form-control" id="Name" v-model="newProject.Name">
              </div>
              <div class="mb-3">
                <label for="Description" class="col-form-label">Description:</label>
                <input type="text" class="form-control" id="Description" v-model="newProject.Description">
              </div>
              <div class="mb-3">
                <label for="user" class="col-form-label float-start">CreatedBy:</label>
                <select class="form-control" id="User_ID" v-model="newProject.CreatedBy">
                  <option value="" disabled>-- Chọn người dùng --</option>
                  <option v-for="user in users" :key="user.User_ID" :value="user.User_ID">
                    {{ user.FullName }}
                  </option>
                </select>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                <button type="submit" class="btn btn-primary">
                  {{ isEditing ? "Save Changes" : "Add Project" }}
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
  import ProjectPage from '@/assets/js/projectpage.js';

  export default {
    components: {
      AdminLayout,
    },
    //Mixins trong Vue.js là một cách để tái sử dụng logic giữa các component
    mixins: [ProjectPage],
  };

</script>

<style>
  @import '/src/assets/style/rolepage.css';
</style>