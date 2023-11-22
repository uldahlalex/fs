import {BaseTransferObject} from "./baseTransferObject";

export class ClientWantsToEnterRoom extends BaseTransferObject {
  roomId: number;

  constructor(roomId: number) {
    super();

    this.roomId = roomId;
  }
}
