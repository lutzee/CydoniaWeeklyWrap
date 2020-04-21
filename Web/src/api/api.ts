import axios, { AxiosPromise } from "axios";
import store from "../store";

export module Api {
    export class Url {
        private url: string;
        private headers: any;

        private hasAccessToken: boolean = false;

        constructor(url: string, headers?: any) {
            this.url = url;
            if (headers) { this.headers = headers } else {
                this.headers = {
                    "Cache-Control": "no-cache"
                }
            }
        }

        public setAccessToken(accessToken: string): Url {
            if (!this.hasAccessToken) {
                this.headers["Authorization"] = `Bearer ${accessToken}`;
                this.hasAccessToken = true;
            }
            return this;
        }

        public setSpotifyAccessToken(): Url {
            this.setAccessToken(store.state.spotify.accessToken);
            return this;
        }

        public toString(): string {
            return this.url;
        }

        public append(value: any): Url {
            return new Url(this._append(this.url, value.toString()), this.headers);
        }

        public query(value: any): Url {
            return new Url(this._query(this.url, value.toString()));
        }

        public get(data: any = {}): AxiosPromise<any> {
            return axios.get(this.url, { data: data, headers: this.headers });
        }

        public post(data: any = {}): AxiosPromise<any> {
            return axios.post(this.url, data, { headers: this.headers });
        }

        public put(data: any = {}): AxiosPromise<any> {
            return axios.put(this.url, data, { headers: this.headers });
        }

        public patch(data: any = {}): AxiosPromise<any> {
            return axios.patch(this.url, data, { headers: this.headers });
        }

        public delete(): AxiosPromise<any> {
            return axios.delete(this.url, { headers: this.headers });
        }

        private _query(str1: string, str2: string): string {
            return str1 + "?" + str2;
        }

        private _append(str1: string, str2: string): string {
            // ensure trailing slash is always present
            if (!str2.endsWith("/")) {
                str2 = str2 + "/";
            }

            if (str1.endsWith("/") && str2.startsWith("/")) {
                // Remove leading slash from str2 to prevent double slash
                return str1 + str2.slice(1);
            }

            if (!str1.endsWith("/") && !str2.startsWith("/")) {
                // Neither str1 has a trailing slash, nor str2 has a leading slash -
                // add one in to preserve the intended behaviour.
                return str1 + "/" + str2;
            }

            // Simple concatenation is fine.
            return str1 + str2;
        }
    }

    export var base = "https://cwwapi.lutzee.net";
    //export var base = "https://localhost:44339";
    export var spotifyUrl = new Url("https://api.spotify.com/v1", {}).setSpotifyAccessToken();
    export var apiUrl = new Url(base).append("api/");
}

export default Api;