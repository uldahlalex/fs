import {Component, inject} from "@angular/core";
import {State} from "../services/service.state";
import {JsonPipe, NgForOf, NgIf} from "@angular/common";
import {ActivatedRoute} from "@angular/router";
import {FormsModule} from "@angular/forms";
import {WebSocketClientService} from "../services/service.websocketclient";
import ago from 's-ago';
import {ClientWantsToLoadOlderMessages} from "../models/sendMessage";


@Component({
  template: `

      <h3>Main Content</h3>

      <div style="display: flex; flex-direction: row; justify-content: center; ">
          <button (click)="loadOlderMessages()" style="height: 100%;">Load older messages...</button>
      </div>

      <div style="
      display: flex;
      flex-direction: column;
">
          <div *ngIf="roomId">
              <div *ngFor="let k of state.roomsWithMessages.get(roomId)"
                   style="display: flex; flex-direction: row; justify-content: space-between">
                  <div>
                      UID: {{ k.sender }}
                      said <i>{{ k.messageContent }}</i>
                      <b>{{k.id}}</b>
                  </div>
                  <div title="{{fullDate(k.timestamp)}}">written {{ timestampThis(k.timestamp)}}</div>
              </div>

          </div>

          <div style="display: flex; flex-direction: row; justify-content: center;">
              <input [(ngModel)]="state.input" placeholder="Write something interesting" style="height: 100%;">
              <button (click)="websocketClient.upstreamSendMessageToRoom(roomId)" style="height: 100%;">insert</button>
          </div>


      </div>
  `,
  imports: [
    JsonPipe,
    NgForOf,
    FormsModule,
    NgIf
  ],
  standalone: true
})
export class ComponentRoom {

  roomId: number | undefined;
  state = inject(State);
  route = inject(ActivatedRoute);
  websocketClient = inject(WebSocketClientService);

  constructor() {
    this.route.paramMap.subscribe(params => this.enter(Number.parseInt(params.get('id')!)));

  }


  private enter(id: number) {
    this.roomId = id;
    this.websocketClient.clientWantsToEnterRoom(id)
  }


  timestampThis(timestamp: string | undefined) {
    var date = new Date(timestamp!);
    return ago(date);
  }

  fullDate(timestamp: string | undefined) {
    var date = new Date(timestamp!);
    return date.toLocaleString();
  }

  loadOlderMessages() {
    let dto: ClientWantsToLoadOlderMessages = new ClientWantsToLoadOlderMessages({
      roomId: this.roomId,
      lastMessageId: this.state.roomsWithMessages.get(this.roomId!)![0].id
    })
    this.websocketClient.clientWantsToLoadOlderMessages(dto);
  }
}
