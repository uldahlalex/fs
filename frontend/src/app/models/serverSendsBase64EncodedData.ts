import {BaseDto} from "./baseDto";

export class ServerSendsBase64EncodedData extends BaseDto<ServerSendsBase64EncodedData>{
  base64EncodedData?: string;
}
