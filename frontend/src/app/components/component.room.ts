import {Component, inject} from "@angular/core";
import {ActivatedRoute} from "@angular/router";
import {FormControl} from "@angular/forms";
import {WebSocketClientService} from "../services/service.websocketclient";
import ago from 's-ago';

import {ClientWantsToLoadOlderMessages} from "../models/clientWantsToLoadOlderMessages";
import {ClientWantsToEnterRoom} from "../models/clientWantsToEnterRoom";
import {ClientWantsToSendMessageToRoom} from "../models/clientWantsToSendMessageToRoom";
import {ClientWantsToDeleteMessage} from "../models/clientWantsToDeleteMessage";

@Component({
  template: `

    <h3>Main Content</h3>
    <div style="display: flex; flex-direction: row; justify-content: space-between; ">
      <button (click)="loadOlderMessages()" style="height: 100%;">Load older messages...</button>
      <div>Currently live in room: {{ webSocketClientService.roomsWithConnections.get(roomId!) }}</div>
    </div>
    <div style="
      display: flex;
      flex-direction: column;
      overflow-y: scroll; max-height: 300px;
">
      <div *ngFor="let k of webSocketClientService.roomsWithMessages.get(roomId!)"
           [ngStyle]="jwtDecoded.email == k.email ? {'justify-content': 'flex-start'} : {'justify-content': 'flex-end'}"
           style="display: flex; flex-direction: row;">

        <div>
          <i title="{{fullDate(k.timestamp)}}"> {{ timestampThis(k.timestamp) }}</i>
          <br>
          <b *ngIf="jwtDecoded.email != k.email">{{ k.email }} says:</b>
          <b *ngIf="jwtDecoded.email == k.email">You:</b>

          <br>
          <div [ngStyle]="jwtDecoded.email == k.email ? {'background-color': '#2eb4ea'} : {'background-color': 'grey'} "
               style="position: relative; display: flex; margin: 15px; padding: 5px; border-radius: 25px; color: #000000; max-width: 100%;">

            <button style="position: absolute; top: 0; right: 0; color: black; background: transparent; border: none;"
                    (click)="DeleteMessage(k.id)">🚮
            </button>
            <p>{{ k.messageContent }}</p>

          </div>
        </div>
      </div>
    </div>

    <div style="display: flex; flex-direction: row; justify-content: center;">
      <input [formControl]="messageInput" placeholder="Write something interesting" style="height: 100%;">
      <button (click)="clientWantsToSendMessageToRoom()" style="height: 100%;">insert</button>
    </div>


  `,
})
export class ComponentRoom {

  messageInput = new FormControl('');
  roomId: number | undefined;
  route = inject(ActivatedRoute);
  jwtDecoded;

  constructor(public webSocketClientService: WebSocketClientService
  ) {
    this.jwtDecoded = this.parseJwt(localStorage.getItem("jwt")!);
    console.log(this.jwtDecoded)
    console.log(this.jwtDecoded.email)
    this.route.paramMap.subscribe(params => {
      this.roomId = Number.parseInt(params.get('id')!)
      if (this.webSocketClientService.roomsWithConnections.get(this.roomId) == 0) this.enterRoom();
    });
  }

  parseJwt(token: string) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    return JSON.parse(jsonPayload);
  }


  async enterRoom() {
    let clientWantsToEnterRoom = new ClientWantsToEnterRoom({roomId: this.roomId});
    this.webSocketClientService.socketConnection.sendDto(clientWantsToEnterRoom);

  }

  timestampThis(timestamp: string | undefined) {
    return ago(new Date(timestamp!));
  }

  fullDate(timestamp: string | undefined) {
    return new Date(timestamp!).toLocaleString()

  }

  loadOlderMessages() {
    let dto: ClientWantsToLoadOlderMessages = new ClientWantsToLoadOlderMessages({
      roomId: this.roomId,
      lastMessageId: this.webSocketClientService.roomsWithMessages.get(this.roomId!)![0].id
    })
    this.webSocketClientService.socketConnection.sendDto(dto);
  }

  clientWantsToSendMessageToRoom() {
    let dto = new ClientWantsToSendMessageToRoom({messageContent: this.messageInput.value!, roomId: this.roomId});
    this.webSocketClientService.socketConnection.sendDto(dto);
  }

  DeleteMessage(id: number | undefined) {
    this.webSocketClientService.socketConnection.sendDto(new ClientWantsToDeleteMessage({
      messageId: id,
      roomId: this.roomId
    }));
  }


}
