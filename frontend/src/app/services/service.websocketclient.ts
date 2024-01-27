import {Injectable} from "@angular/core";
import {BaseDto} from "../models/baseDto";
import {ServerAddsClientToRoom} from "../models/serverAddsClientToRoom";
import {ServerSendsOlderMessagesToClient} from "../models/serverSendsOlderMessagesToClient";
import {ServerBroadcastsMessageToClientsInRoom} from "../models/serverBroadcastsMessageToClientsInRoom";
import {ServerAuthenticatesUser} from "../models/serverAuthenticatesUser";
import {ServerDeletesMessage} from "../models/serverDeletesMessage";
import {ServerNotifiesClientsInRoomSomeoneHasLeftRoom} from "../models/serverNotifiesClientsInRoomSomeoneHasLeftRoom";
import {ServerSendsErrorMessageToClient} from "../models/serverSendsErrorMessageToClient";
import {ServerBroadcastsTimeSeriesData} from "../models/serverBroadcastsTimeSeriesData";
import {Message, Room, TimeSeries, TimeSeriesApexChartData} from "../models/entities";
import {MessageService} from "primeng/api";
import {ServerSendsOlderTimeSeriesDataToClient} from "../models/serverSendsOlderTimeSeriesDataToClient";
import {ApexAxisChartSeries, ApexNonAxisChartSeries} from "ng-apexcharts";
import {Router} from "@angular/router";
import {ServerRejectsJwt} from "../models/serverRejectsJwt";
import {WebsocketSuperclass} from "../models/WebsocketSuperclass";
import {
  ServerNotifiesClientsInRoomSomeoneHasJoinedRoom
} from "../models/serverNotifiesClientsInRoomSomeoneHasJoinedRoom";
import {environment} from "../../environments/environment";


@Injectable({providedIn: 'root'})
export class WebSocketClientService {


  public roomsWithMessages: Map<number, Message[]> = new Map<number, Message[]>();
  public roomsWithConnections: Map<number, number> = new Map<number, number>();
  public rooms: Room[] = [{id: 1, title: "Work stuff"}, {id: 2, title: "Casual conversations"}, {
    id: 3,
    title: "Sports"
  }];

  public series: ApexAxisChartSeries | ApexNonAxisChartSeries = [{
    name: "Timeseries",
    data: []
  }];


  public socketConnection: WebsocketSuperclass;

  constructor(public messageService: MessageService,
              public router: Router) {
    this.socketConnection = new WebsocketSuperclass(environment.url);
    this.rooms.forEach(room => {
      this.roomsWithMessages.set(room.id!, []);
      this.roomsWithConnections.set(room.id!, 0)
    });
    this.handleEvent()
  }

  handleEvent() {
    this.socketConnection.onmessage = (event) => {
      const data = JSON.parse(event.data) as BaseDto<any>;
      console.log("Received: " + JSON.stringify(data));
      //@ts-ignore
      this[data.eventType].call(this, data);
    }
  }


  ServerAddsClientToRoom(dto: ServerAddsClientToRoom) {
    this.roomsWithMessages.set(dto.roomId!, dto.messages!.reverse());
    this.roomsWithConnections.set(dto.roomId!, dto.liveConnections!);
  }

  ServerAuthenticatesUser(dto: ServerAuthenticatesUser) {
    this.messageService.add({life: 2000,  detail: 'Authentication successful!'});
    localStorage.setItem("jwt", dto.jwt!);
    this.router.navigate(['/room/1'])
  }

  ServerAuthenticatesUserFromJwt(dto: ServerAuthenticatesUser) {
    this.messageService.add({life: 2000, summary: 'success', detail: 'Authentication successful!'});
    this.router.navigate(['/room/1'])
  }

  ServerBroadcastsMessageToClientsInRoom(dto: ServerBroadcastsMessageToClientsInRoom) {
    this.roomsWithMessages.get(dto.roomId!)!.push(dto.message!);
    this.messageService.add({life: 2000, summary: '📬', detail: 'New message!'});
  }

  ServerNotifiesClientsInRoomSomeoneHasJoinedRoom(dto: ServerNotifiesClientsInRoomSomeoneHasJoinedRoom) {
    this.messageService.add({
      life: 2000,
      summary: '🧨',
      detail: "New user joined: " + dto.user?.email
    });
    this.roomsWithConnections.set(dto.roomId!, this.roomsWithConnections.get(dto.roomId!)! + 1);
  }

  ServerNotifiesClientsInRoomSomeoneHasLeftRoom(dto: ServerNotifiesClientsInRoomSomeoneHasLeftRoom) {
    this.messageService.add({
      life: 2000,
      summary: '👋',
      detail: dto.user?.email + " left the room!"
    });
    this.roomsWithConnections.set(dto.roomId!, this.roomsWithConnections.get(dto.roomId!)! - 1);
  }

  ServerSendsErrorMessageToClient(dto: ServerSendsErrorMessageToClient) {
    this.messageService.add({life: 2000, severity: 'error', summary: '⚠️', detail: dto.errorMessage}); //todo implement with err handler
  }

  ServerRejectsJwt(dto: ServerRejectsJwt) {
    this.messageService.add({life: 2000, severity: 'error', summary: 'Error', detail: "Jwt has been rejected!"});
    localStorage.removeItem('jwt'); //todo
  }

  ServerSendsOlderMessagesToClient(serverSendsOlderMessagesToClient: ServerSendsOlderMessagesToClient) {
    this.roomsWithMessages.get(serverSendsOlderMessagesToClient.roomId!)!
      .unshift(...serverSendsOlderMessagesToClient.messages?.reverse()!);
  }

  ServerBroadcastsTimeSeriesData(dto: ServerBroadcastsTimeSeriesData) {
    this.messageService.add({life: 2000, severity: 'info', summary: '📈', detail: "New time series data!"});
    let transform: TimeSeriesApexChartData = {
      x: new Date(dto.timeSeriesDataPoint?.timestamp!),
      y: dto.timeSeriesDataPoint?.datapoint
    }
    this.series = [{
      name: "Timeseries",
      // @ts-ignore
      data: this.series[0].data.concat(transform)
    }];
  }

  ServerSendsOlderTimeSeriesDataToClient(serverSendsOlderTimeSeriesDataToClient: ServerSendsOlderTimeSeriesDataToClient) {
    this.messageService.add({life: 2000, severity: 'info', summary: '📈', detail: "Heeeeere's the data!"});
    let transformation = serverSendsOlderTimeSeriesDataToClient.timeseries.map((ts: TimeSeries) => ({
      x: ts.timestamp!,
      y: ts.datapoint
    }));
    this.series = [{
      name: "Timeseries",
      data: transformation
    }];

  }

  ServerDeletesMessage(serverDeletesMessage: ServerDeletesMessage) {
    this.messageService.add({
      life: 2000,
      severity: 'info',
      summary: '🗑️',
      detail: "Someone deleted a message from one of your chats!"
    })
    const messages = this.roomsWithMessages.get(serverDeletesMessage.roomId!)!.filter(message => message.id != serverDeletesMessage.messageId!)
    this.roomsWithMessages.set(serverDeletesMessage.roomId!, messages);
  }


}
