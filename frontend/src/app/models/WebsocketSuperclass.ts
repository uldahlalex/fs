import {BaseDto} from "./baseDto";
import {ClientWantsToAuthenticateWithJwt} from "./clientWantsToAuthenticateWithJwt";
import ReconnectingWebSocket from "reconnecting-websocket";

export class WebsocketSuperclass extends ReconnectingWebSocket {
  private messageQueue: Array<BaseDto<any>> = [];

  constructor(address: string) {
    super(address);
    this.onopen = this.handleOpen.bind(this);
  }

  sendDto(dto: BaseDto<any>) {
    console.log("Sending: " + JSON.stringify(dto));
    if (this.readyState === WebSocket.OPEN) {
      this.send(JSON.stringify(dto));
    } else {
      this.messageQueue.push(dto);
    }
  }

  private handleOpen() {
    let jwt = localStorage.getItem('jwt');
    if (jwt && jwt != '')
      this.sendDto(new ClientWantsToAuthenticateWithJwt({jwt: jwt}));
    //attempt to rejoin current room
    while (this.messageQueue.length > 0) {

      const dto = this.messageQueue.shift();
      if (dto) {
        this.send(JSON.stringify(dto));
      }
    }
  }

}
