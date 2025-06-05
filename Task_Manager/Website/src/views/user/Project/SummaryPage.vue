<template>
    <div>
        <div class="nav-bar">
            <UserLayout />
        </div>

        <ProjectLayout />

        <div class="flex-grow-1 p-4 ml-250">
            <div class="container">
                <h2 class="mb-4">Summary</h2>

                <!-- Top stats -->
                <div class="row mb-4 text-center">
                    <div class="col-md-3">
                        <div class="card shadow-sm p-3">
                            <div class="text-success fw-bold">1 completed</div>
                            <div class="text-muted">in the last 7 days</div>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="card shadow-sm p-3">
                            <div class="fw-bold">5 updated</div>
                            <div class="text-muted">in the last 7 days</div>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="card shadow-sm p-3">
                            <div class="fw-bold">5 created</div>
                            <div class="text-muted">in the last 7 days</div>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="card shadow-sm p-3">
                            <div class="text-warning fw-bold">1 due soon</div>
                            <div class="text-muted">in the next 7 days</div>
                        </div>
                    </div>
                </div>

                <!-- Chart + Activity -->
                <div class="row">
                    <div class="col-md-6">
                        <div class="card p-4 h-100" style="min-height: 320px;">
                            <h5>Status overview</h5>
                            <p class="text-muted">Get a snapshot of the status of your work items. 
                                <a href="#">View all work items</a>
                            </p>
                            <div class="text-center">
                                <canvas id="statusChart" style="max-width: 300px;"></canvas>
                                <p class="mt-2">5 Total work items</p>
                            </div>
                        </div>
                    </div>
                
                    <div class="col-md-6">
                        <div class="card p-4 h-100" style="min-height: 320px;">
                            <h5>Recent activity</h5>
                            <p class="text-muted">Stay up to date with what's happening across the project.</p>
                            <div style="max-height: 500px; overflow-y: auto;">

                                <ul class="list-unstyled">
                                    <li class="mb-3">
                                        <span class="fw-bold">Qui Nam</span> created 
                                        <span class="badge bg-primary">PT-7: xong</span>
                                        <span class="badge bg-success ms-1">DONE</span>
                                        <div class="text-muted small">7 days ago</div>
                                    </li>
                                    <li class="mb-3">
                                        <span class="fw-bold">Qui Nam</span> updated field "issuetype" on 
                                        <span class="badge bg-danger">PT-6: qwqeq</span>
                                        <span class="badge bg-info text-dark ms-1">IN PROGRESS</span>
                                        <div class="text-muted small">7 days ago</div>
                                    </li>
                                    <li class="mb-3">
                                        <span class="fw-bold">Qui Nam</span> updated field "status" on 
                                        <span class="badge bg-danger">PT-6: qwqeq</span>
                                        <span class="badge bg-info text-dark ms-1">IN PROGRESS</span>
                                        <div class="text-muted small">7 days ago</div>
                                    </li>
                                    <li class="mb-3">
                                        <span class="fw-bold">Qui Nam</span> updated field "status" on 
                                        <span class="badge bg-danger">PT-6: qwqeq</span>
                                        <span class="badge bg-info text-dark ms-1">IN PROGRESS</span>
                                        <div class="text-muted small">7 days ago</div>
                                    </li>
                                    <li class="mb-3">
                                        <span class="fw-bold">Qui Nam</span> updated field "status" on 
                                        <span class="badge bg-danger">PT-6: qwqeq</span>
                                        <span class="badge bg-info text-dark ms-1">IN PROGRESS</span>
                                        <div class="text-muted small">7 days ago</div>
                                    </li>
                                    <li class="mb-3">
                                        <span class="fw-bold">Qui Nam</span> updated field "status" on 
                                        <span class="badge bg-danger">PT-6: qwqeq</span>
                                        <span class="badge bg-info text-dark ms-1">IN PROGRESS</span>
                                        <div class="text-muted small">7 days ago</div>
                                    </li>
                                    <li class="mb-3">
                                        <span class="fw-bold">Qui Nam</span> updated field "status" on 
                                        <span class="badge bg-danger">PT-6: qwqeq</span>
                                        <span class="badge bg-info text-dark ms-1">IN PROGRESS</span>
                                        <div class="text-muted small">7 days ago</div>
                                    </li>
                                    <li class="mb-3">
                                        <span class="fw-bold">Qui Nam</span> updated field "status" on 
                                        <span class="badge bg-danger">PT-6: qwqeq</span>
                                        <span class="badge bg-info text-dark ms-1">IN PROGRESS</span>
                                        <div class="text-muted small">7 days ago</div>
                                    </li>
                                </ul>
                            </div>
                        </div>
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
    import UserLayout from "@/components/UserLayout.vue";
    import ProjectLayout from "@/components/ProjectLayout.vue";
    import Summary from '@/assets/js/summary.js';
    

    export default {
        components: {
            UserLayout,
            ProjectLayout
        },
        mixins: [Summary],

    };

</script>