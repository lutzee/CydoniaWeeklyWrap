export interface LastTrack {
    id: any;
    name: string;
    duration: any;
    mbid: string;
    artistName: string;
    artistMbid: string;
    url: string;
    rank: number;
    playCount: number;
    isLoved: boolean;
}

export interface Track {
    id: any;
    trackName: string;
    duration: any;
    mbid: string;
    artistName: string;
    url: string;
    playCount: number;
    spotifyUid: string;
    spotifyUrl: string;
}


export default Track;