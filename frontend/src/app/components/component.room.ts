import {Component, Inject, inject} from "@angular/core";
import {JsonPipe, NgForOf, NgIf} from "@angular/common";
import {ActivatedRoute} from "@angular/router";
import {FormControl, FormsModule, ReactiveFormsModule} from "@angular/forms";
import {WebSocketClientService} from "../services/service.websocketclient";
import ago from 's-ago';

import {ClientWantsToLoadOlderMessages} from "../models/clientWantsToLoadOlderMessages";
import {ClientWantsToEnterRoom} from "../models/clientWantsToEnterRoom";
import {ClientWantsToSendMessageToRoom} from "../models/clientWantsToSendMessageToRoom";
import {API_SERVICE_TOKEN} from "../app.module";


@Component({
  template: `

      <h3>Main Content</h3>

      <div style="display: flex; flex-direction: row; justify-content: center; ">
          <button (click)="loadOlderMessages()" style="height: 100%;">Load older messages...</button>
          <p>Currently live in room: {{ webSocketClientService.roomsWithConnections.get(roomId!) }}</p>
      </div>

      <div style="
      display: flex;
      flex-direction: column;
      overflow-y: scroll; max-height: 50%;
">
          <div *ngIf="roomId">
              <div *ngFor="let k of webSocketClientService.roomsWithMessages.get(roomId)"
                   style="display: flex; flex-direction: row; justify-content: space-between;">
                  <div>
                      UID: {{ k.sender }}
                      said <i>{{ k.messageContent }}</i>
                      <b>{{ k.id }}</b>
                  </div>
                  <div title="{{fullDate(k.timestamp)}}">written {{ timestampThis(k.timestamp) }}</div>
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

  constructor( @Inject(API_SERVICE_TOKEN) public webSocketClientService: WebSocketClientService
  ) {
    this.route.paramMap.subscribe(params => {
      this.roomId = Number.parseInt(params.get('id')!)
      if(this.webSocketClientService.roomsWithConnections.get(this.roomId)==0) this.enterRoom();
    } );
  }


   async enterRoom() {
     console.log("entering")
    try {
      let clientWantsToEnterRoom =  new ClientWantsToEnterRoom({roomId: this.roomId});
      this.webSocketClientService.ClientWantsToEnterRoom(clientWantsToEnterRoom);
    } catch (e) {
      console.log("connection not established, retrying in 1 second")
      await new Promise(resolve => setTimeout(resolve, 1000));
      this.enterRoom();
    }
  }


  timestampThis(timestamp: string | undefined) {
      return ago(new Date(timestamp!)  );
  }

  fullDate(timestamp: string | undefined) {
    return new Date(timestamp!).toLocaleString()

  }

  loadOlderMessages() {
    let dto: ClientWantsToLoadOlderMessages = new ClientWantsToLoadOlderMessages({
      roomId: this.roomId,
      lastMessageId: this.webSocketClientService.roomsWithMessages.get(this.roomId!)![0].id
    })
    this.webSocketClientService.ClientWantsToLoadOlderMessages(dto);
  }

  clientWantsToSendMessageToRoom() {
    let dto = new ClientWantsToSendMessageToRoom({messageContent: this.messageInput.value!, roomId: this.roomId});
    this.webSocketClientService.ClientWantsToSendMessageToRoom(dto);
  }
}
