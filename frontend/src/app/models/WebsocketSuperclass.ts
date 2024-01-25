import {BaseDto} from "./baseDto";

export class WebsocketSuperclass extends WebSocket {
  sendDto(dto: BaseDto<any>) {
    try {
      this.send(JSON.stringify(dto));
    } catch (e) {
      console.log(e)
    }

  }
}
