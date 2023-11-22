import {Component, inject} from "@angular/core";
import {State} from "../services/service.state";
import {JsonPipe, NgForOf, NgIf} from "@angular/common";
import {ActivatedRoute} from "@angular/router";
import {FormsModule} from "@angular/forms";
import {WebsockSocketClient} from "../services/service.websocketclient";


@Component({
  template: `

      <h3>Main Content</h3>

      <div style="display: flex; flex-direction: row; align-items:stretch;">
          <input [(ngModel)]="state.input" placeholder="insert some number" style="height: 100%;">
          <button (click)="websocketClient.upstreamSendMessageToRoom(roomId)" style="height: 100%;">insert</button>
      </div>

      <div style="
      display: flex;
      flex-direction: column;
">
          <div *ngIf="roomId">
              <div *ngFor="let k of state.roomsWithMessages.get(roomId)">UID: {{ k.sender }}
                  said {{ k.messageContent }}
                  at {{ k.timestamp }}
              </div>
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
  websocketClient = inject(WebsockSocketClient);

  constructor() {
    this.route.paramMap.subscribe(params => this.enter(Number.parseInt(params.get('id')!)));

  }


  private enter(id: number) {
    this.roomId = id;
    this.websocketClient.clientWantsToEnterRoom(id)
  }

}
