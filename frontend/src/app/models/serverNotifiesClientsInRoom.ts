import {BaseDto} from "./baseDto";
import {EndUser} from "./entities";

export class ServerNotifiesClientsInRoom extends BaseDto<ServerNotifiesClientsInRoom> {
  roomId?: number;
  message?: string;
}

export class ServerNotifiesClientsInRoomSomeoneHasJoinedRoom extends ServerNotifiesClientsInRoom {
  user?: EndUser
}

export class ServerNotifiesClientsInRoomSomeoneHasLeftRoom extends ServerNotifiesClientsInRoom {
  user?: EndUser
}
