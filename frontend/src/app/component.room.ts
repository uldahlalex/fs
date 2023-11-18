import {Component} from "@angular/core";
import {DataContainer} from "./service.datacontainer";
import {JsonPipe, NgForOf, NgIf} from "@angular/common";
import {ActivatedRoute} from "@angular/router";
import {FormsModule} from "@angular/forms";


@Component({
  template: `

      <h3>Main Content</h3>

      <div style="display: flex; flex-direction: row; align-items:stretch;">
          <input [(ngModel)]="service.input" placeholder="insert some number" style="height: 100%;">
          <button (click)="service.upstreamSendMessageToRoom(null)" style="height: 100%;">insert</button>
      </div>

      <div style="
      display: flex;
      flex-direction: column;
">
          <div *ngIf="roomId">
              <div *ngFor="let k of service.roomsWithMessages.get(roomId)">UID: {{ k.sender }}
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

  constructor(public service: DataContainer, public route: ActivatedRoute) {
    this.route.paramMap.subscribe(params => this.enter(Number.parseInt(params.get('id')!)));

  }


  private enter(id: number) {
    this.roomId = id;
    this.service.upstreamEnterRoom(id)
  }

}
