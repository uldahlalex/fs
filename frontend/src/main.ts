import {bootstrapApplication} from '@angular/platform-browser';
import {ComponentApp} from './app/components/component.app';
import {provideRouter} from "@angular/router";
import {ComponentRoom} from "./app/components/component.room";
import {ComponentLogin} from "./app/components/component.login";
import {ApiCallServiceInterface} from "./app/services/apiCallService.interface";
import {ensureSourceFileVersions} from "@angular-devkit/build-angular/src/tools/esbuild/angular/angular-host";
import {environment} from "./environments/environment";
import {ApiCallServiceMock} from "./app/services/apiCallService.mock";
import {InjectionToken} from "@angular/core";
import {WebSocketClientService} from "./app/services/service.websocketclient";

export const API_SERVICE_TOKEN = new InjectionToken<ApiCallServiceInterface>('ApiServiceToken');

const ApiServiceProvider = {
  provide: API_SERVICE_TOKEN,
  useClass: environment.production ? WebSocketClientService : ApiCallServiceMock,
};


bootstrapApplication(ComponentApp, {
  providers: [provideRouter(
    [
      {
        path: 'room/:id', component: ComponentRoom
      },
      {
        path: 'login', component: ComponentLogin
      }
    ]
  ),
    ApiServiceProvider
]}).catch((err) => console.error(err));
