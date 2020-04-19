<template>
    <section class="section" id="main-content">
        <div class="container">
            <div class="columns">
                <h1 v-if="!!spotifyUser">Hello {{ spotifyUser }}</h1>
            </div>
            <div class="columns" v-if="hasSpotifyAccessToken">
                <div class="column is-half">
                    <div class="field has-addons">
                        <div class="control">
                            <button class="button" v-on:click="createPlaylist">Create a playlist</button>
                            <a v-if="playlistCreated" :href="playlist.external_urls.spotify">Playlist {{ playlist.name }} created</a>
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
                            <th>Play Count</th>
                            <th>Most Plays</th>
                            <th>Last.fm</th>
                            <th>Spotify</th>
                        </tr>
                    </thead>
                    <tbody id="music-table-body">
                        <tr v-for="(track, index) in tracks" :key="track.mbid">
                            <td>{{ index + 1 }}</td>
                            <td>{{ track.artistName }}</td>
                            <td>{{ track.trackName }}</td>
                            <td>{{ track.playCount }}</td>
                            <td>{{ track.mostListens }}</td>
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
    import { Chance } from 'chance';

    import store from "../store";
    
    import GroupApi from "../api/api.group";
    import SpotifyApi from "../api/api.spotify";

    import Track from "../typings/track";
    import _ from "underscore";
import { Playlist } from "../typings/spotify";

    @Component({
        components: {
            RingLoader
        }
    })
    export default class HomeVue extends Vue {
        public tracks: Array<Track> = [];
        public isLoading: boolean = false;
        public playlistCreated: boolean = false;
        public playlist: Playlist;

        public created(): void {
            this.getCombined();
        }

        public createPlaylist(): void {
            var num = new Chance().integer({ min: 1, max: 1000});
            var playlist = {
                name: `CWW ${num}`
            };

            SpotifyApi.User.createPlaylist(store.state.spotify.user.id, playlist)
                .then(playlist => {
                    this.playlist = playlist.data;
                    this.playlistCreated = true;
                    var filteredTracks = _.filter(this.tracks, track => !!track.spotifyUid);

                    var spotifyUids = _.pluck(filteredTracks, "spotifyUid");
                    var spotifyUris = _.map(spotifyUids, (uid: string) => `spotify:track:${uid}`);
                    var chunkedIds = _.chunk(spotifyUris, 100);

                    _.each(chunkedIds,
                    function (chunk, index) {
                        window.setTimeout(function() {
                                SpotifyApi.User.addTracksToPlaylist(playlist.data.id, chunk);
                            }, 1000*index);
                    });
                });
        }

        get spotifyAccessToken(): string {
            return store.state.spotify.accessToken;
        }

        get hasSpotifyAccessToken(): boolean {
            return !!this.spotifyAccessToken;
        }

        get spotifyUser(): string {
            return store.state.spotify.user.display_name;
        }

        public getCombined(): void {
            this.isLoading = true;
            GroupApi.Get.get()
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

