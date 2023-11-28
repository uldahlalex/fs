import {BaseTransferObject} from "./baseTransferObject";

export class ServerNotifiesClientsInRoom extends BaseTransferObject<ServerNotifiesClientsInRoom> {
  roomId?: number;
  message?: string;
}
