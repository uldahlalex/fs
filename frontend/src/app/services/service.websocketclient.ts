import {inject, Injectable} from "@angular/core";
import {BaseTransferObject} from "../models/baseTransferObject";
import {State} from "./service.state";
import {ClientWantsToEnterRoom} from "../models/RoomTransferObjects";

@Injectable({providedIn: 'root'})
export class WebsockSocketClient {

  state = inject(State);
  constructor() {
    this.state.socketConnection.onopen = () => console.log("connection established");
    //TRIGGERED ON ANY DOWNSTREAM MESSAGE
    this.state.socketConnection.onmessage = (event) => {
      //console.log(event)
      var data = JSON.parse(event.data) as BaseTransferObject;

      //todo pt er det en json med en string som json - kan jeg få dette ændret?
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
    this.state.roomsWithMessages.set(obj.roomId, obj.recentMessages);
  }

  upstreamSendMessageToRoom(roomId: any) {

  }

  async clientWantsToEnterRoom(roomId: any) {
    try {
      let o : ClientWantsToEnterRoom = new ClientWantsToEnterRoom(roomId);
      this.state.socketConnection!.send(JSON.stringify(o));
    } catch (e) {
      console.log("connection not established, retrying in 1 second")
      await new Promise(resolve => setTimeout(resolve, 1000));
      this.clientWantsToEnterRoom(roomId);
    }

  }
}
