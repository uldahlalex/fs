import {BaseDto} from "./baseDto";
import {EndUser} from "./entities";

export class ServerNotifiesClientsInRoomSomeoneHasJoinedRoom extends BaseDto<ServerNotifiesClientsInRoomSomeoneHasJoinedRoom> {
  user?: EndUser
  roomId?: number;
  message?: string;
}
