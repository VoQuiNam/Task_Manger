<template>
    <!-- Navbar -->
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
        <div class="container-fluid">
            <a class="navbar-brand" href="#">
                <i class="fa-brands fa-jira fa-2x mx-3 ps-1" style="color: #ffffff;"></i>
            </a>
            <button class="navbar-toggler" type="button" data-mdb-collapse-init
                data-mdb-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false"
                aria-label="Toggle navigation">
                <i class="fas fa-bars"></i>
            </button>
            <div class="collapse navbar-collapse" id="navbarSupportedContent">
                <form class="me-3">
                    <div class="form-white input-group" style="width: 250px;">
                        <input type="search" class="form-control rounded" placeholder="Search or jump to... ( / )"
                            aria-label="Search" aria-describedby="search-addon" />
                    </div>
                </form>
                <ul class="navbar-nav me-auto mb-2 mb-lg-0">
                    <li class="nav-item">
                        <router-link class="nav-link" to="/homepage" exact-active-class="active-link">Home</router-link>
                    </li>
                    <li class="nav-item dropdown" ref="dropdownWrapper">
                        <a class="nav-link dropdown-toggle down_ml" href="#" role="button"
                            @click.prevent="toggleDropdown" :class="{ 'active-link': isProjectRouteActive }">
                            Projects
                        </a>

                        <div class="dropdown-menu show custom-dropdown" v-show="showDropdown" ref="dropdownMenu">
                            <!-- Recent -->
                            <div class="px-3">
                                <small class="text-muted">Recent</small>
                                <div class="mt-1" v-for="project in recentProjects" :key="project.id">
                                    <div class="d-flex align-items-start mt-2">
                                        <div>
                                            <div class="fw-bold small">{{ project.name }}</div>
                                            <div class="text-muted small">Software project</div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- Bottom Links -->
                            <div class="dropdown-divider my-2"></div>
                            <router-link to="/viewallproject" class="dropdown-item">View all projects</router-link>
                            <router-link to="/create-project" class="dropdown-item">Create project</router-link>
                        </div>
                    </li>


                </ul>
                <ul class="navbar-nav d-flex flex-row ms-auto me-3">
                    <!-- lg: áp dụng từ kích thước "large" breakpoint trở lên (≥992px).
                    3: giá trị tương ứng với khoảng cách 1rem (16px)`. -->
                    <!-- Notification icon -->
                    <li class="nav-item me-3 me-lg-3">
                        <a class="nav-link" href="#">
                            <div class="position-relative">
                                <i class="fas fa-bell fa-lg"></i>
                                <span
                                    class="badge rounded-pill bg-danger position-absolute top-0 start-100 translate-middle">
                                    3
                                </span>
                            </div>
                        </a>
                    </li>

                    <li class="nav-item me-3 me-lg-0 dropdown">
                        <a class="nav-link dropdown-toggle" href="#" id="navbarDropdown1" role="button"
                            data-bs-toggle="dropdown" aria-expanded="false">
                            <img src="https://mdbootstrap.com/img/Photos/Avatars/img (31).jpg" class="rounded-circle"
                                height="22" alt="" loading="lazy" />
                        </a>
                        <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="navbarDropdown1">
                            <li><a class="dropdown-item" href="#">Profile</a></li>
                            <li>
                                <hr class="dropdown-divider" />
                            </li>
                            <li>
                                <a class="dropdown-item" @click="logout">Log out</a>
                            </li>
                        </ul>
                    </li>
                </ul>
            </div>
        </div>
    </nav>
    <!-- Navbar -->
</template>


<script>
    export default {
        data() {
            return {
                showDropdown: false,
                recentProjects: [
                    {
                        id: 1,
                        name: "My Project",
                        icon: "https://via.placeholder.com/20x20/0000FF/ffffff?text=P"
                    },
                    {
                        id: 2,
                        name: "Another Project",
                        icon: "https://via.placeholder.com/20x20/008000/ffffff?text=A"
                    }
                ]
            };
        },
        mounted() {
            // Lịch sử trình duyệt
            window.history.pushState(null, '', window.location.href);
            window.onpopstate = () => {
                const isAuthenticated = !!localStorage.getItem('userToken');
                if (!isAuthenticated) {
                    this.$router.replace('/login');
                } else {
                    window.history.pushState(null, '', window.location.href);
                }
            };

            // Lắng nghe click ngoài dropdown để đóng
            document.addEventListener('click', this.handleClickOutside);
        },
        computed: {
            isProjectRouteActive() {
                const path = this.$route.path;
                return path.includes('/viewallproject') || path.includes('/create-project') ||  path.includes('/project_page');
            }
        },
        beforeUnmount() {
            document.removeEventListener('click', this.handleClickOutside);
        },
        methods: {
            toggleDropdown() {
                this.showDropdown = !this.showDropdown;
            },
            handleClickOutside(event) {
                const wrapper = this.$refs.dropdownWrapper;
                if (wrapper && !wrapper.contains(event.target)) {
                    this.showDropdown = false;
                }
            },
            logout() {
                localStorage.removeItem('userToken');
                localStorage.removeItem('userRole');
                this.$router.push('/login');
            }
        }
    };

</script>


<style>
    @import '/src/assets/style/userlayout.css';
</style>