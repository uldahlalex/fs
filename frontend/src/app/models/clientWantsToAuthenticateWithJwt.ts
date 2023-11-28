import {BaseTransferObject} from "./baseTransferObject";

export class ClientWantsToAuthenticateWithJwt extends BaseTransferObject<ClientWantsToAuthenticateWithJwt> {
  jwt?: string;
}
