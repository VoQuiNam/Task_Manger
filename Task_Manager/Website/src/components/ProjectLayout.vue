<template>
    <div>
        <!-- Main layout -->
        <div class="d-flex pd-top">
            <!-- Sidebar -->
            <div class="bg-light border-end vh-100 side_bar">
                <div class="p-3 overflow-auto height_calc">
                    <div class="d-flex align-items-center mb-3">
                        <img src="https://voquinamit-1744906135412.atlassian.net/rest/api/2/universal_avatar/view/type/project/avatar/10418?size=xxlarge"
                            class="me-2 logo__" alt="project logo" />
                        <div class="lh-sm">
                            <strong>{{ project.Name }}</strong><br />
                            <small class="text-muted">Software project</small>
                        </div>
                    </div>

                    <h6 class="text-uppercase text-muted small">Planning</h6>
                    <ul class="nav flex-column">
                        <li class="nav-item">
                            <router-link class="nav-link nav-link_side nav-link-project" active-class="active"
                                :to="{ path: '/summarypage', query: { projectId: $route.query.projectId } }">
                                <i class="fas fa-globe me-2"></i> Summary
                            </router-link>


                        </li>

                        <li class="nav-item">
                            <router-link class="nav-link nav-link_side nav-link-project" active-class="active"
                                :to="{ path: '/boardpage', query: { projectId: $route.query.projectId } }">
                                <i class="fas fa-columns me-2"></i> Board
                            </router-link>
                        </li>

                        <li class="nav-item">
                            <a class="nav-link nav-link_side" href="#"><i class="fas fa-list me-2"></i> List</a>
                        </li>

                        <li class="nav-item">
                            <router-link class="nav-link nav-link_side nav-link-project" active-class="active"
                                :to="{ path: '/issuetypepage', query: { projectId: $route.query.projectId } }">
                                <i class="fas fa-check-to-slot me-2"></i> Issues
                            </router-link>
                        </li>
                        <li class="nav-item">
                            <router-link class="nav-link nav-link_side nav-link-project" active-class="active"
                                :to="{ path: '/usermanagementpage', query: { projectId: $route.query.projectId } }">
                                <i class="fas fa-user-plus me-2"></i> User
                            </router-link>
                        </li>

                    </ul>
                </div>
            </div>
        </div>
    </div>
</template>


<style>
    @import '/src/assets/style/projectpageofuser.css';
</style>

<script>
    import ViewAllProject from '@/assets/js/viewallproject.js';

    export default {
        //Mixins trong Vue.js là một cách để tái sử dụng logic giữa các component
        mixins: [ViewAllProject],

    };

</script>