import Vue from "vue";
import Router from "vue-router";

Vue.use(Router);

const router = new Router({
    mode: 'history',
    routes: [
        {
            path: '/',
            name: 'home',
            component: () => import('./components/home.vue')
        },
        {
            path: '/user',
            name: 'user',
            component: () => import('./components/user.vue')
        }
    ]
});

export default router;