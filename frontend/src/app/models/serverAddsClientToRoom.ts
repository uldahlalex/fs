import {BaseTransferObject} from "./baseTransferObject";
import {Message} from "./entities";

export class ServerAddsClientToRoom extends BaseTransferObject<ServerAddsClientToRoom> {
  roomId?: number;
  messages?: Message[];
}
