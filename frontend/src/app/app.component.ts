import {Component} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterOutlet} from '@angular/router';
import {FormsModule} from "@angular/forms";


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, FormsModule],
  template: `
      <div>
          <input placeholder="insert some number" [(ngModel)]="input">
          <button class="" (click)="pushToItems()">insert</button>
      </div>



      @for (i of items;track i) {
          <div style="flex-direction: row; order: 2;"> {{ i | json }}</div>
      }

  `,
})
export class AppComponent {

  ws: WebSocket = new WebSocket("ws://localhost:8181/1/2/3"); //only establish conn after login and room pick

  items: Message[] = [];
  input: string = "";

  constructor() {
    this.ws.onmessage = (event) => {
      const dataFromServer = JSON.parse(event.data) as Message[];
      this.items = this.items.concat(dataFromServer);
    }

  }

  pushToItems() {
    this.ws.send(
      //JSON.stringify({messageContent: this.input})
      this.input
    );
  }
}


export type Message = {
  id: number;
  messageContent: string;
}
