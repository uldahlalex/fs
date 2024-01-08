import {BaseTransferObject} from "./baseTransferObject";

export class ServerSendsErrorMessageToClient extends BaseTransferObject<ServerSendsErrorMessageToClient> {
  errorMessage?: string;
  receivedMessage?: string;
}
