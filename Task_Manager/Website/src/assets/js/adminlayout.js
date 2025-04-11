import axios from "axios"; // Import axios

export default {
  data() {
    return {
      isCollapsed: JSON.parse(localStorage.getItem("isCollapsed")) || false,
      activeIndex: 0,
      menuItems: [], // Lấy từ API,
      canCreateModule: true
    };
  },
  async mounted() {
    await this.fetchModules(); // Gọi API khi component được mount
    // await this.fetchRoleModule();
    this.updateActiveIndex();

    window.history.pushState(null, "", window.location.href);
    window.onpopstate = () => {
      const isAuthenticated = !!localStorage.getItem("userToken");
      if (!isAuthenticated) {
        this.$router.replace("/login");
      } else {
        window.history.pushState(null, "", window.location.href);
      }
    };
  },
  watch: {
    $route() {
      this.updateActiveIndex();
    }
  },
  methods: {
    async fetchModules() {
      try {
        console.log("Fetching modules...");
        const response = await axios.get("http://localhost:5260/api/modules/GetModules");
        console.log(this.menuItems);
        // Chuyển đổi dữ liệu API thành menuItems
        this.menuItems = response.data
          .map(module => ({
            label: module.ModuleName,
            path: module.Link || "/",
            icon: module.Icon ? ['fas', module.Icon] : ['fas', 'circle'], // Mặc định icon nếu thiếu
            order: module.OrderNumber || 999
          }))
          .sort((a, b) => a.order - b.order); // Sắp xếp theo thứ tự hiển thị

        this.updateActiveIndex();
      } catch (error) {
        console.error("Lỗi khi lấy dữ liệu modules:", error);
      }
    },
    resolvePath(path) {
      return path.startsWith("/") ? path : `/dashboard/${path}`;
    },
    toggleSidebar() {
      this.isCollapsed = !this.isCollapsed;
      localStorage.setItem("isCollapsed", JSON.stringify(this.isCollapsed));
    },
    setActive(index) {
      this.activeIndex = index;
    },
    updateActiveIndex() {
      const currentPath = this.$route.path;
      const foundIndex = this.menuItems.findIndex(item => item.path === currentPath);
      if (foundIndex !== -1) {
        this.activeIndex = foundIndex;
      }
    },
    logout() {
      localStorage.removeItem("userToken");
      localStorage.removeItem("userRole");
      this.$router.push("/login");
    }
  }
};