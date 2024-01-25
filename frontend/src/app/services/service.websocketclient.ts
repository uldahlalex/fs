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
import {ClientWantsToAuthenticateWithJwt} from "../models/clientWantsToAuthenticateWithJwt";
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


  public socketConnection: WebsocketSuperclass = new WebsocketSuperclass(environment.url);

  constructor(public messageService: MessageService, public router: Router) {
   try {
     //todo global solution
     this.handleEvent()
   } catch (e) {
     console.log(e)
   }


  }
  handleEvent() {
    this.rooms.forEach(room => {
      this.roomsWithMessages.set(room.id!, []);
      this.roomsWithConnections.set(room.id!, 0)
    });
    this.socketConnection.onopen = () => {
      let jwt = localStorage.getItem("jwt");
      if (jwt != null && jwt != '') {
        this.socketConnection.sendDto(new ClientWantsToAuthenticateWithJwt({jwt: jwt!}));
        this.messageService.add({ life: 2000, severity: 'success', summary: 'i am alive', detail: 'Server connection is open!'});

      } else this.router.navigate(['/login']);
    }
    this.socketConnection.onmessage = (event) => {
      let data = JSON.parse(event.data) as BaseDto<any>;
      //@ts-ignore
      this[data.eventType].call(this, data);
    }
    this.socketConnection.onerror = (event) => {
      this.messageService.add({ life: 2000, severity: 'error', summary: 'hello?', detail: 'no connection! Will re-try every 5 seconds...'});
    }
    this.socketConnection.onclose = (event) => {
      this.messageService.add({ life: 2000, severity: 'error', summary: 'hello?', detail: 'no connection! Will re-try every 5 seconds...'});

      this.reestablishConnection();


    }
  }


  reestablishConnection() {
    //todo resource draining stuff
    if(this.socketConnection == undefined || this.socketConnection.readyState != 1) {
      this.messageService.add({key: "connlost", life: 2000, severity: 'error', summary: 'hello?', detail: 'no connection! Will re-try every 5 seconds...'})
      this.socketConnection = new WebsocketSuperclass(environment.url);
      this.handleEvent();
      this.socketConnection.sendDto(new ClientWantsToAuthenticateWithJwt({jwt: localStorage.getItem('jwt')!}));
      setTimeout(() => this.reestablishConnection(), 2000);
    }


    }


  ServerAddsClientToRoom(dto: ServerAddsClientToRoom) {
    this.roomsWithMessages.set(dto.roomId!, dto.messages!.reverse());
    this.roomsWithConnections.set(dto.roomId!, dto.liveConnections!);
    this.messageService.add({life: 2000, severity: 'success', summary: 'Success', detail: 'Welcome to room, '});
  }

  ServerAuthenticatesUser(dto: ServerAuthenticatesUser) {
    this.messageService.add({life: 2000, summary: 'Success', detail: 'Authentication successful!'});
    localStorage.setItem("jwt", dto.jwt!);
    this.router.navigate(['/room/1'])
  }

  ServerBroadcastsMessageToClientsInRoom(dto: ServerBroadcastsMessageToClientsInRoom) {
    this.roomsWithMessages.get(dto.roomId!)!.push(dto.message!);
    this.messageService.add({life: 2000, summary: '📬', detail: 'New message!'});
  }

  ServerNotifiesClientsInRoomSomeoneHasJoinedRoom(dto: ServerNotifiesClientsInRoomSomeoneHasJoinedRoom) {
    this.messageService.add({
      life: 2000,
      severity: 'warning',
      summary: '🧨',
      detail: "New user joined: " + dto.user?.email
    });
    this.roomsWithConnections.set(dto.roomId!, this.roomsWithConnections.get(dto.roomId!)! + 1);
  }

  ServerNotifiesClientsInRoomSomeoneHasLeftRoom(dto: ServerNotifiesClientsInRoomSomeoneHasLeftRoom) {
    this.messageService.add({
      life: 2000,
      severity: 'warning',
      summary: '👋',
      detail: dto.user?.email + " left the room!"
    });
    this.roomsWithConnections.set(dto.roomId!, this.roomsWithConnections.get(dto.roomId!)! - 1);
  }

  ServerSendsErrorMessageToClient(dto: ServerSendsErrorMessageToClient) {
    this.messageService.add({life: 2000, severity: 'error', summary: 'Error', detail: dto.errorMessage});
    if (JSON.parse(dto.receivedMessage!).eventType == 'ClientWantsToAuthenticateWithJwt')
      localStorage.removeItem('jwt');
  }

  ServerRejectsJwt(dto: ServerRejectsJwt) {
    this.messageService.add({life: 2000, severity: 'error', summary: 'Error', detail: "Jwt has been rejected!"});
    localStorage.removeItem('jwt');
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
    this.messageService.add({life: 2000, severity: 'info', summary: '🗑️', detail: "Someone deleted a message from one of your chats!"})
    var messages = this.roomsWithMessages.get(serverDeletesMessage.roomId!)!.filter(message => message.id != serverDeletesMessage.messageId!)
    this.roomsWithMessages.set(serverDeletesMessage.roomId!, messages);
  }


  // CLIENT -> SERVER COMMUNICATION
  //i think ill retire all this boilerplate
  // ClientWantsToRegister(clientWantsToRegister: ClientWantsToRegister): void {
  //   this.socketConnection.send(JSON.stringify(clientWantsToRegister));
  // }
  //
  // ClientWantsToAuthenticate(clientWantsToAuthenticate: ClientWantsToAuthenticate): void {
  //   this.socketConnection.send(JSON.stringify(clientWantsToAuthenticate));
  // }
  //
  // ClientWantsToAuthenticateWithJwt(clientWantsToAuthenticateWithJwt: ClientWantsToAuthenticateWithJwt): void {
  //   this.socketConnection.send(JSON.stringify(clientWantsToAuthenticateWithJwt));
  // }
  //
  // ClientWantsToEnterRoom(clientWantsToEnterRoom: ClientWantsToEnterRoom): void {
  //   this.socketConnection.send(JSON.stringify(clientWantsToEnterRoom));
  // }
  //
  // ClientWantsToLeaveRoom(clientWantsToLeaveRoom: ClientWantsToLeaveRoom): void {
  //   this.socketConnection.send(JSON.stringify(clientWantsToLeaveRoom));
  // }
  //
  // ClientWantsToLoadOlderMessages(clientWantsToLoadOlderMessages: ClientWantsToLoadOlderMessages): void {
  //   this.socketConnection.send(JSON.stringify(clientWantsToLoadOlderMessages));
  // }
  //
  // ClientWantsToSendMessageToRoom(clientWantsToSendMessageToRoom: ClientWantsToSendMessageToRoom): void {
  //   this.socketConnection.send(JSON.stringify(clientWantsToSendMessageToRoom));
  // }
  //
  // ClientWantsToSubscribeToTimeSeriesData(clientWantsToSubscribeToTimeSeriesData: ClientWantsToRegister): void {
  //   {
  //     this.socketConnection.send(JSON.stringify(clientWantsToSubscribeToTimeSeriesData));
  //   }
  // }
}
