import {Message} from "./entities";
import {BaseDto} from "./baseDto";

export class ServerBroadcastsMessageToClientsInRoom extends BaseDto<ServerBroadcastsMessageToClientsInRoom> {
  roomId?: number;
  message?: Message;
}
