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

  ws: WebSocket = new WebSocket("ws://localhost:8181");

  items: Message[] = [];
  input: string = "";

  constructor() {
    this.ws.onmessage = (event) => {
      const dataFromServer = JSON.parse(event.data) as MessageDto;
      console.log(dataFromServer);
      if (dataFromServer.type == MessageT.PAST_MESSAGES) {
        this.items = dataFromServer.data! as Message[];
        return;
      }
      this.items.push(dataFromServer.data as Message);
    }

  }

  pushToItems() {
    this.ws.send(JSON.stringify({messageContent: this.input}));
  }
}

export type MessageDto = {
  data: Message[] | Message
  type: MessageT
}

export enum MessageT {
  NEW_MESSAGE = "NEW_MESSAGE",
  PAST_MESSAGES = "PAST_MESSAGES"
}

export type Message = {
  id: number;
  messageContent: string;
}
