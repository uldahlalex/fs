import {BaseTransferObject} from "./baseTransferObject";
import {Message} from "./entities";

export class ClientWantsToEnterRoom extends BaseTransferObject {
  roomId?: number;

  constructor(init?: Partial<ClientWantsToEnterRoom>) {
    super();
    // copy all properties from init to this
    Object.assign(this, init);
  }

}

export class ServerLetsClientEnterRoom extends BaseTransferObject {
  roomId?: number;
  recentMessages?: Message[];
  constructor() {
    super();
  }

}
