import { createRouter, createWebHistory } from 'vue-router'
import LoginPage from '@/views/LoginPage.vue';
import RegisterPage from '@/views/RegisterPage.vue';
import DashboardPage from '@/views/admin/DashboardPage.vue';
import UserPage from '@/views/admin/UserPage.vue';
import RolePage from '@/views/admin/RolePage.vue';
import ModulePage from '@/views/admin/ModulePage.vue';
import RoleModulePage from '@/views/admin/RoleModulePage.vue';
import LabelPage from '@/views/admin/LabelPage.vue';
import StatusPage from '@/views/admin/StatusPage.vue';
import ProjectPage from '@/views/admin/ProjectPage.vue';
import TaskPage from '@/views/admin/TaskPage.vue';
import HomePage from '@/views/user/HomePage.vue';
import SummaryPage from '@/views/user/Project/SummaryPage.vue';
import ViewAllProject from '@/views/user/Project/ViewAllProject.vue';
import BoardPage from '@/views/user/Project/BoardPage.vue';
import IssueType from '@/views/user/Project/IssueType.vue';
import UserManagementPage from '@/views/user/Project/UserManagementPage.vue';
import TaskList from '@/views/user/Project/TaskList.vue';
const routes = [
    {
        path: '/',
        redirect: '/login' // <-- Tự động chuyển hướng sang /login
    },
    {
        path: '/login',
        name: 'login',
        component: LoginPage
    },
    {
        path: '/register',
        name: 'register',
        component: RegisterPage
    },
    {
        path: '/dashboard',
        name: 'dashboard',
        component: DashboardPage,
        meta: { requiresAuth: true },// 🔒 Đánh dấu cần đăng nhập
    },
    {
        path: '/userpage',
        name: 'userpage',
        component: UserPage,
        meta: { requiresAuth: true }
    },
    {
        path: '/rolepage',
        name: 'rolepage',
        component: RolePage,
        meta: { requiresAuth: true }
    },
    {
        path: '/modulepage',
        name: 'modulepage',
        component: ModulePage,
        meta: { requiresAuth: true }
    },
    {
        path: '/rolemodulepage',
        name: 'rolemodulepage',
        component: RoleModulePage,
        meta: { requiresAuth: true } 
    },
    {
        path: '/labelpage',
        name: 'labelpage',
        component: LabelPage,
        meta: { requiresAuth: true } 
    },
    {
        path: '/statuspage',
        name: 'statuspage',
        component: StatusPage,
        meta: { requiresAuth: true } 
    },
    {
        path: '/projectpage',
        name: 'projectpage',
        component: ProjectPage,
        meta: { requiresAuth: true } 
    },
    {
        path: '/taskpage',
        name: 'taskpage',
        component: TaskPage,
        meta: { requiresAuth: true } 
    },
    {
        path: '/homepage',
        name: 'homepage',
        component: HomePage,
        meta: { requiresAuth: true } 
    },
    {
        path: '/summarypage',
        name: 'summarypage',
        component: SummaryPage,
        meta: { requiresAuth: true } 
    },
    {
        path: '/viewallproject',
        name: 'viewallproject',
        component: ViewAllProject,
        meta: { requiresAuth: true } 
    },
    {
        path: '/boardpage',
        name: 'boardpage',
        component: BoardPage,
        meta: { requiresAuth: true } 
    },
    {
        path: '/issuetypepage',
        name: 'issuetypepage',
        component: IssueType,
        meta: { requiresAuth: true } 
    },
    {
        path: '/usermanagementpage',
        name: 'usermanagementpage',
        component: UserManagementPage,
        meta: { requiresAuth: true } 
    },
    {
        path: '/tasklistpage',
        name: 'tasklistpage',
        component: TaskList,
        meta: { requiresAuth: true } 
    },


]



const router = createRouter({
    history: createWebHistory(),
    routes
});

// 🚀 Chặn truy cập nếu chưa đăng nhập
router.beforeEach((to, from, next) => {
    const isAuthenticated = !!localStorage.getItem('userToken'); // Kiểm tra token

    if (to.meta.requiresAuth && !isAuthenticated) {
        next('/login'); // Nếu chưa đăng nhập, quay về login
    } else {
        next(); // Nếu đã đăng nhập, tiếp tục
    }
});

export default router
