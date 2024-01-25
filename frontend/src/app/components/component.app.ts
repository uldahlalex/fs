import {Component} from '@angular/core';
import {ChipModule} from "primeng/chip";

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


