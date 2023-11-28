import {Message} from "./entities";
import {BaseTransferObject} from "./baseTransferObject";

export class ClientWantsToLoadOlderMessages extends BaseTransferObject {
  roomId?: number;
  lastMessageId?: number;
  constructor(init?: Partial<ClientWantsToLoadOlderMessages>) {
    super();
    Object.assign(this, init);
  }
}

export class ServerSendsOlderMessagesToClient extends BaseTransferObject {
  roomId?: number;
  messages?: Message[];
  constructor(init?: Partial<ServerSendsOlderMessagesToClient>) {
    super();
    Object.assign(this, init);
  }
}
