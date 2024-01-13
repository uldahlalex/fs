import {BaseDto} from "./baseDto";

export class ClientWantsToRegister extends BaseDto<ClientWantsToRegister> {
  email?: string;
  password?: string;
}
