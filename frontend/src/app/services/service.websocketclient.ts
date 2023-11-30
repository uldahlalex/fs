import {inject, Injectable} from "@angular/core";
import {BaseTransferObject} from "../models/baseTransferObject";
import {State} from "./service.state";
import {ServerAddsClientToRoom} from "../models/serverAddsClientToRoom";
import {ServerSendsOlderMessagesToClient} from "../models/serverSendsOlderMessagesToClient";
import {ClientWantsToRegister} from "../models/clientWantsToRegister";
import {ClientWantsToAuthenticate} from "../models/clientWantsToAuthenticate";
import {ClientWantsToEnterRoom} from "../models/clientWantsToEnterRoom";
import {ClientWantsToLoadOlderMessages} from "../models/clientWantsToLoadOlderMessages";
import {ServerBroadcastsMessageToClientsInRoom} from "../models/serverBroadcastsMessageToClientsInRoom";
import {ServerAuthenticatesUser} from "../models/serverAuthenticatesUser";
import {ServerNotifiesClientsInRoom} from "../models/serverNotifiesClientsInRoom";
import {ServerSendsErrorMessageToClient} from "../models/serverSendsErrorMessageToClient";
import {ServerBroadcastsTimeSeriesData} from "../models/serverBroadcastsTimeSeriesData";

@Injectable({providedIn: 'root'})
export class WebSocketClientService {

  state = inject(State);
  constructor() {
    this.state.socketConnection.onopen = () => console.info("connection established");
    this.state.socketConnection.onmessage = (event) => {
      var data = JSON.parse(event.data) as BaseTransferObject<any>;

      //@ts-ignore
        this[data.eventType].call(this, data);
    }
  }
  ServerAddsClientToRoom(dto: ServerAddsClientToRoom) {
    this.state.roomsWithMessages.set(dto.roomId!, dto.messages!);
  }

  ServerAuthenticatesUser(dto: ServerAuthenticatesUser) {
    localStorage.setItem("jwt", dto.jwt!);
  }

  ServerBroadcastsMessageMessageToClientsInRoom(dto: ServerBroadcastsMessageToClientsInRoom) {
    console.log(dto)
  }

  ServerNotifiesClientsInRoom(dto: ServerNotifiesClientsInRoom) {
    console.log(dto)
  }

  ServerSendsErrorMessageToClient(dto: ServerSendsErrorMessageToClient) {
    console.log(dto)
  }

  ServerBroadcastsTimeSeriesData(dto: ServerBroadcastsTimeSeriesData) {
    console.log(dto)
  }


  ServerSendsOlderMessagesToClient(serverSendsOlderMessagesToClient: ServerSendsOlderMessagesToClient) {
    this.state.roomsWithMessages.get(serverSendsOlderMessagesToClient.roomId!)!
      .unshift(...serverSendsOlderMessagesToClient.messages?.reverse()!);
  }

}
