import {BaseTransferObject} from "./baseTransferObject";

export class ClientWantsToLoadOlderMessages extends BaseTransferObject<ClientWantsToLoadOlderMessages> {
  roomId?: number;
  lastMessageId?: number;
}
