import {inject, Injectable} from "@angular/core";
import {BaseTransferObject} from "../models/baseTransferObject";
import {State} from "./service.state";
import {ClientWantsToEnterRoom, ServerLetsClientEnterRoom} from "../models/EnterRoom";
import {ClientWantsToLogIn, ClientWantsToRegister} from "../models/authTransferObjects";
import {ClientWantsToLoadOlderMessages, ServerSendsOlderMessagesToClient} from "../models/sendMessage";

@Injectable({providedIn: 'root'})
export class WebSocketClientService {

  state = inject(State);
  constructor() {
    this.state.socketConnection.onopen = () => console.info("connection established");
    this.state.socketConnection.onmessage = (event) => {
      var data = JSON.parse(event.data) as BaseTransferObject;
      switch (data.eventType) {
        case "ServerLetsClientEnterRoom":
          this.ServerLetsClientEnterRoom( data as ServerLetsClientEnterRoom);
          break;
        case "ServerBroadcastsMessageToRoom":
          //todo finish server induced events
        case "ServerSendsOlderMessagesToClient":
          this.ServerSendsOlderMessagesToClient(data as ServerSendsOlderMessagesToClient);
          break;
      }
    }
  }

  private ServerSendsOlderMessagesToClient(serverSendsOlderMessagesToClient: ServerSendsOlderMessagesToClient) {
    this.state.roomsWithMessages.get(serverSendsOlderMessagesToClient.roomId!)!.push(...serverSendsOlderMessagesToClient.messages!);

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

  clientWantsToLogIn(clientWantsToLogIn: ClientWantsToLogIn) {
    console.log(clientWantsToLogIn)
  }

  clientWantsToRegister(clientWantsToRegister: ClientWantsToRegister) {
    console.log(clientWantsToRegister)
  }

  clientWantsToLoadOlderMessages(clientWantsToLoadOlderMessages: ClientWantsToLoadOlderMessages) {
    this.state.socketConnection.send(JSON.stringify(clientWantsToLoadOlderMessages));
  }
}
