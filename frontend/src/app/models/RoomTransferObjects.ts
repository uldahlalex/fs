import {BaseTransferObject} from "./baseTransferObject";
import {Message} from "./entities";

export class ClientWantsToEnterRoom extends BaseTransferObject {
  roomId: number;

  constructor(roomId: number) {
    super();
    this.roomId = roomId;
  }
}

export class ServerLetsClientEnterRoom extends BaseTransferObject {
  roomId: number;
  recentMessages: Message[];
  constructor(recentMessages: Message[], roomId: number) {
    super();
    this.roomId = roomId;
    this.recentMessages = recentMessages;
  }

  static deserialize(input: any): ServerLetsClientEnterRoom {
    return Object.assign(this, input);
  }
}
