import { createApp } from 'vue'
import App from './App.vue'  // <-- Đổi sang App.vue
import router from './router'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap'

import { library } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faGoogle, faFacebook } from '@fortawesome/free-brands-svg-icons'

library.add(faGoogle, faFacebook)

const app = createApp(App); // <-- Đúng

app.use(router);
app.component('font-awesome-icon', FontAwesomeIcon);
app.mount('#app');
