import { createApp } from 'vue'
import App from './App.vue'  // <-- Đổi sang App.vue
import router from './router'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap'

import { library } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faGoogle, faFacebook } from '@fortawesome/free-brands-svg-icons'
import { faGauge, faUser, faUserCog, faBars, faEdit, faTrash, faBox, faPuzzlePiece } from '@fortawesome/free-solid-svg-icons' // Thêm faBox
// Import the functions you need from the SDKs you need
import { initializeApp } from "firebase/app";
// Import vue3-toastify
import Vue3Toastify from 'vue3-toastify'
import 'vue3-toastify/dist/index.css'

// Your web app's Firebase configuration
const firebaseConfig = {
  apiKey: "AIzaSyAMFpxWQsUFeFl9BjFR01kZKJ8OhmbVjvg",
  authDomain: "vuefirebaseauth-63fce.firebaseapp.com",
  projectId: "vuefirebaseauth-63fce",
  storageBucket: "vuefirebaseauth-63fce.firebasestorage.app",
  messagingSenderId: "545487206193",
  appId: "1:545487206193:web:7c1ffe740b1e2d912c2210"
};

// Initialize Firebase
initializeApp(firebaseConfig);

library.add(faGoogle, faFacebook, faGauge, faUser, faUserCog, faBars, faEdit, faTrash, faBox, faPuzzlePiece); // Thêm faBox vào library

const app = createApp(App); // <-- Đúng

// Cấu hình vue3-toastify
app.use(Vue3Toastify, {
  autoClose: 3000, // Đóng thông báo sau 3 giây
  position: 'top-right', // Vị trí hiển thị thông báo
  theme: 'light' // Chế độ sáng (hoặc 'dark')
})

app.use(router);
app.component('font-awesome-icon', FontAwesomeIcon);
app.mount('#app');
