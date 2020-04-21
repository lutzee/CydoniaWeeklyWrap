<template>
    <section class="section" id="main-content">
        <div class="container">
            <div class="columns">
                <div class="column is-half">
                    <div class="field has-addons">
                        <div class="control">
                            <input v-model="username" v-on:input="onUsernameChange" class="input is-primary" type="text" placeholder="Username">
                        </div>
                    </div>
                </div>
            </div>
            <div class="columns is-centered">
                <div class="column is-half">
                    <ring-loader class="my-loader" :loading="isLoading" v-bind:size="'200px'"></ring-loader>
                </div>
            </div>
            <div>
                <table class="table" v-if="!isLoading">
                    <thead>
                        <tr>
                            <th>#</th>
                            <th>Artist</th>
                            <th>Track</th>
                            <th>Listen Count</th>
                            <th>Last.FM</th>
                            <th>Spotify</th>
                        </tr>
                    </thead>
                    <tbody id="music-table-body">
                        <tr v-for="(track, index) in tracks" :key="track.mbid">
                            <td>{{ index + 1 }}</td>
                            <td>{{ track.artistName }}</td>
                            <td>{{ track.trackName }}</td>
                            <td>{{ track.playCount }}</td>
                            <td><a class="button is-danger" v-bind:href="track.url">Last.fm</a></td>
                            <td><a class="button is-success" v-if="!!track.spotifyUrl" v-bind:href="track.spotifyUrl">Spotify</a></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </section>
</template>

<script lang="ts">
    import { Component, Vue } from "vue-property-decorator";
    import RingLoader from 'vue-spinner/src/RingLoader.vue'

    import UserApi from "../api/api.user";
    import { Track } from "../typings/track";
    import _ from "underscore";

    @Component({
        components: {
            RingLoader
        }
    })
    export default class HomeVue extends Vue {
        public tracks: Array<Track> = [];
        public username: string = "";
        public isLoading: boolean = false;

        public created(): void {
            
        }

        public onUsernameChange(): void {
            if(this.username) {
                this.isLoading = true;
                this.getRecent();
            }
        }

        public getRecent = _.debounce(this.getRecentInternal, 500);

        public getRecentInternal(): void {
            UserApi.Get.recent(this.username)
                            .then(tracks => {
                                this.tracks  = tracks.data;
                                this.isLoading = false;
                            })
            
        }
     }
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="less" scoped>
    .my-loader {
        margin: 0 auto;
        width: 200px;
    }
</style>

