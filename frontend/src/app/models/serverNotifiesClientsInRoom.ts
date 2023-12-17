import {BaseTransferObject} from "./baseTransferObject";
import {EndUser} from "./entities";

export class ServerNotifiesClientsInRoom extends BaseTransferObject<ServerNotifiesClientsInRoom> {
  roomId?: number;
  message?: string;
}

export class ServerNotifiesClientsInRoomSomeoneHasJoinedRoom extends ServerNotifiesClientsInRoom {
  user?: EndUser
}

export class ServerNotifiesClientsInRoomSomeoneHasLeftRoom extends ServerNotifiesClientsInRoom {
  user?: EndUser
}
