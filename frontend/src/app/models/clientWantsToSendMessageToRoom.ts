import {BaseDto} from "./baseDto";

export class ClientWantsToSendMessageToRoom extends BaseDto<ClientWantsToSendMessageToRoom> {
  roomId?: number;
  messageContent?: string;
}
