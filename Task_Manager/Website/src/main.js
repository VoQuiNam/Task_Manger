import { createApp } from 'vue'
import App from './App.vue'  // <-- Đổi sang App.vue
import router from './router'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap'

import { library } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faGoogle, faFacebook } from '@fortawesome/free-brands-svg-icons'
import { faGauge, faUser, faUserCog, faBars, faEdit, faTrash, faBox, faPuzzlePiece, faTags, faSignal, faDiagramProject,
  faBarsProgress
 } from '@fortawesome/free-solid-svg-icons' // Thêm faBox
// Import the functions you need from the SDKs you need
import { initializeApp } from "firebase/app";
// Import vue3-toastify
import Vue3Toastify from 'vue3-toastify'
import 'vue3-toastify/dist/index.css'

import '@fortawesome/fontawesome-free/css/all.css'
import '@fortawesome/fontawesome-free/js/all.js'
import { QuillEditor } from '@vueup/vue-quill'
import '@vueup/vue-quill/dist/vue-quill.snow.css'
import Multiselect from 'vue-multiselect'
import 'vue-multiselect/dist/vue-multiselect.min.css'
// @ts-ignore
import vClickOutside from 'v-click-outside';




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

library.add(
  faGoogle, faFacebook, faGauge, faUser, faUserCog, faBars, faEdit, faTrash, 
  faBox, faPuzzlePiece, faTags, faSignal, faDiagramProject, faBarsProgress
);

const app = createApp(App); // <-- Đúng

// Cấu hình vue3-toastify
app.use(Vue3Toastify, {
  autoClose: 3000, // Đóng thông báo sau 3 giây
  position: 'top-right', // Vị trí hiển thị thông báo
  theme: 'light' // Chế độ sáng (hoặc 'dark')
})

app.use(router);
app.component('font-awesome-icon', FontAwesomeIcon);
app.use(vClickOutside) // nếu ở main.js


app.component('QuillEditor', QuillEditor);
app.component('UiMultiselect', Multiselect);

app.mount('#app');
