import {BaseTransferObject} from "./baseTransferObject";

export class ClientWantsToLogIn extends BaseTransferObject{
  email?: string;
  password?: string;

  constructor(init?: Partial<ClientWantsToLogIn>) {
    super();
    Object.assign(this, init);
  }
}

export class ClientWantsToRegister extends BaseTransferObject{
  email?: string;
  password?: string;
  passwordRepeat?: string;

  constructor(init?: Partial<ClientWantsToRegister>) {
    super();
    Object.assign(this, init);
  }
}
