import Vuex, { StoreOptions } from "vuex";
import _ from "underscore";
import * as store from "store";

import { RootState } from "./typings/state";
import { SpotifyUser } from "typings/spotify";

const vstore: StoreOptions<RootState> = {
    state: {
       spotify: {
            accessToken: "",
            user: {}
       }
    },
    mutations: {
        loadSpotifyAccessToken(state: RootState, accessToken: string) 
        {
            store.set('spotify_access_token', accessToken);
            state.spotify.accessToken = accessToken;
        },
        loadSpotifyUser(state: RootState, user: SpotifyUser) {
            state.spotify.user = user;
        }
    }
};

export default new Vuex.Store(vstore);