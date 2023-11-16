import {bootstrapApplication} from '@angular/platform-browser';
import {ComponentApp} from './app/component.app';
import {provideRouter} from "@angular/router";
import {ComponentRoom} from "./app/component.room";

bootstrapApplication(ComponentApp, {
  providers: [provideRouter(
    [
      {
        path: 'room/:id', component: ComponentRoom
      }
    ]
  )]
}).catch((err) => console.error(err));
