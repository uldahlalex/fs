import {ApiCallServiceInterface} from "./apiCallService.interface";
import {ClientWantsToRegister} from "../models/clientWantsToRegister";
import {ClientWantsToEnterRoom} from "../models/clientWantsToEnterRoom";
import {ClientWantsToLeaveRoom} from "../models/clientWantsToLeaveRoom";
import {ClientWantsToLoadOlderMessages} from "../models/clientWantsToLoadOlderMessages";
import {ClientWantsToSendMessageToRoom} from "../models/clientWantsToSendMessageToRoom";
import {ClientWantsToAuthenticate} from "../models/clientWantsToAuthenticate";
import {ClientWantsToAuthenticateWithJwt} from "../models/clientWantsToAuthenticateWithJwt";
import {ServerAuthenticatesUser} from "../models/serverAuthenticatesUser";
import {Injectable} from "@angular/core";

//@Injectable()
export class ApiCallServiceMock implements ApiCallServiceInterface {
    ClientWantsToRegister(clientWantsToRegister: ClientWantsToRegister) {
        let expectedServerEvent = new ServerAuthenticatesUser({jwt: "jwt"});
        localStorage.setItem("jwt", expectedServerEvent.jwt!);
    }

  ClientWantsToAuthenticate(clientWantsToAuthenticate: ClientWantsToAuthenticate): void {
  }

  ClientWantsToAuthenticateWithJwt(clientWantsToAuthenticateWithJwt: ClientWantsToAuthenticateWithJwt): void {
  }

  ClientWantsToEnterRoom(clientWantsToEnterRoom: ClientWantsToEnterRoom): void {
  }

  ClientWantsToLeaveRoom(clientWantsToLeaveRoom: ClientWantsToLeaveRoom): void {
  }

  ClientWantsToLoadOlderMessages(clientWantsToLoadOlderMessages: ClientWantsToLoadOlderMessages): void {
  }

  ClientWantsToSendMessageToRoom(clientWantsToSendMessageToRoom: ClientWantsToSendMessageToRoom): void {
  }


}
