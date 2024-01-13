import {BaseDto} from "./baseDto";

export class ClientWantsToAuthenticateWithJwt extends BaseDto<ClientWantsToAuthenticateWithJwt> {
  jwt?: string;
}
