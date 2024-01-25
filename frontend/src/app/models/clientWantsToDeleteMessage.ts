import {BaseDto} from "./baseDto";

export class ClientWantsToDeleteMessage extends BaseDto<ClientWantsToDeleteMessage> {
  messageId?: number;
  roomId?: number;
}
