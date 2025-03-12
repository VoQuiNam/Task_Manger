<template>
    <AdminLayout class="admin-layout" />
    <div class="col py-3">
        Content area...
    </div>
</template>

<script>
    import AdminLayout from "@/components/AdminLayout.vue";
    export default {
        components: {
            AdminLayout,
        },
    };
</script>

<style scoped>
    @import '/src/assets/style/dashboard.css';
</style>