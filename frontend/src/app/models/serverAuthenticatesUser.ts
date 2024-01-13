import {BaseDto} from "./baseDto";

export class ServerAuthenticatesUser extends BaseDto<ServerAuthenticatesUser> {
  jwt?: string;
}
