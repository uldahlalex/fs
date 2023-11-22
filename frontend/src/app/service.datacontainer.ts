import {Injectable} from "@angular/core";
import {Message, Room} from "./models/entities";
import {BaseTransferObject} from "./models/baseTransferObject";
import {ClientWantsToEnterRoom, ServerLetsClientEnterRoom} from "./models/RoomTransferObjects";
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
      //console.log(event)
      var data = JSON.parse(event.data) as BaseTransferObject;
      console.log(data)
      switch (data.eventType) {
        case "ServerLetsClientEnterRoom":
          this.DownstreamSendPastMessagesForRoom(data);
          //let o = dataFromServer as ClientWantsToEnterRoom;
          //this.DownstreamSendPastMessagesForRoom(o.roomId, dataFromServer.data.messages)
          break;
        case "DownstreamBroadcastMessageToRoom":
          // here a new message is added and appended to the list of messages
          break;
      }
    }
  }


  DownstreamSendPastMessagesForRoom(obj: any) {
    this.roomsWithMessages.set(obj.roomId, obj.recentMessages);
    console.log(this.roomsWithMessages)
  }

  upstreamSendMessageToRoom(roomId: any) {

  }

  async clientWantsToEnterRoom(roomId: any) {
    try {
      let o : ClientWantsToEnterRoom = new ClientWantsToEnterRoom(roomId);
      this.socketConnection!.send(JSON.stringify(o));
    } catch (e) {
      console.log("connection not established, retrying in 1 second")
      await new Promise(resolve => setTimeout(resolve, 1000));
      this.clientWantsToEnterRoom(roomId);
    }

  }
}

