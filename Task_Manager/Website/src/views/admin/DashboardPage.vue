<template>
    <div class="login-container">
        Đây là trang của admin
    </div>
    <button @click="logout">Logout</button>

</template>

<script>
    export default {
        mounted() {
            window.history.pushState(null, '', window.location.href);
            window.onpopstate = () => {
                const isAuthenticated = !!localStorage.getItem('userToken'); // Kiểm tra token
                if (!isAuthenticated) {
                    this.$router.replace('/login'); // Dùng replace để tránh thêm trang vào lịch sử
                } else {
                    window.history.pushState(null, '', window.location.href);
                }
            };
        },
        methods: {
            logout() {
                localStorage.removeItem('userToken'); // Xóa token
                localStorage.removeItem('userRole');  // Xóa quyền user
                this.$router.push('/login'); // Chuyển hướng về trang đăng nhập
            }
        }
    };
</script>

<style>
 
</style>