import { AxiosPromise } from "axios";
import Api from "./api";
import Track from "../typings/track";

export module Group {
    var groupApi = Api.apiUrl.append("group");

    export module Get {
        export function get(): AxiosPromise<Array<Track>> {
            return groupApi.get();
        }
    }
}

export default Group;