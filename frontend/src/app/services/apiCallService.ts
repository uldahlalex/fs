import {ApiCallServiceInterface} from "./apiCallService.interface";
import {ClientWantsToRegister} from "../models/clientWantsToRegister";
import {Inject, inject, Injectable} from "@angular/core";
import {WebSocketClientService} from "./service.websocketclient";

@Injectable()
export class ApiCallService implements ApiCallServiceInterface{

  websocketClient = inject(WebSocketClientService);

  ClientWantsToRegister(clientWantsToRegister: ClientWantsToRegister): void {
    this.websocketClient.socketConnection.send(JSON.stringify(clientWantsToRegister));
  }

}
