import { AxiosPromise } from "axios";
import Api from "./api";
import { SpotifyUser, Playlist } from "../typings/spotify";

export module Spotify {
    var spotifyUserApi = Api.spotifyUrl.append("users");
    var spotifyPlaylistApi = Api.spotifyUrl.append("playlists");

    export module User {
        export function get(): AxiosPromise<SpotifyUser> {
            return Api.spotifyUrl.append("me").setSpotifyAccessToken().get();
        }

        export function createPlaylist(username: string, playlist: Playlist): AxiosPromise<Playlist> {
            return spotifyUserApi.append(username).append("playlists").post(playlist);
        }

        export function addTracksToPlaylist(playlistId: string, spotifyUids: Array<string>): AxiosPromise<any> {
            return spotifyPlaylistApi.append(playlistId).append("tracks").post(spotifyUids);
        }
    }
}

export default Spotify;