import {BaseDto} from "./baseDto";

export class ServerSendsErrorMessageToClient extends BaseDto<ServerSendsErrorMessageToClient> {
  errorMessage?: string;
  receivedMessage?: string;
}
