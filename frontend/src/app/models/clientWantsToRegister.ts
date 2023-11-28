import {BaseTransferObject} from "./baseTransferObject";

export class ClientWantsToRegister extends BaseTransferObject<ClientWantsToRegister> {
  email?: string;
  password?: string;
}
