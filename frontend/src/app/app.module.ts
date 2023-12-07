import {BrowserModule} from '@angular/platform-browser';
import {ComponentApp} from './components/component.app';
import {RouterModule} from "@angular/router";
import {ComponentRoom} from "./components/component.room";
import {ComponentLogin} from "./components/component.login";
import {ApiCallServiceInterface} from "./services/apiCallService.interface";
import {environment} from "../environments/environment";
import {ApiCallServiceMock} from "./services/apiCallService.mock";
import {enableProdMode, InjectionToken, NgModule} from "@angular/core";
import {WebSocketClientService} from "./services/service.websocketclient";
import {ComponentSidebar} from "./components/component.sidebar";
import {ToastModule} from "primeng/toast";
import {MessageModule} from "primeng/message";
import {BrowserAnimationsModule} from "@angular/platform-browser/animations";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {CommonModule} from "@angular/common";
import {platformBrowserDynamic} from "@angular/platform-browser-dynamic";
import {MessageService} from "primeng/api";
import {DialogModule} from "primeng/dialog";
import {ComponentTimeseries} from "./components/component.timeseries";

export const API_SERVICE_TOKEN = new InjectionToken<ApiCallServiceInterface>('ApiServiceToken');

const ApiServiceProvider = {
  provide: API_SERVICE_TOKEN,
  useClass: environment.production ? WebSocketClientService : ApiCallServiceMock,
};

@NgModule({
  imports: [
    BrowserModule,
    CommonModule,
    ToastModule,
    MessageModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule.forRoot([
      {
        path: 'room/:id', component: ComponentRoom
      },
      {
        path: 'login', component: ComponentLogin
      },
      {
        path: 'timeseries', component: ComponentTimeseries
      }
    ]),
    DialogModule,
    DialogModule,
  ],
  declarations: [
    ComponentApp,
    ComponentRoom,
    ComponentSidebar,
    ComponentLogin,
    ComponentTimeseries
  ],
  providers: [ApiServiceProvider, MessageService],
  bootstrap: [ComponentApp]
})
export class AppModule {
}

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic().bootstrapModule(AppModule)
    .catch(err => console.log(err));
