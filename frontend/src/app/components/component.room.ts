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
      <div *ngIf="roomId">
        <div *ngFor="let k of webSocketClientService.roomsWithMessages.get(roomId)"
             style="display: flex; flex-direction: row; justify-content: space-between;">

          <div><b>{{ k.email }}</b> says:<br>
            <div
              style="display: inline-block; margin: 10px; padding: 10px; border-radius: 25px;  background: #f3bce6; color: #000000;  max-width: 80%;">

              {{ k.messageContent }}
              <button (click)="DeleteMessage(k.id)">Delete</button>
            </div>
          </div>
          <i title="{{fullDate(k.timestamp)}}">written {{ timestampThis(k.timestamp) }}</i>
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
  protected readonly localStorage = localStorage;

  constructor(public webSocketClientService: WebSocketClientService
  ) {
    this.route.paramMap.subscribe(params => {
      this.roomId = Number.parseInt(params.get('id')!)
      if (this.webSocketClientService.roomsWithConnections.get(this.roomId) == 0) this.enterRoom();
    });
  }

  async enterRoom() {
    console.log("entering")
    try {
      let clientWantsToEnterRoom = new ClientWantsToEnterRoom({roomId: this.roomId});
      this.webSocketClientService.socketConnection.sendDto(clientWantsToEnterRoom);
    } catch (e) {
      console.log("connection not established, retrying in 1 second")
      await new Promise(resolve => setTimeout(resolve, 1000));
      this.enterRoom();
    }
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
    this.webSocketClientService.socketConnection.sendDto(new ClientWantsToDeleteMessage({messageId: id, roomId: this.roomId}));
  }
}
