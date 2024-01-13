import {BaseDto} from "./baseDto";
import {Message} from "./entities";

export class ServerAddsClientToRoom extends BaseDto<ServerAddsClientToRoom> {
  roomId?: number;
  liveConnections?: number;
  messages?: Message[];
}
