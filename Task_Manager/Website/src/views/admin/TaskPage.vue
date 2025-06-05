<template>
  <div class="d-flex">
    <!-- Sidebar -->
    <div class="admin-layout">
      <AdminLayout />
    </div>

    <!-- Main Content -->
    <div class="content-container flex-grow-1 p-3">

      <div class="header__">
        <h1 class="mb-3 user-title">Tasks Page</h1>

        <!-- Add User Button -->
        <button type="button" class="btn btn-primary mb-3 btn_user user-title" @click="openModal(null)">
          Add Tasks
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
              <th>Title</th>
              <th>ProjectID</th>
              <th>AssignedTo</th>
              <th>StatusID</th>
              <th>ProjectIssue</th>
              <th>DueDate</th>
              <th>ParentTaskID</th>
              <th>CreatedAt</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="task in paginatedTasks" :key="task.taskID">
              <td data-content="Title">{{ task.Title }}</td>
              <td data-content="ProjectID">{{ getProjectName(task.ProjectID) }}</td>
              <td data-content="AssignedTo">{{  getUserName(task.AssignedTo) }}</td>
              <td data-content="StatusID">{{ getStatus(task.StatusID) }}</td>
              <td data-content="ProjectIssueTypeID">{{ task.ProjectIssueTypeID }}</td>
              <td data-content="DueDate">{{ formatDate(task.DueDate) }}</td>
              <td data-content="ParentTaskID">{{ task.ParentTaskID }}</td>
              <td data-content="Create at">{{ formatDate(task.CreatedAt) }}</td>
              <td data-content="Actions">
                <button class="btn btn-sm btn-outline-primary me-2" @click="openModal(task)">
                  <font-awesome-icon icon="edit" />
                </button>
                <button class="btn btn-sm btn-outline-danger" @click="deleteTask(task.TaskID)">
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
    <div class="modal fade" id="addTaskModal" tabindex="-1" aria-labelledby="addTaskModalLabel" aria-hidden="true">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="addTaskModalLabel">
              {{ isEditing ? "Edit Task" : "Add New Task" }}
            </h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <form @submit.prevent="handleSubmit">
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label for="title" class="col-form-label">Title:</label>
                  <input type="text" class="form-control" id="title" v-model="newTask.Title" required>
                </div>
                <div class="col-md-6 mb-3">
                  <label for="description" class="col-form-label">Description:</label>
                  <input type="text" class="form-control" id="description" v-model="newTask.Description" required>
                </div>
              </div>
    
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label for="ProjectID" class="col-form-label">Project:</label>
                  <select class="form-control" id="ProjectID" v-model="newTask.ProjectID" required>
                    <option value="" disabled>-- Chọn dự án --</option>
                    <option v-for="project in projects" :key="project.ProjectID" :value="project.ProjectID">
                      {{ project.Name }}
                    </option>
                  </select>
                </div>
                <div class="col-md-6 mb-3">
                  <label for="User_ID" class="col-form-label">User:</label>
                  <select class="form-control" id="User_ID" v-model="newTask.AssignedTo" required>
                    <option value="" disabled>-- Chọn người dùng --</option>
                    <option v-for="user in users" :key="user.User_ID" :value="user.User_ID">
                      {{ user.FullName }}
                    </option>
                  </select>
                </div>
              </div>
    
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label for="StatusID" class="col-form-label">Status:</label>
                  <select class="form-control" id="StatusID" v-model="newTask.StatusID" required>
                    <option value="" disabled>-- Chọn trạng thái --</option>
                    <option v-for="status in statuses" :key="status.StatusID" :value="status.StatusID">
                      {{ status.Name }}
                    </option>
                  </select>
                </div>
                <div class="col-md-6 mb-3">
                  <label for="DueDate" class="col-form-label">Due Date:</label>
                  <input type="datetime-local" class="form-control" id="DueDate" v-model="newTask.DueDate" required>
                </div>
              </div>
    
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label for="ParentTaskID" class="col-form-label">Parent Task ID:</label>
                  <input type="text" class="form-control" id="ParentTaskID" v-model="newTask.ParentTaskID">
                </div>

                <div class="col-md-6 mb-3">
                  <label for="ProjectIssueTypeID" class="col-form-label">Project Issue Type:</label>
                  <select class="form-control" id="ProjectIssueTypeID" v-model="newTask.ProjectIssueTypeID" required>
                    <option value="" disabled>-- Chọn Project Issue Type --</option>
                    <option v-for="pit in projectissuetypes" :key="pit.ProjectIssueTypeID" :value="pit.ProjectIssueTypeID">
                      {{ pit.ProjectIssueTypeID }}
                    </option>
                  </select>
                </div>
              </div>
    
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                <button type="submit" class="btn btn-primary">
                  {{ isEditing ? "Save Changes" : "Add Task" }}
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
  import TaskPage from '@/assets/js/taskpage.js';

  export default {
    components: {
      AdminLayout,
    },
    //Mixins trong Vue.js là một cách để tái sử dụng logic giữa các component
    mixins: [TaskPage],
  };

</script>


<style>
  @import '/src/assets/style/userpage.css';
</style>