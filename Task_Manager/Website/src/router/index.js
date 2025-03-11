import { createRouter, createWebHistory } from 'vue-router'
import LoginPage from '@/views/LoginPage.vue';
import RegisterPage from '@/views/RegisterPage.vue';
import DashboardPage from '@/views/admin/DashboardPage.vue';
import HomePage from '@/views/user/HomePage.vue';

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
        meta: { requiresAuth: true } // 🔒 Đánh dấu cần đăng nhập
    },
    {
        path: '/user',
        name: 'user',
        component: HomePage,
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
