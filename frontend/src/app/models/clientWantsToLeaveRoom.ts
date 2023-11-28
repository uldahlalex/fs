import {BaseTransferObject} from "./baseTransferObject";

export class ClientWantsToLeaveRoom extends BaseTransferObject<ClientWantsToLeaveRoom> {
  roomId?: number;
}
