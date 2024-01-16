import {EndUser} from "./entities";
import {BaseDto} from "./baseDto";


export class ServerNotifiesClientsInRoomSomeoneHasLeftRoom extends BaseDto<ServerNotifiesClientsInRoomSomeoneHasLeftRoom>{
  user?: EndUser
  roomId?: number;
  message?: string;
}
