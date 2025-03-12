import { createApp } from 'vue'
import App from './App.vue'  // <-- Đổi sang App.vue
import router from './router'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap'

import { library } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faGoogle, faFacebook } from '@fortawesome/free-brands-svg-icons'
import { faGauge, faUser, faUserCog, faBars } from '@fortawesome/free-solid-svg-icons' 
// Import the functions you need from the SDKs you need
import { initializeApp } from "firebase/app";
// TODO: Add SDKs for Firebase products that you want to use
// https://firebase.google.com/docs/web/setup#available-libraries

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

library.add(faGoogle, faFacebook, faGauge, faUser, faUserCog, faBars);

const app = createApp(App); // <-- Đúng

app.use(router);
app.component('font-awesome-icon', FontAwesomeIcon);
app.mount('#app');
