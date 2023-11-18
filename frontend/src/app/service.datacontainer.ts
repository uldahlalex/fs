import {Injectable} from "@angular/core";
import {DownstreamSendPastMessagesForRoom, Message, Room, TransferObject} from "./types";
import {concatWith} from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class DataContainer {

  input: string = "";
  socketConnection: WebSocket | undefined;

  roomsWithMessages: Map<number, Message[]> = new Map<number, Message[]>( );
  rooms: Room[] = [{id: 1, title: "Work stuff"}, {id: 2, title: "Casual conversations"}, {id: 3, title: "Sports"}];

  constructor() {
    this.rooms.forEach(room => this.roomsWithMessages.set(room.id, []));
  }

  EstablishConnection() {
    const jwt = localStorage.getItem("jwt") || ""
    this.socketConnection = new WebSocket(`ws://localhost:8181`);
    this.socketConnection.onopen = () => { };
    //TRIGGERED ON ANY DOWNSTREAM MESSAGE
    this.socketConnection.onmessage = (event) => {
      const dataFromServer = JSON.parse(event.data) as TransferObject<any>;
      switch (dataFromServer.eventType) {
        case "DownstreamSendPastMessagesForRoom":
            this.DownstreamSendPastMessagesForRoom(dataFromServer.data.roomId, dataFromServer.data.messages)
          break;
       case "DownstreamBroadcastMessageToRoom":

          break;
      }
    }
  }

  DownstreamSendPastMessagesForRoom(roomId: number, messages: Message[]) {
    const messagesCombined = this.roomsWithMessages.get(roomId)!.concat(messages);
    this.roomsWithMessages.set(roomId, messagesCombined);
  }

  upstreamAddMessage(roomId: any) {

  }

  async upstreamEnterRoom(roomId: any) {
    console.log("now entering room", roomId);
    try {
      await delay(1000);
        this.socketConnection!.send(JSON.stringify({eventType: "UpstreamEnterRoom", data: {roomId: roomId}}));



    } catch (e) {
      console.log("prob")
      console.log(e)
    }
  }
}


function delay(ms: number) {
  return new Promise(resolve => setTimeout(resolve, ms));
}
