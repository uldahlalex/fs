import {Component, Inject, inject, signal} from "@angular/core";
import {Router} from "@angular/router";
import {NgForOf} from "@angular/common";
import {WebSocketClientService} from "../services/service.websocketclient";
import {API_SERVICE_TOKEN} from "../../main";

@Component({
  template: `
      <h5>Controls</h5>


      <div style="display: flex; flex-direction: column; justify-content: space-between; position: relative;">
          <button (click)="goToLoginPage()">Go to log in</button>
          <button (click)="toggleDialog()">{{ dialogText }}</button>
          <dialog (click)="toggleDialog()"
                  [open]="isOpen"
                  style="box-shadow: 10px 10px 10px lightgray; border: transparent 0px; position: absolute; left: 100px;">
              <h3>Rooms</h3>
              <ul>
                  <li (click)="GoToRoom(room.id)" *ngFor="let room of webSocketClientService.rooms">{{ room.title }}👈</li>
              </ul>

          </dialog>
      </div>
  `,
  selector: "app-sidebar",
})
export class ComponentSidebar {
  router: Router = inject(Router);

  constructor(  @Inject(API_SERVICE_TOKEN)public webSocketClientService: WebSocketClientService
  ) {
  }

  isOpen: boolean = false;
  dialogText: string = "Show rooms";

  toggleDialog() {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.dialogText = "Close popover";
    } else {
      this.dialogText = "Show rooms";
    }
  }

  GoToRoom(roomId: any) {
    this.router.navigate(['/room/'+roomId]);
  }

  goToLoginPage() {
    this.router.navigate(['/login']);
  }
}
