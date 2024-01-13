import {Message} from "./entities";
import {BaseDto} from "./baseDto";

export class ServerSendsOlderMessagesToClient extends BaseDto<ServerSendsOlderMessagesToClient> {
  roomId?: number;
  messages?: Message[];
}
