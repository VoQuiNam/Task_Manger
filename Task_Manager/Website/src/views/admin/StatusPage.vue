<template>
    <div class="d-flex">
        <!-- Sidebar -->
        <div class="admin-layout">
            <AdminLayout />
        </div>

        <!-- Main Content -->
        <div class="content-container flex-grow-1 p-3">
            <div class="header__">
                <h1 class="mb-3 user-title">Status Page</h1>

                <!-- Add User Button -->
                <button type="button" class="btn btn-primary mb-3 btn_user user-title" @click="openModal(null)">
                    Add Status
                </button>

                <!-- Search Bar -->
                <div class="d-flex mb-2 float-end">
                    <input type="text" class="form-control w-100 search__" placeholder="Search..."
                        v-model="searchQuery" />
                </div>
            </div>


            <!-- User Table -->
            <div class="table-responsive d-flex justify-content-center">
                <table class="table table-striped table-mobile-responsive table-mobile-sided">
                    <thead class="table-light">
                        <tr>
                            <th>Name</th>
                            <th>ColorCode</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="status in paginatedStatuses" :key="status.StatusID">
                            <td data-content="Name">{{ status.Name }}</td>
                            <td data-content="ColorCode">{{ status.ColorCode }}</td>
                            <td data-content="Actions">
                                <button class="btn btn-sm btn-outline-primary me-2" @click="openModal(status)">
                                    <font-awesome-icon icon="edit" />
                                </button>
                                <button class="btn btn-sm btn-outline-danger" @click="deleteStatus(status.StatusID)">
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

        <!-- Add User Modal -->
        <div class="modal fade" id="addStatusModal" tabindex="-1" aria-labelledby="addStatusModalLabel"
            aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="addStatusModalLabel">
                            {{ isEditing ? "Edit Status" : "Add New Status" }}
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>

                    <div class="modal-body">
                        <form @submit.prevent="handleSubmit">
                            <div class="row">
                                <div class="col-md-6 mb-3">
                                    <label for="Name" class="form-label float-start">Name:</label>
                                    <input type="text" v-model="newStatus.Name" class="form-control" id="Name" />
                                </div>

                                <div class="col-md-6 mb-3">
                                    <label for="ColorCode" class="form-label float-start">Color Code:</label>
                                    <input type="color" v-model="newStatus.ColorCode"
                                        class="form-control form-control-color w-100" id="ColorCode"
                                        title="Choose your color" />
                                </div>
                            </div>

                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                                <button type="submit" class="btn btn-primary">
                                    {{ isEditing ? "Save Changes" : "Add Label" }}
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
    import StatusPage from '@/assets/js/statuspage.js';

    export default {
        components: {
            AdminLayout,
        },
        //Mixins trong Vue.js là một cách để tái sử dụng logic giữa các component
        mixins: [StatusPage],
    };

</script>

<style>
    @import '/src/assets/style/rolepage.css';
</style>