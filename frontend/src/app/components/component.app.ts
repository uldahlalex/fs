import {APP_INITIALIZER, Component, inject} from '@angular/core';
import {CommonModule} from '@angular/common';
import {Router, RouterOutlet} from '@angular/router';
import {FormsModule} from "@angular/forms";
import {firstValueFrom} from "rxjs";
import {HttpClient, HttpClientModule, HttpSentEvent} from "@angular/common/http";
import {Room} from "../models/entities";
import {ComponentSidebar} from "./component.sidebar";
import {environment} from "../../environments/environment";


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, FormsModule, HttpClientModule, ComponentSidebar],
  template: `
      @if (!environment.production) {
          <div style="border: red solid 2px;">Running in development</div>
      }
      <div style="display: flex; height: 100vh;">
          <div style="flex: 0 0 2%; background: #f8f9fa; margin-right: 10px;">
              <!--<button>Open</button>
              <button (click)="toggleDialog()">{{ dialogText }}</button>
              <dialog [open]="isOpen"
                      style="box-shadow: 10px 10px 10px lightgray; border: transparent 0px; position: absolute; left: 100px;">

              </dialog>-->
              <app-sidebar></app-sidebar>
          </div>
          <div id="sidebarContainer" style="flex: 0 0 20%; background: #f8f9fa; margin-right: 10px;">
              <app-sidebar></app-sidebar>
          </div>

          <div style="flex: 3; border-left: black;">
              <router-outlet></router-outlet>
          </div>
      </div>
  `
})
export class ComponentApp {

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

  protected readonly environment = environment;
}


