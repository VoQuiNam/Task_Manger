import { createRouter, createWebHistory } from 'vue-router'
import LoginPage from '@/views/LoginPage.vue';
import RegisterPage from '@/views/RegisterPage.vue';
import Dashboard from '@/views/admin/Dashboard.vue';
import Home from '@/views//user/Home.vue';

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
        component: Dashboard
    },
    {
        path: '/user',
        name: 'user',
        component: Home
    },
]



const router = createRouter({
    history: createWebHistory(),
    routes
})

export default router
