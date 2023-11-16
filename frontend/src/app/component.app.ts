import {Component, inject} from '@angular/core';
import {CommonModule} from '@angular/common';
import {Router, RouterOutlet} from '@angular/router';
import {FormsModule} from "@angular/forms";
import {DataContainer} from "./service.datacontainer";
import {firstValueFrom} from "rxjs";
import {HttpClient, HttpClientModule, HttpSentEvent} from "@angular/common/http";
import {Room} from "./types";


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, FormsModule, HttpClientModule],
  template: `
      <div style="display: flex; height: 100vh;">
          <div style="flex: 0 0 20%; background: #f8f9fa; margin-right: 10px;">
              <h3>Controls</h3>


              <div style="display: flex; flex-direction: row; position: relative;">
                  <button (click)="openDialog()">{{ dialogText }}</button>
                  <dialog (click)="openDialog()"
                          [open]="isOpen"
                          style="box-shadow: 10px 10px 10px lightgray; border: transparent 0px; position: absolute; left: 100px;">
                      <h3>Rooms</h3>
                      <ul>
                          <li (click)="GoToRoom(1)">Room 1👈</li>
                          <li>Room 2</li>
                      </ul>

                  </dialog>
              </div>
          </div>
          <div style="flex: 1; border-left: black;">
              <router-outlet></router-outlet>
          </div>
      </div>
  `
})
export class ComponentApp {

  constructor(public service: DataContainer, public http: HttpClient) {
    this.getRooms();
  }

  router: Router = inject(Router);

  isOpen: boolean = false;
  dialogText: string = "Open Dialog";
  openDialog() {
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

  async getRooms() {
    this.service.rooms = await firstValueFrom<Room[]>(this.http.get<Room[]>("http://localhost:8181/rooms"));
  }
}


