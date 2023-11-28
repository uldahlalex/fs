import {BaseTransferObject} from "./baseTransferObject";

export class ServerAuthenticatesUser extends BaseTransferObject<ServerAuthenticatesUser> {
  jwt?: string;
}
