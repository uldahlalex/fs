import {BaseDto} from "./baseDto";

export class ClientWantsToLoadOlderMessages extends BaseDto<ClientWantsToLoadOlderMessages> {
  roomId?: number;
  lastMessageId?: number;
}
