import {Component, inject} from "@angular/core";
import {Router} from "@angular/router";
import {WebSocketClientService} from "../services/service.websocketclient";

@Component({
  template: `
    <div style="display: flex; flex-direction: row; justify-content: start;">
      <h3>Controls</h3>
      <p-chip *ngIf="webSocketClientService.socketConnection.readyState == 1" label="Connection established"></p-chip>
      <p-chip *ngIf="webSocketClientService.socketConnection.readyState != 1" label="No connection!"></p-chip>

    </div>


    <div style="display: flex; flex-direction: row; justify-content: normal; position: relative;">
      <button (click)="goToLoginPage()">Go to log in</button>
      <button (click)="toggleDialog()">{{ dialogText }}</button>

      <p-dialog (click)="toggleDialog()" [(visible)]="isOpen">
        <h3>Rooms</h3>
        <ul>
          <li (click)="GoToRoom(room.id)" *ngFor="let room of webSocketClientService.rooms">{{ room.title }}
            👈
          </li>
        </ul>
      </p-dialog>
      <button (click)="goToTimeSeriesDashboard()">Show time series data</button>

    </div>
  `,
  selector: "app-sidebar",
})
export class ComponentSidebar {
  router: Router = inject(Router);
  isOpen: boolean = false;
  dialogText: string = "Show rooms";

  constructor(public webSocketClientService: WebSocketClientService
  ) {
  }

  toggleDialog() {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.dialogText = "Close popover";
    } else {
      this.dialogText = "Show rooms";
    }
  }

  GoToRoom(roomId: any) {
    this.router.navigate(['/room/' + roomId]);
  }

  goToLoginPage() {
    this.router.navigate(['/login']);
  }

  goToTimeSeriesDashboard() {
    this.router.navigate(['/timeseries']);
  }
}
