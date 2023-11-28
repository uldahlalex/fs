import {Message} from "./entities";
import {BaseTransferObject} from "./baseTransferObject";

export class ServerSendsOlderMessagesToClient extends BaseTransferObject<ServerSendsOlderMessagesToClient> {
  roomId?: number;
  messages?: Message[];
}
