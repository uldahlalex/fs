import {Injectable} from "@angular/core";
import {Message, Room} from "./types";

@Injectable({
  providedIn: 'root'
})
export class DataContainer {
  items: Message[] = [];
  input: string = "";
  roomConnections: Map<any, WebSocket> = new Map();
  rooms: Room[] = [];

  enterRoom(roomId: any, userId: any) {
    this.roomConnections.set(roomId, new WebSocket(`ws://localhost:8181/${roomId}/${userId}`));
    this.roomConnections.get(roomId)!.onmessage = (event) => {
      const dataFromServer = JSON.parse(event.data) as Message[];
      this.items = this.items.concat(dataFromServer);
    }
  }

  pushToItems(roomId: any) {
    this.roomConnections.get(roomId)!.send(
      //JSON.stringify({messageContent: this.input})
      this.input
    );
  }
}
