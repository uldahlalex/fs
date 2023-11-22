import {inject, Injectable} from "@angular/core";
import {BaseTransferObject} from "../models/baseTransferObject";
import {State} from "./service.state";
import {ClientWantsToEnterRoom, ServerLetsClientEnterRoom} from "../models/EnterRoom";

@Injectable({providedIn: 'root'})
export class WebsockSocketClient {

  state = inject(State);
  constructor() {
    this.state.socketConnection.onopen = () => console.info("connection established");
    this.state.socketConnection.onmessage = (event) => {
      var data = JSON.parse(event.data) as BaseTransferObject;
      switch (data.eventType) {
        case "ServerLetsClientEnterRoom":
          this.ServerLetsClientEnterRoom( data as ServerLetsClientEnterRoom);
          break;
        case "DownstreamBroadcastMessageToRoom":
          //todo finish server induced events
          break;
      }
    }
  }

  ServerLetsClientEnterRoom(serverLetsClientEnterRoom: ServerLetsClientEnterRoom) {
    this.state.roomsWithMessages.set(serverLetsClientEnterRoom.roomId!, serverLetsClientEnterRoom.recentMessages!);
  }

  upstreamSendMessageToRoom(roomId: any) {

  }

  async clientWantsToEnterRoom(roomId: any) {
    try {
      let clientWantsToEnterRoom =  new ClientWantsToEnterRoom({roomId: roomId});
      this.state.socketConnection!.send(JSON.stringify(clientWantsToEnterRoom));
    } catch (e) {
      console.log("connection not established, retrying in 1 second")
      await new Promise(resolve => setTimeout(resolve, 1000));
      this.clientWantsToEnterRoom(roomId);
    }

  }
}
