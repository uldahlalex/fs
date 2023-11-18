import {Injectable} from "@angular/core";
import {Message, Room, TransferObject} from "./types";

@Injectable({
  providedIn: 'root'
})
export class DataContainer {

  input: string = "";
  socketConnection: WebSocket = new WebSocket(`ws://localhost:8181`);
  roomsWithMessages: Map<number, Message[]> = new Map<number, Message[]>();
  rooms: Room[] = [{id: 1, title: "Work stuff"}, {id: 2, title: "Casual conversations"}, {id: 3, title: "Sports"}];

  constructor() {
    this.rooms.forEach(room => this.roomsWithMessages.set(room.id, []));
    this.socketConnection.onopen = () => console.log("connection established");
    //TRIGGERED ON ANY DOWNSTREAM MESSAGE
    this.socketConnection.onmessage = (event) => {
      const dataFromServer = JSON.parse(event.data.eventType) as TransferObject<any>;
      switch (dataFromServer.eventType) {
        case "DownstreamSendPastMessagesForRoom":
          this.DownstreamSendPastMessagesForRoom(dataFromServer.data.roomId, dataFromServer.data.messages)
          break;
        case "DownstreamBroadcastMessageToRoom":
          // here a new message is added and appended to the list of messages
          break;
      }
    }
  }


  DownstreamSendPastMessagesForRoom(roomId: number, messages: Message[]) {
    this.roomsWithMessages.set(roomId, messages);
  }

  upstreamSendMessageToRoom(roomId: any) {

  }

  async upstreamEnterRoom(roomId: any) {
    try {
      this.socketConnection!.send(JSON.stringify({eventType: "UpstreamEnterRoom", data: {roomId: roomId}}));
    } catch (e) {
      console.log("connection not established, retrying in 1 second")
      await new Promise(resolve => setTimeout(resolve, 1000));
      this.upstreamEnterRoom(roomId);
    }

  }
}

