import Vue from "vue";
import Vuex from "vuex";

Vue.use(Vuex);

import App from "./App.vue";
import router from "./router";
import store from "./store";
import * as str from "store";
import { Hash } from "./typings/browser";
import "bulma";

import "@fortawesome/fontawesome-free/js/fontawesome"
import { faAt, faCode, faCodeBranch, faChevronDown } from "@fortawesome/free-solid-svg-icons";
import { faTwitter, faFacebook, faGithub, faLinkedin } from "@fortawesome/free-brands-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";

import { library } from "@fortawesome/fontawesome-svg-core";
library.add(faAt, faTwitter, faFacebook, faGithub, faCode, faCodeBranch, faLinkedin, faChevronDown);

Vue.component('font-awesome-icon', FontAwesomeIcon);

Vue.config.productionTip = true;

new Vue({
    render: (h: any) => h(App),
    router
}).$mount("#app");

var initialHash: Hash = {};
const hash: any = window.location.hash
    .substring(1)
    .split("&")
    .reduce((initial: Hash, item) => 
    {
        if (item) {
            var parts = item.split("=");
            initial[parts[0]] = decodeURIComponent(parts[1]);
        }
        return initial;
    }, initialHash);

var accessTokenFromLocalStorage = str.get('spotify_access_token');
if (hash.access_token) {
    store.commit('loadSpotifyAccessToken', hash.access_token);
} else if (accessTokenFromLocalStorage) {
    store.commit('loadSpotifyAccessToken', accessTokenFromLocalStorage);
}