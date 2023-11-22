import {Component} from "@angular/core";

@Component({
  standalone: true,
  imports: [],
  template: `
    <div style="height: calc(20% + 100px); width: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center;">
        <input placeholder="email">
        <input placeholder="password">
    </div>
  `
})
export class ComponentLogin {

}
