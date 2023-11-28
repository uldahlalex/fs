import {BaseTransferObject} from "./baseTransferObject";

export class ClientWantsToAuthenticate extends BaseTransferObject<ClientWantsToAuthenticate> {
  email?: string;
  password?: string;
}
