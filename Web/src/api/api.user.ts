import { AxiosPromise } from "axios";
import Api from "./api";
import Track from "../typings/track";

export module User {
    var userApi = Api.apiUrl.append("user");

    export module Get {
        export function recent(user: string): AxiosPromise<Array<Track>> {
            return userApi.append(user).append("recent").get();
        }
    }
}

export default User;