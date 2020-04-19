export interface SpotifyUser {
    country?: string;
    display_name?: string;
    email?: string;
    external_urls?: ExternalUrls;
    followers?: Followers;
    href?: string;
    id?: string;
    images?: Array<Image>;
    product?: string;
    type?: string;
    url?: string;
}

export interface ExternalUrls {
    spotify: string;
}

export interface Followers {
    href: string;
    total: string;
}

export interface Image {
    height: number;
    width: number;
    url: string;
}

export interface Playlist {
    collaborative?: boolean;
    description?: string;
    external_urls?: ExternalUrls;
    followers?: Followers;
    href?: string;
    id?: string;
    images?: Array<Image>;
    name?: string;
    owner?: SpotifyUser;
    public?: boolean;
    snapshot_id?: string;
    tracks?: TrackList;
    type?: "playlist";
    uri?: string;
}

export interface TrackList {
    href: string;
    items: Array<any>;
    limit: number;
    next: any;
    offset: number;
    previous: any;
    total: number;
}