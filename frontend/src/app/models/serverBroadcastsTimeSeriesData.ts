import {BaseTransferObject} from "./baseTransferObject";
import {TimeSeriesData} from "./entities";

export class ServerBroadcastsTimeSeriesData extends BaseTransferObject<ServerBroadcastsTimeSeriesData> {
  timeSeriesData?: TimeSeriesData;
}
