import {BaseDto} from "./baseDto";
import {TimeSeries} from "./entities";

export class ServerBroadcastsTimeSeriesData extends BaseDto<ServerBroadcastsTimeSeriesData> {
  timeSeriesDataPoint?: TimeSeries;
}
