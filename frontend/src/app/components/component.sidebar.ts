import {Component, inject, signal} from "@angular/core";
import {Router} from "@angular/router";
import {State} from "../services/service.state";
import {NgForOf} from "@angular/common";

@Component({
  template: `
      <h5>Controls</h5>


      <div style="display: flex; flex-direction: column; justify-content: space-between; position: relative;">
          <button style="background-color: transparent; color: black" (click)="goToLoginPage()">Go to log in</button>
          <button (click)="toggleDialog()">{{ dialogText }}</button>
          <dialog (click)="toggleDialog()"
                  [open]="isOpen"
                  style="box-shadow: 10px 10px 10px lightgray; border: transparent 0px; position: absolute; left: 100px;">
              <h3>Rooms</h3>
              <ul>
                  <li (click)="GoToRoom(room.id)" *ngFor="let room of service.rooms">{{ room.title }}👈</li>
              </ul>

          </dialog>
      </div>
  `,
  selector: "app-sidebar",
  imports: [
    NgForOf
  ],
  standalone: true
})
export class ComponentSidebar {

  service: State = inject(State);
  router: Router = inject(Router);

  isOpen: boolean = false;
  dialogText: string = "Open Dialog";

  toggleDialog() {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.dialogText = "Close Dialog";
    } else {
      this.dialogText = "Open Dialog";
    }
  }

  GoToRoom(roomId: any) {
    this.router.navigate(['/room/'+roomId]);
  }

  goToLoginPage() {
    this.router.navigate(['/login']);
  }
}
