<template>
  <div class="nav-bar">
    <UserLayout />
  </div>

  <ProjectLayout />

  <div class="container-fluid mt-4 px-4">
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h5 class="fw-bold">Task List</h5>
      <div class="d-flex gap-2">
        <input type="text" class="form-control form-control-sm" placeholder="Search list" v-model="searchKeyword" />
        <button class="btn btn-outline-secondary btn-sm">
          <i class="fas fa-filter"></i> Filter
        </button>
      </div>
    </div>

    <div class="table-responsive">
      <table class="table table-hover align-middle w-100">

        <thead class="table-light">
          <tr>
            <th><input type="checkbox" /></th>
            <th>Type</th>
            <th>Key</th>
            <th>Summary</th>
            <th>Status</th>
            <th>Comments</th>
            <th>Assignee</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="task in filteredTasks" :key="task.TaskID">
            <td><input type="checkbox" /></td>
            <td>
              <i :class="getIssueIcon(task.ProjectIssueTypeID)" class="me-1"></i>
            </td>
            <td>{{ task.TaskKey }}</td>
            <td>{{ task.Title }}</td>
            <td>
              <span class="badge"
                :class="{
                  'bg-success': task.Status === 'DONE',
                  'bg-primary': task.Status === 'IN PROGRESS',
                  'bg-secondary': task.Status === 'TO DO'
                }"
              >
                {{ task.Status }}
              </span>
            </td>
            <td>
              <span v-if="task.CommentCount > 0">{{ task.CommentCount }} comment<span v-if="task.CommentCount > 1">s</span></span>
              <span v-else>Add comment</span>
            </td>
            <td>
              <img :src="task.AssigneeAvatar || 'https://via.placeholder.com/30'" class="rounded-circle" width="30" height="30" />
              {{ task.AssigneeName }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>


<script>
import UserLayout from '@/components/UserLayout.vue'
import ProjectLayout from '@/components/ProjectLayout.vue'

export default {
  components: {
    UserLayout,
    ProjectLayout
  },
  data() {
    return {
      tasks: [], // danh sách task từ API
      searchKeyword: '',
    };
  },
  computed: {
    filteredTasks() {
      const keyword = this.searchKeyword.toLowerCase();
      return this.tasks.filter(t =>
        t.Title.toLowerCase().includes(keyword) ||
        t.TaskKey.toLowerCase().includes(keyword)
      );
    }
  },
  methods: {
    getIssueIcon(issueTypeId) {
      const map = {
        1: 'fas fa-bug text-danger',
        2: 'fas fa-bolt text-purple',
        3: 'fas fa-check-square text-primary'
      };
      return map[issueTypeId] || 'fas fa-question-circle text-muted';
    }
  },
  mounted() {
    // Giả lập fetch task
    this.tasks = [
      {
        TaskID: 1,
        TaskKey: 'PT-2',
        Title: 'test 1',
        Status: 'DONE',
        CommentCount: 1,
        AssigneeName: 'Quí Nam',
        AssigneeAvatar: 'https://via.placeholder.com/30',
        ProjectIssueTypeID: 2
      },
      {
        TaskID: 2,
        TaskKey: 'PT-4',
        Title: 'test 3',
        Status: 'TO DO',
        CommentCount: 1,
        AssigneeName: 'Quí Nam',
        AssigneeAvatar: 'https://via.placeholder.com/30',
        ProjectIssueTypeID: 3
      }
      // ... thêm dữ liệu thực tế từ API của bạn
    ];
  }
};
</script>
