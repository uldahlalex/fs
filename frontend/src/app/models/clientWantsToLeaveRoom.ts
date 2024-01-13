import {BaseDto} from "./baseDto";

export class ClientWantsToLeaveRoom extends BaseDto<ClientWantsToLeaveRoom> {
  roomId?: number;
}
