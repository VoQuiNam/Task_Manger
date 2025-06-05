<template>
    <div>
        <!-- Navbar -->
        <div class="nav-bar">
            <UserLayout />
        </div>

        <!-- Project Sidebar Layout -->
        <ProjectLayout />

        <div class="flex-grow-1 p-4 ml-250">
            <!-- Header -->
            <div class="d-flex justify-content-between align-items-center mb-4">
                <h2 class="mb-0">PT board</h2>
                <button class="btn btn-primary" @click="openModal(null)" :disabled="currentUserRole === 'Viewer'">
                    Create
                </button>
            </div>

            <!-- Toolbar -->
            <div class="d-flex align-items-center mb-3 flex-wrap gap-2">
                <input type="text" class="form-control form-control-sm w-auto" placeholder="Search"
                    v-model="searchKeyword" />
                <div class="d-flex align-items-center gap-2">
                    <span
                        class="badge bg-warning text-dark rounded-circle d-inline-flex align-items-center justify-content-center border_radius32">
                        QN
                    </span>
                </div>
                <div class="dropdown">
                    <button class="btn btn-outline-secondary btn-sm dropdown-toggle" data-bs-toggle="dropdown">
                        {{ selectedIssueTypeName || 'Type' }}
                    </button>
                    <ul class="dropdown-menu">
                        <li>
                            <a class="dropdown-item" href="#"
                                @click.prevent="filterIssueTypeId = null; selectedIssueTypeName = null">
                                All Types
                            </a>
                        </li>
                        <li v-for="item in projectIssues" :key="item.ProjectIssueTypeID">
                            <a class="dropdown-item" href="#"
                                @click.prevent="filterIssueTypeId = item.ProjectIssueTypeID; selectedIssueTypeName = getIssueTypeName(item.ProjectIssueTypeID)">
                                {{ getIssueTypeName(item.ProjectIssueTypeID) }}
                            </a>
                        </li>
                    </ul>
                </div>

                <div class="ms-auto d-flex gap-2">
                    <select class="form-select form-select-sm w-auto">
                        <option selected>Group by: None</option>
                        <option value="type">Type</option>
                        <option value="user">Assignee</option>
                    </select>
                    <button class="btn btn-outline-secondary btn-sm">📊</button>
                    <button class="btn btn-outline-secondary btn-sm">⚙️</button>
                </div>
            </div>

            <!-- Kanban Columns -->
            <div class="d-flex gap-3 overflow-auto">
                <div v-for="status in statuses" :key="status.StatusID" class="bg-light rounded p-2"
                    style="width: 300px; min-height: 400px;">
                    <div class="fw-bold mb-2">{{ status.Name }}</div>

                    <!-- Draggable Task Cards -->
                    <draggable :list="filteredTasksByStatus(status.StatusID)" :group="getDragGroup" item-key="TaskID"
                        @change="onTaskDrop($event, status.StatusID)">

                        <template #item="{ element: task }">
                            <div class="card mb-2 position-relative">
                                <!-- Three-dots menu -->
                                <div class="dropdown position-absolute top-0 end-0 m-2"
                                    v-if="currentUserRole !== 'Viewer'">
                                    <button class="btn btn-sm btn-link text-dark dropdown-toggle no-caret" type="button"
                                        data-bs-toggle="dropdown" aria-expanded="false">
                                        <i class="fas fa-ellipsis-v"></i>
                                    </button>
                                    <ul class="dropdown-menu">
                                        <li>
                                            <a class="dropdown-item text-danger" href="#"
                                                @click.prevent="deleteTask(task.TaskID)">
                                                <i class="fas fa-trash-alt me-2"></i> Delete
                                            </a>
                                        </li>
                                    </ul>
                                </div>

                                <!-- Card content -->
                                <div class="card-body p-2" style="cursor: pointer;" data-bs-toggle="modal"
                                    data-bs-target="#taskDetailModal" @click="openTaskDetail(task)">
                                    <div>{{ task.Title }}</div>
                                    <small class="text-danger">📅 {{ formatDate(task.DueDate) }}</small><br />
                                    <span class="text-muted small">{{ getIssueTypeName(task.ProjectIssueTypeID)
                                        }}</span>
                                    <div class="float-end badge bg-warning text-dark rounded-circle d-flex align-items-center justify-content-center"
                                        style="width: 28px; height: 28px;">
                                        {{ getUserName(task.AssignedTo) }}
                                    </div>
                                </div>
                            </div>
                        </template>
                    </draggable>

                    <!-- Quick Create -->
                    <button class="btn btn-light w-100 mt-2" @click="quickCreateTask(status.StatusID)"
                        v-if="currentUserRole !== 'Viewer'">+ Create</button>
                </div>
            </div>
        </div>

        <!-- Modal tạo Task -->
        <div class="modal fade" id="createTaskModal" tabindex="-1" aria-labelledby="createTaskModalLabel"
            aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="createTaskModalLabel">Create Task</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body" style="max-height: calc(100vh - 200px); overflow-y: auto;">
                        <form @submit.prevent="handleSubmit">
                            <!-- Chọn Project -->
                            <div class="mb-3">
                                <label for="project" class="form-label">Project</label>
                                <select v-model="newTask.ProjectID" id="project" class="form-select">
                                    <option v-for="project in projects" :key="project.ProjectID"
                                        :value="project.ProjectID">
                                        {{ project.Name }}
                                    </option>
                                </select>
                            </div>

                            <!-- Chọn Issue Type -->
                            <div class="mb-3">
                                <label for="projectIssues" class="form-label">Issue Type</label>
                                <select v-model="newTask.ProjectIssueTypeID" id="projectIssues" class="form-select">
                                    <!-- ✅ Đã sửa -->
                                    <option v-for="projectIssues in projectIssues"
                                        :key="projectIssues.ProjectIssueTypeID"
                                        :value="projectIssues.ProjectIssueTypeID">
                                        {{ getIssueTypeName(projectIssues.ProjectIssueTypeID) }}
                                    </option>

                                </select>
                            </div>

                            <!-- Chọn Status -->
                            <div class="mb-3">
                                <label for="status" class="form-label">Status</label>
                                <select v-model="newTask.StatusID" id="status" class="form-select">
                                    <option v-for="status in statuses" :key="status.StatusID" :value="status.StatusID">
                                        {{ status.Name }}
                                    </option>
                                </select>
                            </div>

                            <!-- Tiêu đề -->
                            <div class="mb-3">
                                <label for="title" class="form-label">Title</label>
                                <input v-model="newTask.Title" type="text" class="form-control" id="title"
                                    placeholder="Enter task title" required />
                            </div>

                            <!-- Mô tả -->
                            <!-- Description with Rich Text Editor -->
                            <div class="mb-3">
                                <label for="description" class="form-label">Description</label>
                                <QuillEditor ref="editor" v-model:content="newTask.Description" contentType="html"
                                    theme="snow" style="height: 200px" />
                            </div>



                            <!-- Assigned To -->
                            <div class="mb-3">
                                <label for="assignedTo" class="form-label">Assign To</label>
                                <select v-model="newTask.AssignedTo" id="assignedTo" class="form-select">
                                    <option v-for="user in users" :key="user.User_ID" :value="user.User_ID">
                                        {{ user.FullName }}
                                    </option>
                                </select>
                            </div>

                            <!-- Parent Task ID -->
                            <div class="mb-3">
                                <label for="parentTask" class="form-label">Parent Task</label>
                                <select v-model="newTask.ParentTaskID" id="parentTask" class="form-select">
                                    <option v-for="task in tasks" :key="task.TaskID" :value="task.TaskID">
                                        {{ task.Title }}
                                    </option>
                                </select>
                            </div>

                            <!-- Due Date -->
                            <div class="mb-3">
                                <label for="dueDate" class="form-label">Due Date</label>
                                <input v-model="newTask.DueDate" type="date" class="form-control" id="dueDate"
                                    required />
                            </div>

                            <div class="mb-3">
                                <label for="label" class="form-label">Label</label>
                                <UiMultiselect v-model="selectedLabels" :options="labels" :multiple="true"
                                    :taggable="true" label="Name" track-by="LabelID"
                                    placeholder="Chọn hoặc tạo label mới" @tag="addNewLabel" />
                            </div>




                            <!-- File Attachments -->
                            <div class="mb-3">
                                <label for="attachments" class="form-label">Attachments</label>
                                <input type="file" id="attachments" class="form-control" multiple ref="fileInput"
                                    @change="handleFileUpload" />

                            </div>


                            <button type="submit" class="btn btn-primary w-100">Create Task</button>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!-- Task Detail Modal -->
        <div class="modal fade" id="taskDetailModal" ref="taskDetailModal" tabindex="-1">
            <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable modal-lg">
                <div class="modal-content">
                    <!-- Header -->
                    <div class="modal-header">
                        <div v-if="!isEditingTitle && canEdit()" @click="enableEdit('title')" class="fw-bold">
                            {{ selectedTask.Title || 'Click to edit title' }}
                        </div>
                        <input v-else-if="isEditingTitle" v-model="selectedTask.Title"
                            @blur="saveField('Title', selectedTask.Title); disableEdit('title')"
                            @keyup.enter="saveField('Title', selectedTask.Title); disableEdit('title')"
                            class="form-control" />
                        <div v-else class="fw-bold">{{ selectedTask.Title }}</div>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>

                    <!-- Body -->
                    <div class="modal-body">
                        <!-- Description -->
                        <div class="mb-3">
                            <h6>Description</h6>
                            <div v-if="!isEditingDescription && canEdit()" @click="enableEdit('description')">
                                <p v-html="selectedTask.Description || 'Click to edit description'"></p>
                            </div>
                            <textarea v-else-if="isEditingDescription" v-model="selectedTask.Description"
                                class="form-control"
                                @blur="saveField('Description', selectedTask.Description); disableEdit('description')"
                                @keyup.enter="saveField('Description', selectedTask.Description); disableEdit('description')"></textarea>
                            <div v-else>
                                <p v-html="selectedTask.Description || '-'"></p>
                            </div>
                        </div>

                        <!-- Attachments -->
                        <div class="mb-3">
                            <h6>Attachments</h6>
                            <div v-if="selectedTask.Attachments && selectedTask.Attachments.length > 0"
                                class="position-relative">
                                <button @click="scrollAttachments('left')"
                                    class="btn btn-light position-absolute start-0 top-50 translate-middle-y z-1">◀</button>
                                <div ref="attachmentWrapper" class="d-flex overflow-auto gap-2 px-5"
                                    style="scroll-behavior: smooth;">
                                    <div v-for="(attachment, index) in selectedTask.Attachments" :key="index"
                                        class="border rounded p-2 flex-shrink-0 position-relative attachment-item"
                                        style="width: 150px;">
                                        <div class="position-relative">
                                            <div class="mb-1 d-flex align-items-center justify-content-center"
                                                style="height: 100px;">
                                                <img v-if="isImageFile(attachment.FilePath)"
                                                    :src="getAttachmentUrl(attachment.FilePath)" alt="Attachment"
                                                    class="img-fluid rounded" style="max-height: 100%;" />
                                                <i v-else class="fas fa-file-alt text-secondary"
                                                    style="font-size: 2rem;"></i>
                                            </div>
                                            <div
                                                class="attachment-actions position-absolute top-0 end-0 m-1 d-flex gap-1">
                                                <a :href="getAttachmentUrl(attachment.FilePath)"
                                                    :download="getFileName(attachment.FilePath)"
                                                    class="text-primary bg-white p-1 border border-secondary"
                                                    title="Download"
                                                    style="font-size: 0.85rem; width: 24px; height: 24px; display: flex; align-items: center; justify-content: center;">
                                                    <i class="fas fa-download"></i>
                                                </a>
                                                <button v-if="canEdit()" @click="deleteAttachment(attachment)"
                                                    title="Delete"
                                                    class="text-danger bg-white p-1 border border-secondary"
                                                    style="font-size: 0.85rem; width: 24px; height: 24px; display: flex; align-items: center; justify-content: center;">
                                                    <i class="fas fa-trash"></i>
                                                </button>
                                            </div>
                                        </div>
                                        <div class="text-truncate mt-1" style="font-size: 0.85rem;">
                                            {{ getFileName(attachment.FilePath) }}
                                        </div>
                                    </div>
                                </div>
                                <button @click="scrollAttachments('right')"
                                    class="btn btn-light position-absolute end-0 top-50 translate-middle-y z-1">▶</button>
                            </div>
                            <div v-else class="text-muted">No attachments</div>
                            <div class="mt-2" v-if="canEdit()">
                                <button class="btn btn-sm btn-outline-primary" @click="triggerFileInput">+ Add</button>
                                <input type="file" ref="fileInput" @change="handleUpdateFileUpload" class="d-none"
                                    multiple />
                            </div>
                        </div>

                        <!-- Dropdowns -->
                        <div class="row mb-3">
                            <div class="col-md-6">
                                <label class="form-label">Issue Type</label>
                                <select v-if="canEdit()" class="form-select" v-model="selectedTask.ProjectIssueTypeID"
                                    @change="saveField('ProjectIssueTypeID', selectedTask.ProjectIssueTypeID)">
                                    <option :value="null" disabled>Select issue type</option>
                                    <option v-for="projectIssue in projectIssues" :key="projectIssue.ProjectIssueTypeID"
                                        :value="projectIssue.ProjectIssueTypeID">
                                        {{ getIssueTypeName(projectIssue.ProjectIssueTypeID) }}
                                    </option>
                                </select>
                                <div v-else class="form-control">
                                    {{ getIssueTypeName(selectedTask.ProjectIssueTypeID) || '-' }}
                                </div>
                            </div>

                            <div class="col-md-6">
                                <label class="form-label">Status</label>
                                <select v-if="canEdit()" class="form-select" v-model="selectedTask.StatusID"
                                    @change="saveField('StatusID', selectedTask.StatusID)">
                                    <option :value="null" disabled>Select status</option>
                                    <option v-for="status in statuses" :key="status.StatusID" :value="status.StatusID">
                                        {{ status.Name }}
                                    </option>
                                </select>
                                <div v-else class="form-control">
                                    {{ getStatusName(selectedTask.StatusID) || '-' }}
                                </div>
                            </div>
                        </div>

                        <!-- Details -->
                        <hr />
                        <h6 class="mb-3">Details</h6>

                        <!-- Assignee -->
                        <div class="mb-2">
                            <strong>Assignee:</strong>
                            <span v-if="!isEditingAssignee && canEdit()" class="ms-2" @click="enableEdit('assignee')"
                                style="cursor: pointer;">
                                {{ getUserName(selectedTask.AssignedTo) || 'Click to assign' }}
                            </span>
                            <select v-else-if="isEditingAssignee" v-model="selectedTask.AssignedTo"
                                class="form-select form-select-sm d-inline-block w-auto ms-2"
                                @blur="saveField('AssignedTo', selectedTask.AssignedTo); disableEdit('assignee')"
                                @change="saveField('AssignedTo', selectedTask.AssignedTo); disableEdit('assignee')">
                                <option :value="null" disabled>Select user</option>
                                <option v-for="user in users" :key="user.User_ID" :value="user.User_ID">
                                    {{ user.FullName }}
                                </option>
                            </select>
                            <span v-else class="ms-2">
                                {{ getUserName(selectedTask.AssignedTo) || '-' }}
                            </span>
                        </div>

                        <!-- Labels -->
                        <div class="mb-2">
                            <strong>Labels:</strong>
                            <div v-if="editLabels && canEdit()" v-click-outside="onLabelBlur"
                                class="ms-2 d-inline-block">
                                <multiselect v-model="selectedLabels" :options="labels" :multiple="true"
                                    :taggable="true" label="Name" track-by="LabelID"
                                    placeholder="Select or create labels" @tag="handleCreateLabel" @close="onLabelBlur"
                                    @remove="onRemoveLabel" class="form-select-sm"
                                    style="min-width: 200px; max-width: 300px;" />
                            </div>
                            <span v-else class="ms-2" :style="{ cursor: canEdit() ? 'pointer' : 'default' }"
                                @click="canEdit() && (editLabels = true)">
                                <template v-if="getLabelsForSelectedTask().length > 0">
                                    <span v-for="label in getLabelsForSelectedTask()" :key="label.LabelID"
                                        class="badge me-1" :style="{ backgroundColor: label.Color || '#0d6efd' }">
                                        {{ label.Name }}
                                    </span>
                                </template>
                                <template v-else>
                                    <span class="text-muted">No labels</span>
                                </template>
                            </span>
                        </div>

                        <!-- Parent -->
                        <div class="mb-2">
                            <strong>Parent:</strong>
                            <span v-if="!isEditingParent && canEdit()" class="ms-2" style="cursor: pointer;"
                                @click="enableEdit('parent')">
                                {{ selectedTask.ParentTaskID ? getTitleTask(selectedTask.ParentTaskID) : 'No parent' }}
                            </span>
                            <select v-else-if="isEditingParent" v-model="selectedTask.ParentTaskID"
                                class="form-select form-select-sm d-inline-block w-auto ms-2"
                                @blur="saveField('ParentTaskID', selectedTask.ParentTaskID); disableEdit('parent')"
                                @change="saveField('ParentTaskID', selectedTask.ParentTaskID); disableEdit('parent')">
                                <option :value="null">No parent</option>
                                <option v-for="task in tasks.filter(t => t.TaskID !== selectedTask.TaskID)"
                                    :key="task.TaskID" :value="task.TaskID">
                                    {{ task.Title }}
                                </option>
                            </select>
                            <span v-else class="ms-2">
                                {{ selectedTask.ParentTaskID ? getTitleTask(selectedTask.ParentTaskID) : 'No parent' }}
                            </span>
                        </div>

                        <!-- Due Date -->
                        <div class="mb-2">
                            <strong>Due date:</strong>
                            <span v-if="!isEditingDueDate && canEdit()" class="ms-2" style="cursor: pointer;"
                                @click="enableEdit('dueDate')">
                                {{ formatDate(selectedTask.DueDate) || 'Click to select' }}
                            </span>
                            <input v-else-if="isEditingDueDate" type="date"
                                class="form-control d-inline-block w-auto ms-2"
                                :value="formatDateForInput(selectedTask.DueDate)"
                                @input="selectedTask.DueDate = $event.target.value"
                                @blur="saveField('DueDate', selectedTask.DueDate); disableEdit('dueDate')" />
                            <span v-else class="ms-2">
                                {{ formatDate(selectedTask.DueDate) || '-' }}
                            </span>
                        </div>

                        <!-- Start Date -->
                        <div class="mb-2">
                            <strong>Start date:</strong>
                            <span v-if="!isEditingStartDate && canEdit()" class="ms-2" style="cursor: pointer;"
                                @click="enableEdit('startDate')">
                                {{ formatDate(selectedTask.CreatedAt) || 'Click to select' }}
                            </span>
                            <input v-else-if="isEditingStartDate" type="date"
                                class="form-control d-inline-block w-auto ms-2"
                                :value="formatDateForInput(selectedTask.CreatedAt)"
                                @input="selectedTask.CreatedAt = $event.target.value"
                                @blur="saveField('CreatedAt', selectedTask.CreatedAt); disableEdit('startDate')" />
                            <span v-else class="ms-2">
                                {{ formatDate(selectedTask.CreatedAt) || '-' }}
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>
</template>



<style>
    @import '/src/assets/style/projectpageofuser.css';
    @import '/src/assets/style/boardpage.css';
</style>

<script>
    import UserLayout from "@/components/UserLayout.vue";
    import ProjectLayout from "@/components/ProjectLayout.vue";
    import BoardPage from '@/assets/js/boardpage.js';
    import { QuillEditor } from '@vueup/vue-quill'
    import draggable from 'vuedraggable';
    import Multiselect from 'vue-multiselect'




    export default {
        components: {
            UserLayout,
            ProjectLayout,
            QuillEditor,
            draggable,
            Multiselect
        },
        mixins: [BoardPage],
    };

</script>