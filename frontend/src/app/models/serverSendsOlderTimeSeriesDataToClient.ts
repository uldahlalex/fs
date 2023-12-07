import {BaseTransferObject} from "./baseTransferObject";
import {TimeSeries} from "./entities";

export class ServerSendsOlderTimeSeriesDataToClient extends BaseTransferObject<ServerSendsOlderTimeSeriesDataToClient> {
    public timeseries: TimeSeries[] = [];
}
