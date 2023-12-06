import {BaseTransferObject} from "./baseTransferObject";

export class ClientWantsToSendMessageToRoom extends BaseTransferObject<ClientWantsToSendMessageToRoom> {
  roomId?: number;
  messageContent?: string;
}
