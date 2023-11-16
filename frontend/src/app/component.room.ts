import {Component, Inject} from "@angular/core";
import {DataContainer} from "./service.datacontainer";
import {JsonPipe, NgForOf} from "@angular/common";
import {ActivatedRoute} from "@angular/router";
import {FormsModule} from "@angular/forms";

@Component({
  template: `

      <h3>Main Content</h3>

      <div style="display: flex; flex-direction: row; align-items:stretch;">
          <input [(ngModel)]="service.input" placeholder="insert some number" style="height: 100%;">
          <button (click)="service.pushToItems(roomId)" style="height: 100%;">insert</button>
      </div>

      <div style="
      display: flex;
      flex-direction: column;
">
          <div *ngFor="let i of service.items">
              {{ i | json }}
          </div>

      </div>
  `,
  imports: [
    JsonPipe,
    NgForOf,
    FormsModule
  ],
  standalone: true
})
export class ComponentRoom {

  roomId = this.route.snapshot.params['id'];

  constructor(public service: DataContainer, public route: ActivatedRoute) {
    this.service.enterRoom(this.roomId, 1)
  }
}
