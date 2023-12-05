import {ApiCallServiceInterface} from "./apiCallService.interface";
import {ClientWantsToRegister} from "../models/clientWantsToRegister";
import {ServerAddsClientToRoom} from "../models/serverAddsClientToRoom";
import {ServerAuthenticatesUser} from "../models/serverAuthenticatesUser";
import {Injectable} from "@angular/core";

@Injectable()
export class ApiCallServiceMock implements ApiCallServiceInterface {
    ClientWantsToRegister(clientWantsToRegister: ClientWantsToRegister) {
        let expectedServerEvent = new ServerAuthenticatesUser({jwt: "jwt"});
        localStorage.setItem("jwt", expectedServerEvent.jwt!);
    }
}
