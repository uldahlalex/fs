import {Injectable} from "@angular/core";
import {BaseTransferObject} from "../models/baseTransferObject";
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
import {Message, Room} from "../models/entities";
import {ClientWantsToAuthenticateWithJwt} from "../models/clientWantsToAuthenticateWithJwt";
import {ApiCallServiceInterface} from "./apiCallService.interface";
import {ClientWantsToSendMessageToRoom} from "../models/clientWantsToSendMessageToRoom";
import {ClientWantsToLeaveRoom} from "../models/clientWantsToLeaveRoom";

@Injectable()
export class WebSocketClientService implements ApiCallServiceInterface {


  private socketConnection: WebSocket = new WebSocket(`ws://localhost:8181`);
  public roomsWithMessages: Map<number, Message[]> = new Map<number, Message[]>();
  public rooms: Room[] = [{id: 1, title: "Work stuff"}, {id: 2, title: "Casual conversations"}, {
    id: 3,
    title: "Sports"
  }];

  constructor() {
    this.rooms.forEach(room => this.roomsWithMessages.set(room.id!, []));
    this.socketConnection.onopen = () => {
      let jwt = localStorage.getItem("jwt");
      if (jwt != '') {
        this.socketConnection.send(JSON.stringify(new ClientWantsToAuthenticateWithJwt({jwt: jwt!})));
      }
      console.info("connection established");
    }
    this.socketConnection.onmessage = (event) => {
      let data = JSON.parse(event.data) as BaseTransferObject<any>;

      //@ts-ignore
      this[data.eventType].call(this, data);
    }
  }

  ServerAddsClientToRoom(dto: ServerAddsClientToRoom) {
    this.roomsWithMessages.set(dto.roomId!, dto.messages!);
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
    this.roomsWithMessages.get(serverSendsOlderMessagesToClient.roomId!)!
      .unshift(...serverSendsOlderMessagesToClient.messages?.reverse()!);
  }


  ClientWantsToRegister(clientWantsToRegister: ClientWantsToRegister): void {
    this.socketConnection.send(JSON.stringify(clientWantsToRegister));
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
