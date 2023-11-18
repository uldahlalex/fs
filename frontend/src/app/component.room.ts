import {Component, OnInit} from "@angular/core";
import {DataContainer} from "./service.datacontainer";
import {JsonPipe, NgForOf, NgIf} from "@angular/common";
import {ActivatedRoute} from "@angular/router";
import {FormsModule} from "@angular/forms";

@Component({
  template: `

      <h3>Main Content</h3>

      <div style="display: flex; flex-direction: row; align-items:stretch;">
          <input [(ngModel)]="service.input" placeholder="insert some number" style="height: 100%;">
          <button (click)="service.upstreamAddMessage(null)" style="height: 100%;">insert</button>
      </div>

      <div style="
      display: flex;
      flex-direction: column;
" *ngIf="service.roomsWithMessages!=undefined">
          <div *ngFor="let i of service.roomsWithMessages.keys()">
              <div *ngFor="let k of service.roomsWithMessages.get(i)">UID: {{k.sender}} said {{k.messageContent}} at {{k.timestamp}}</div>
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
export class ComponentRoom implements OnInit{



  constructor(public service: DataContainer, public route: ActivatedRoute) {
this.route.paramMap.subscribe(params => this.enter(params.get('id')));

  }

  ngOnInit() {


  }



  private enter(id: string | null) {
    console.log("entering room", id);
    this.service.upstreamEnterRoom(id)
  }
}
