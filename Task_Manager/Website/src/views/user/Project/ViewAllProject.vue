<template>
    <div>
        <UserLayout />

        <!-- Main content -->
        <div class="flex-grow-1 p-4">
            <div class="container-fluid">
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h3 class="mb-0">Projects</h3>
                    <div>
                        <button class="btn btn-primary me-2" @click="openModal(null)">Create project</button>
                    </div>
                </div>

                <div class="row mb-3">
                    <div class="col-md-6 mb-2">
                        <input type="text" class="form-control" placeholder="Search Projects" v-model="searchQuery"/>
                    </div>

                </div>

                <table class="table table-hover align-middle">
                    <thead class="table-light">
                        <tr>
                            <th scope="col"><i class="bi bi-star"></i></th>
                            <th scope="col">Name</th>
                            <th scope="col">Description</th>
                            <th scope="col">Lead</th>
                            <th scope="col">Created at</th>
                            <th scope="col">More actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="project in paginatedProjects" :key="project.ProjectID">
                            <td><i class="bi bi-star-fill text-secondary"></i></td>
                            <td>
                                <img src="https://voquinamit-1744906135412.atlassian.net/rest/api/2/universal_avatar/view/type/project/avatar/10418?size=medium"
                                    class="img_icon me-2" />
                                <router-link :to="`/summarypage?projectId=${project.ProjectID}`"
                                    class="text-decoration-none">
                                    {{ project.Name }}
                                </router-link>

                            </td>
                            <td>{{ project.Description }}</td>
                            <td>{{ getUserName(project.CreatedBy) }}</td>
                            <td>{{ formatDate(project.CreatedAt) }}</td>
                            <td class="position-relative">
                                <div class="dropdown">
                                    <i class="fas fa-ellipsis ms-5" id="dropdownMenuButton" data-bs-toggle="dropdown"
                                        aria-expanded="false" role="button"></i>
                                    <ul class="dropdown-menu" aria-labelledby="dropdownMenuButton">
                                        <li><a class="dropdown-item" href="#">Edit</a></li>
                                        <li><a class="dropdown-item text-danger" href="#">Delete</a></li>
                                    </ul>
                                </div>
                            </td>
                        </tr>

                    </tbody>
                </table>

                <div class="row">
                    <div class="col-12">
                        <!-- Pagination -->
                        <nav class="">
                            <ul class="pagination justify-content-start">
                                <!-- Nút Previous -->
                                <li class="page-item" :class="{ disabled: currentPage === 1 }">
                                    <a class="page-link" href="#"
                                        @click.prevent="goToPage(currentPage - 1)">Previous</a>
                                </li>

                                <!-- Hiển thị số trang -->
                                <li class="page-item" v-for="page in totalPages" :key="page"
                                    :class="{ active: currentPage === page }">
                                    <a class="page-link" href="#" @click.prevent="goToPage(page)">{{ page }}</a>
                                </li>

                                <!-- Nút Next -->
                                <li class="page-item" :class="{ disabled: currentPage === totalPages }">
                                    <a class="page-link" href="#" @click.prevent="goToPage(currentPage + 1)">Next</a>
                                </li>
                            </ul>
                        </nav>
                    </div>
                </div>

            </div>
        </div>

        <!-- Modal Body -->
        <div class="modal fade" id="addProjectModal" tabindex="-1" aria-labelledby="addProjectModalLabel"
            aria-hidden="true">
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
                                <input type="text" class="form-control" id="Description"
                                    v-model="newProject.Description">
                            </div>
                            <!-- Hidden field (optional) -->
                            <input type="hidden" v-model="newProject.CreatedBy" />
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


<style>
    @import '/src/assets/style/projectpageofuser.css';
</style>

<script>
    import UserLayout from "@/components/UserLayout";
    import ViewAllProject from '@/assets/js/viewallproject.js';


    export default {
        components: {
            UserLayout,
        },
        //Mixins trong Vue.js là một cách để tái sử dụng logic giữa các component
        mixins: [ViewAllProject],
    };

</script>