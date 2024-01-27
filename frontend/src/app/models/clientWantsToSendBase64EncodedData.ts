import {BaseDto} from "./baseDto";

export class ClientWantsToSendBase64EncodedData extends BaseDto<ClientWantsToSendBase64EncodedData> {
  base64EncodedData?: string;
}
