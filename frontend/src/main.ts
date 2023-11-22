import {bootstrapApplication} from '@angular/platform-browser';
import {ComponentApp} from './app/components/component.app';
import {provideRouter} from "@angular/router";
import {ComponentRoom} from "./app/components/component.room";
import {ComponentLogin} from "./app/components/component.login";

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
  )]
}).catch((err) => console.error(err));
