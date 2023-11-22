import {APP_INITIALIZER, Component, inject} from '@angular/core';
import {CommonModule} from '@angular/common';
import {Router, RouterOutlet} from '@angular/router';
import {FormsModule} from "@angular/forms";
import {State} from "../services/service.state";
import {firstValueFrom} from "rxjs";
import {HttpClient, HttpClientModule, HttpSentEvent} from "@angular/common/http";
import {Room} from "../models/entities";
import {ComponentSidebar} from "./component.sidebar";


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, FormsModule, HttpClientModule, ComponentSidebar],
  template: `
      <div style="display: flex; height: 100vh;">
          <div style="flex: 0 0 20%; background: #f8f9fa; margin-right: 10px;">
              <app-sidebar></app-sidebar>
          </div>
          <div style="flex: 3; border-left: black;">
              <router-outlet></router-outlet>
          </div>
      </div>
  `
})
export class ComponentApp {}


