import {BaseDto} from "./baseDto";

export class WebsocketSuperclass extends WebSocket {
  sendDto(dto: BaseDto<any>) {
    this.send(JSON.stringify(dto));
  }
}
