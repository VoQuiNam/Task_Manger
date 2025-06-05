<template>
    <div>
        <div class="nav-bar">
            <UserLayout />
        </div>

        <ProjectLayout />

        <div class="flex-grow-1 p-4 ml-250">
            <h3>Issue Types</h3>

            <button class="btn btn-primary mb-3" @click="openModalIssue(null)" v-if="currentUserRole !== 'Viewer'">Add
                Issue
                Type</button>

            <table class="table table-bordered align-middle">
                <thead class="table-light">
                    <tr>
                        <th style="width: 40px;">#</th>
                        <th>List of Issue Types</th>
                        <th style="width: 60px;" v-if="currentUserRole !== 'Viewer'">Delete</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="(issue, index) in issueTypes" :key="issue.TypeID">
                        <td class="text-center">{{ index + 1 }}</td>
                        <td>
                            <strong>{{ issue.Name }}</strong>
                        </td>
                        <td class="text-center" v-if="currentUserRole !== 'Viewer'">
                            <button class="btn btn-sm btn-outline-danger" @click="deleteIssue(issue.TypeID)">
                                <i class="fas fa-trash"></i>
                            </button>
                        </td>
                    </tr>
                </tbody>
            </table>

            <p class="text-muted small">
                Use the button above to create a new issue type. Each type helps categorize issues more effectively.
            </p>

        </div>
        <div class="modal fade" id="addIssueModal" tabindex="-1" aria-labelledby="addIssueModalLabel"
            aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="addRoleModalLabel">
                            {{ isEditing ? "Edit Issue" : "Add New Issue" }}
                        </h5>

                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <form @submit.prevent="handleSubmitIssue">
                            <div class="mb-3">
                                <label for="Name" class="col-form-label">Name:</label>
                                <input type="text" class="form-control" id="Name" v-model="newIssue.Name">
                            </div>

                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                                <button type="submit" class="btn btn-primary">
                                    {{ isEditing ? "Save Changes" : "Add Issue" }}
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
    import UserLayout from "@/components/UserLayout.vue";
    import ProjectLayout from "@/components/ProjectLayout.vue";
    import BoardPage from '@/assets/js/boardpage.js';
    import IssuePage from '@/assets/js/issuepage.js';


    export default {
        components: {
            UserLayout,
            ProjectLayout
        },
        mixins: [IssuePage, BoardPage],

    };
</script>