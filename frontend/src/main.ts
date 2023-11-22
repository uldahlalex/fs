import {bootstrapApplication} from '@angular/platform-browser';
import {ComponentApp} from './app/components/component.app';
import {provideRouter} from "@angular/router";
import {ComponentRoom} from "./app/components/component.room";

bootstrapApplication(ComponentApp, {
  providers: [provideRouter(
    [
      {
        path: 'room/:id', component: ComponentRoom
      }
    ]
  )]
}).catch((err) => console.error(err));
