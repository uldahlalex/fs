import {BaseDto} from "./baseDto";

export class ServerDeletesMessage extends BaseDto<ServerDeletesMessage> {
  messageId?: number;
  roomId?: number;
}
