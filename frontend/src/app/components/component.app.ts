import {Component} from '@angular/core';
import {ChipModule} from "primeng/chip";
import {WebSocketClientService} from "../services/service.websocketclient";

@Component({
  selector: 'app-root',
  template: `
    <div style="display: flex; justify-content: end;">
      <p-chip *ngIf="webSocketClientService.socketConnection.readyState == 1" label="Connection established"></p-chip>
      <p-chip *ngIf="webSocketClientService.socketConnection.readyState != 1" label="No connection!"></p-chip>
    </div>

    <app-sidebar></app-sidebar>

    <router-outlet></router-outlet>


    <p-toast></p-toast>

  `
})
export class ComponentApp {
  constructor(public webSocketClientService: WebSocketClientService) {
  }
}


