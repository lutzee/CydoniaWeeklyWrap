<template>
    <nav class="navbar is-dark is-spaced" role="navigation" aria-label="main navigation">
        <div class="navbar-brand">
            <h1>Cydonia Weekly Wrap</h1>
            <a role="button" id="main-nav-burger" class="navbar-burger" data-target="main-nav" aria-label="menu" aria-expanded="false">
                <span aria-hidden="true"></span>
                <span aria-hidden="true"></span>
                <span aria-hidden="true"></span>
            </a>
        </div>
        <div id="main-nav" class="navbar-menu">
            <div class="navbar-end">
                <router-link to="/" id="home" class="navbar-item">Home</router-link>
                <router-link to="/user" id="user" class="navbar-item">User</router-link>
                <a class="navbar-item" v-show="spotifyRequiresAuth" :href="spotifyAuthUrl">Spotify Login</a>
            </div>
        </div>
    </nav>
</template>

<script lang="ts">
    import { Component, Vue } from "vue-property-decorator";

    import { Chance } from "chance";
    
    import store from "../store";
    import SpotifyApi from "../api/api.spotify";
import { SpotifyUser } from "../typings/spotify";

    @Component
    export default class NavigationVue extends Vue {
        public spotifyAuthUrl: string = "";
        
        constructor() {
            super();
            document.addEventListener("DOMContentLoaded", () => {
                // Check if there are any navbar burgers
                const nav = document.getElementById("main-nav-burger");
                if (nav) {
                    const target = nav.dataset.target;
                    const $target = document.getElementById(<string>target);
                    nav.addEventListener("click", () => {
                        // Get the target from the "data-target" attribute
                        if ($target) {
                            // Toggle the "is-active" class on both the "navbar-burger" and the "navbar-menu"
                            nav.classList.toggle("is-active");
                            $target.classList.toggle("is-active");
                        }
                    });
                }
            });
        }

        public created(): void {
            var baseUrl = "https://accounts.spotify.com/authorize"
            //var redirectUrl = "http://localhost:8080";
            var redirectUrl = "https://cww.lutzee.net";
            var clientId = "9e19e049167e49949cfbf0074816991d";
            var scopes: Array<string> = [
                "playlist-read-collaborative",
                "playlist-modify-public",
                "playlist-read-private",
                "playlist-modify-private",
                "user-library-modify",
                "user-library-read",
                "user-follow-read",
                "user-follow-modify",
                "user-read-email",
                "user-read-private"
            ];

            var responseType = "token";
            var chance = new Chance();
            var state = chance.string({ length: 16, alpha: true});
            var scopesString = scopes.join("%20");
            this.spotifyAuthUrl = `${baseUrl}?client_id=${clientId}&response_type=${responseType}&redirect_uri=${redirectUrl}&scopes=${scopesString}&state=${state}`

            store.watch(state => state.spotify.accessToken, () => {
                if(store.state.spotify.accessToken) {
                    SpotifyApi.User.get()
                        .then(user => {
                            store.commit('loadSpotifyUser', user.data);
                        });
                }
            });
        }

        get spotifyUser(): SpotifyUser {
            return store.state.spotify.user;
        }

        get spotifyRequiresAuth(): boolean {
            return !store.state.spotify.user.id;
        }
    }
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="less" scoped>
</style>
