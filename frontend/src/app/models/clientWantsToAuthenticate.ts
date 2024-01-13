import {BaseDto} from "./baseDto";

export class ClientWantsToAuthenticate extends BaseDto<ClientWantsToAuthenticate> {
  email?: string;
  password?: string;
}
