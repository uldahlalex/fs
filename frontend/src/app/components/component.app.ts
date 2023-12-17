import {Component} from '@angular/core';


@Component({
  selector: 'app-root',
  template: `
    <app-sidebar></app-sidebar>

    <router-outlet></router-outlet>


    <p-toast></p-toast>

  `
})
export class ComponentApp {
}


