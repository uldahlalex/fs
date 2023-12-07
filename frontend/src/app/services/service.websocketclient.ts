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
import {
  ServerNotifiesClientsInRoom,
  ServerNotifiesClientsInRoomSomeoneHasJoinedRoom,
  ServerNotifiesClientsInRoomSomeoneHasLeftRoom
} from "../models/serverNotifiesClientsInRoom";
import {ServerSendsErrorMessageToClient} from "../models/serverSendsErrorMessageToClient";
import {ServerBroadcastsTimeSeriesData} from "../models/serverBroadcastsTimeSeriesData";
import {Message, Room} from "../models/entities";
import {ClientWantsToAuthenticateWithJwt} from "../models/clientWantsToAuthenticateWithJwt";
import {ApiCallServiceInterface} from "./apiCallService.interface";
import {ClientWantsToSendMessageToRoom} from "../models/clientWantsToSendMessageToRoom";
import {ClientWantsToLeaveRoom} from "../models/clientWantsToLeaveRoom";
import {MessageService} from "primeng/api";

@Injectable()
export class WebSocketClientService implements ApiCallServiceInterface {


  public roomsWithMessages: Map<number, Message[]> = new Map<number, Message[]>();
  public roomsWithConnections: Map<number, number> = new Map<number, number>();
  public rooms: Room[] = [{id: 1, title: "Work stuff"}, {id: 2, title: "Casual conversations"}, {
    id: 3,
    title: "Sports"
  }];
  public timeseriesData: any[] = [];
  private socketConnection: WebSocket = new WebSocket(`ws://localhost:8181`);

  constructor(public messageService: MessageService) {
    this.rooms.forEach(room => {
      this.roomsWithMessages.set(room.id!, []);
      this.roomsWithConnections.set(room.id!, 0)
    });
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
    this.socketConnection.onerror = (event) => {
      this.messageService.add({life: 2000, severity: 'error', summary: 'Error', detail: 'Connection error!'});
    }
    this.socketConnection.onclose = (event) => {
      this.messageService.add({life: 2000, severity: 'error', summary: 'Error', detail: 'Connection closed!'});
    }
  }

  ServerAddsClientToRoom(dto: ServerAddsClientToRoom) {
    this.roomsWithMessages.set(dto.roomId!, dto.messages!.reverse());
    this.roomsWithConnections.set(dto.roomId!, dto.liveConnections!)
  }

  ServerAuthenticatesUser(dto: ServerAuthenticatesUser) {
    this.messageService.add({life: 2000, summary: 'Success', detail: 'Authentication successful!'});
    localStorage.setItem("jwt", dto.jwt!);

  }

  ServerBroadcastsMessageToClientsInRoom(dto: ServerBroadcastsMessageToClientsInRoom) {
    this.roomsWithMessages.get(dto.roomId!)!.push(dto.message!);
    this.messageService.add({life: 2000, summary: '📬', detail: 'New message!'});
  }

  ServerNotifiesClientsInRoomSomeoneHasJoinedRoom(dto: ServerNotifiesClientsInRoomSomeoneHasJoinedRoom) {
    this.messageService.add({life: 2000, severity: 'warning', summary: '🧨', detail: dto.message});
    this.roomsWithConnections.set(dto.roomId!, this.roomsWithConnections.get(dto.roomId!)! + 1);
  }

  ServerNotifiesClientsInRoomSomeoneHasLeftRoom(dto: ServerNotifiesClientsInRoomSomeoneHasLeftRoom) {
    this.messageService.add({life: 2000, severity: 'warning', summary: '👋', detail: dto.message});
    this.roomsWithConnections.set(dto.roomId!, this.roomsWithConnections.get(dto.roomId!)! - 1);
  }

  ServerNotifiesClientsInRoom(dto: ServerNotifiesClientsInRoom) {
    this.messageService.add({life: 2000, severity: 'info', summary: 'Info!', detail: dto.message});

  }

  ServerSendsErrorMessageToClient(dto: ServerSendsErrorMessageToClient) {
    this.messageService.add({life: 2000, severity: 'error', summary: 'Error', detail: dto.errorMessage});
  }

  ServerBroadcastsTimeSeriesData(dto: ServerBroadcastsTimeSeriesData) {
    this.messageService.add({life: 2000, severity: 'info', summary: '📈', detail: "New time series data!"});
    this.timeseriesData.push(dto.timeSeriesData!);
  }


  ServerSendsOlderMessagesToClient(serverSendsOlderMessagesToClient: ServerSendsOlderMessagesToClient) {
    this.roomsWithMessages.get(serverSendsOlderMessagesToClient.roomId!)!
      .unshift(...serverSendsOlderMessagesToClient.messages?.reverse()!);
  }



  // CLIENT -> SERVER COMMUNICATION

  ClientWantsToRegister(clientWantsToRegister: ClientWantsToRegister): void {
    this.socketConnection.send(JSON.stringify(clientWantsToRegister));
  }

  ClientWantsToAuthenticate(clientWantsToAuthenticate: ClientWantsToAuthenticate): void {
    this.socketConnection.send(JSON.stringify(clientWantsToAuthenticate));
  }

  ClientWantsToAuthenticateWithJwt(clientWantsToAuthenticateWithJwt: ClientWantsToAuthenticateWithJwt): void {
    this.socketConnection.send(JSON.stringify(clientWantsToAuthenticateWithJwt));
  }

  ClientWantsToEnterRoom(clientWantsToEnterRoom: ClientWantsToEnterRoom): void {
    this.socketConnection.send(JSON.stringify(clientWantsToEnterRoom));
  }

  ClientWantsToLeaveRoom(clientWantsToLeaveRoom: ClientWantsToLeaveRoom): void {
    this.socketConnection.send(JSON.stringify(clientWantsToLeaveRoom));
  }

  ClientWantsToLoadOlderMessages(clientWantsToLoadOlderMessages: ClientWantsToLoadOlderMessages): void {
    this.socketConnection.send(JSON.stringify(clientWantsToLoadOlderMessages));
  }

  ClientWantsToSendMessageToRoom(clientWantsToSendMessageToRoom: ClientWantsToSendMessageToRoom): void {
    this.socketConnection.send(JSON.stringify(clientWantsToSendMessageToRoom));
  }

  ClientWantsToSubscribeToTimeSeriesData(clientWantsToSubscribeToTimeSeriesData: ClientWantsToRegister): void {
    {
      this.socketConnection.send(JSON.stringify(clientWantsToSubscribeToTimeSeriesData));
    }
  }
}
