import {Message} from "./entities";
import {BaseTransferObject} from "./baseTransferObject";

export class ServerBroadcastsMessageToClientsInRoom extends BaseTransferObject<ServerBroadcastsMessageToClientsInRoom> {
  roomId?: number;
  message?: Message;
}
