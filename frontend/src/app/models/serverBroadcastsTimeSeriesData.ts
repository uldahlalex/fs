import {BaseTransferObject} from "./baseTransferObject";
import {TimeSeries} from "./entities";

export class ServerBroadcastsTimeSeriesData extends BaseTransferObject<ServerBroadcastsTimeSeriesData> {
    timeSeriesDataPoint?: TimeSeries;
}
