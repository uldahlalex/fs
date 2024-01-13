import {BaseDto} from "./baseDto";
import {TimeSeries} from "./entities";

export class ServerSendsOlderTimeSeriesDataToClient extends BaseDto<ServerSendsOlderTimeSeriesDataToClient> {
  public timeseries: TimeSeries[] = [];
}
