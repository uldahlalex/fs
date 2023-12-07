import {APP_INITIALIZER, Component, Inject, inject, Injectable} from '@angular/core';
import {CommonModule} from '@angular/common';
import {Router, RouterOutlet} from '@angular/router';
import {FormsModule} from "@angular/forms";
import {firstValueFrom} from "rxjs";
import {HttpClient, HttpClientModule, HttpSentEvent} from "@angular/common/http";
import {Room} from "../models/entities";
import {ComponentSidebar} from "./component.sidebar";
import {environment} from "../../environments/environment";
import {WebSocketClientService} from "../services/service.websocketclient";
import {API_SERVICE_TOKEN} from "../app.module";
import {ToastModule} from "primeng/toast";
import {BrowserAnimationsModule, NoopAnimationsModule} from "@angular/platform-browser/animations";
import {BrowserModule} from "@angular/platform-browser";
import {MessageModule} from "primeng/message";


@Component({
  selector: 'app-root',
  template: `
      <div style="display: flex; height: 50vh;">
          <div style="flex: 0 0 2%; margin-right: 10px;">
              <!--<button>Open</button>
              <button (click)="toggleDialog()">{{ dialogText }}</button>
              <dialog [open]="isOpen"
                      style="box-shadow: 10px 10px 10px lightgray; border: transparent 0px; position: absolute; left: 100px;">

              </dialog>-->
              <app-sidebar></app-sidebar>
          </div>

          <div style="display: flex; flex-direction: column; flex: 3; border-left: black; height: 100%; align-items: center;">
              <router-outlet></router-outlet>


          </div>    <p-toast></p-toast>
      </div>
  `
})
export class ComponentApp {

  isOpen: boolean = false;
  dialogText: string = "Open Dialog";

  constructor( @Inject(API_SERVICE_TOKEN)  public webSocketClientService: WebSocketClientService
  ) {
  }

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


