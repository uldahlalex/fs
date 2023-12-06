import {ClientWantsToRegister} from "../models/clientWantsToRegister";
import {ClientWantsToSendMessageToRoom} from "../models/clientWantsToSendMessageToRoom";
import {ClientWantsToLoadOlderMessages} from "../models/clientWantsToLoadOlderMessages";
import {ClientWantsToLeaveRoom} from "../models/clientWantsToLeaveRoom";
import {ClientWantsToEnterRoom} from "../models/clientWantsToEnterRoom";
import {ClientWantsToAuthenticateWithJwt} from "../models/clientWantsToAuthenticateWithJwt";
import {ClientWantsToAuthenticate} from "../models/clientWantsToAuthenticate";

export interface ApiCallServiceInterface {
  ClientWantsToAuthenticate: (clientWantsToAuthenticate: ClientWantsToAuthenticate) => void;
  ClientWantsToAuthenticateWithJwt: (clientWantsToAuthenticateWithJwt: ClientWantsToAuthenticateWithJwt) => void;
  ClientWantsToEnterRoom: (clientWantsToEnterRoom: ClientWantsToEnterRoom) => void;
  ClientWantsToLeaveRoom: (clientWantsToLeaveRoom: ClientWantsToLeaveRoom) => void;
  ClientWantsToLoadOlderMessages: (clientWantsToLoadOlderMessages: ClientWantsToLoadOlderMessages) => void;
  ClientWantsToRegister: (clientWantsToRegister: ClientWantsToRegister) => void;
  ClientWantsToSendMessageToRoom: (clientWantsToSendMessageToRoom: ClientWantsToSendMessageToRoom) => void;
}

