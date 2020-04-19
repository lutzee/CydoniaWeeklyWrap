import { SpotifyUser } from "./spotify";

export interface RootState {
    spotify: Spotify;
}

export interface Spotify {
    accessToken: string;
    user: SpotifyUser;
}

export default RootState;