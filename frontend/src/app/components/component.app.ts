import {Component} from '@angular/core';


@Component({
  selector: 'app-root',
  template: `
      <div style="display: flex; height: 50vh;">
          <div style="flex: 0 0 2%; margin-right: 10px;">

              <app-sidebar></app-sidebar>
          </div>

          <div
                  style="display: flex; flex-direction: column; flex: 3; border-left: black; height: 100%; align-items: center;">
              <router-outlet></router-outlet>


          </div>
          <p-toast></p-toast>
      </div>
  `
})
export class ComponentApp {
}


