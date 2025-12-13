<template>
  <div class="nav-bar">
    <UserLayout />
  </div>

  <ProjectLayout />

  <div class="container-fluid mt-4 px-4" style="margin-left: 126px !important;">

    <!-- Tiêu đề và Thanh tìm kiếm -->
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h2>Task List</h2>
      <div class="d-flex gap-2">
        <input type="text" class="form-control form-control-sm" placeholder="Search list" v-model="searchQuery" />
      </div>
    </div>

    <!-- Bảng cuộn ngang -->
    <div class="table-responsive overflow-auto" style="white-space: nowrap;">
      <table class="table table-hover align-middle">
        <thead class="table-light">
          <tr>
            <th style="min-width: 80px;">Title</th>
            <th style="min-width: 200px;">Description</th>
            <th style="min-width: 100px;">Status</th>
            <th style="min-width: 120px;">Create at</th>
            <th style="min-width: 140px;">Due date</th>
            <th style="min-width: 200px;">Assignee</th>
          </tr>
        </thead>
        <tbody v-if="filteredTasks.length > 0">
          <tr v-for="task in paginatedTasks" :key="task.TaskID">
            <td>{{ task.Title }}</td>
            <td style="max-width: 200px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">{{
              task.Description }}</td>
            <td>
              <span class="badge" :class="{
      'bg-success': getStatusName(task.StatusID).toUpperCase() === 'DONE',
      'bg-primary': getStatusName(task.StatusID).toUpperCase() === 'IN PROGRESS',
      'bg-secondary': getStatusName(task.StatusID).toUpperCase() === 'TO DO'
    }">
                {{ getStatusName(task.StatusID) }}
              </span>
            </td>

            <td>{{ new Date(task.CreatedAt).toLocaleDateString() }}</td>
            <td>{{ new Date(task.DueDate).toLocaleDateString() }}</td>
            <td>
              <img :src="task.AssigneeAvatar || 'https://via.placeholder.com/30'" class="rounded-circle" width="30"
                height="30" />
              <span class="text-truncate" style="max-width: 140px;">{{ getUserName(task.AssignedTo || 'Unassigned')
                }}</span>
            </td>
          </tr>
        </tbody>
        <tbody v-else>
          <tr>
            <td colspan="6" class="text-center text-muted py-3">
              No tasks found.
            </td>
          </tr>
        </tbody>
      </table>

      
    </div>

    
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
</template>

<style>
  .pdleft {
    padding-left: 18.5rem;
  }
</style>



<script>
  import UserLayout from '@/components/UserLayout.vue'
  import ProjectLayout from '@/components/ProjectLayout.vue'
  import TaskList from '@/assets/js/tasklist.js';


  export default {
    components: {
      UserLayout,
      ProjectLayout
    },

    mixins: [TaskList],
  };
</script>