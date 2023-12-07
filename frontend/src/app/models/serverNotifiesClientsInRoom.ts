import {BaseTransferObject} from "./baseTransferObject";

export class ServerNotifiesClientsInRoom extends BaseTransferObject<ServerNotifiesClientsInRoom> {
  roomId?: number;
  message?: string;
}

export class ServerNotifiesClientsInRoomSomeoneHasJoinedRoom extends ServerNotifiesClientsInRoom {
}

export class ServerNotifiesClientsInRoomSomeoneHasLeftRoom extends ServerNotifiesClientsInRoom {
}
