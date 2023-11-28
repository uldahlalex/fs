import {inject, Injectable} from "@angular/core";
import {BaseTransferObject} from "../models/baseTransferObject";
import {State} from "./service.state";
import {ServerAddsClientToRoom} from "../models/serverAddsClientToRoom";
import {ServerSendsOlderMessagesToClient} from "../models/serverSendsOlderMessagesToClient";
import {ClientWantsToRegister} from "../models/clientWantsToRegister";
import {ClientWantsToAuthenticate} from "../models/clientWantsToAuthenticate";
import {ClientWantsToEnterRoom} from "../models/clientWantsToEnterRoom";
import {ClientWantsToLoadOlderMessages} from "../models/clientWantsToLoadOlderMessages";

@Injectable({providedIn: 'root'})
export class WebSocketClientService {

  state = inject(State);
  constructor() {
    this.state.socketConnection.onopen = () => console.info("connection established");
    this.state.socketConnection.onmessage = (event) => {
      var data = JSON.parse(event.data) as BaseTransferObject<any>;
      switch (data.eventType) {
        case "ServerAddsClientToRoom":
          this.ServerAddsClientToRoom( data as ServerAddsClientToRoom);
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
    this.state.roomsWithMessages.get(serverSendsOlderMessagesToClient.roomId!)!
      .unshift(...serverSendsOlderMessagesToClient.messages?.reverse()!);

  }

  ServerAddsClientToRoom(serverLetsClientEnterRoom: ServerAddsClientToRoom) {
    this.state.roomsWithMessages.set(serverLetsClientEnterRoom.roomId!, serverLetsClientEnterRoom.messages!);
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

  clientWantsToLogIn(clientWantsToLogIn: ClientWantsToAuthenticate) {
    console.log(clientWantsToLogIn)
  }

  clientWantsToRegister(clientWantsToRegister: ClientWantsToRegister) {
    console.log(clientWantsToRegister)
  }

  clientWantsToLoadOlderMessages(clientWantsToLoadOlderMessages: ClientWantsToLoadOlderMessages) {
    this.state.socketConnection.send(JSON.stringify(clientWantsToLoadOlderMessages));
  }
}
