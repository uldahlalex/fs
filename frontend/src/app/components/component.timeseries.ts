import {Component, Inject} from "@angular/core";
import {API_SERVICE_TOKEN} from "../app.module";
import {WebSocketClientService} from "../services/service.websocketclient";
import {ClientWantsToSubscribeToTimeSeriesData} from "../models/ClientWantsToSubscribeToTimeSeriesData";

@Component({
    template: `<div>{{webSocketClientService.timeseriesData | json}}</div>`,
})
export class ComponentTimeseries {
    constructor(@Inject(API_SERVICE_TOKEN) public webSocketClientService: WebSocketClientService
    ) {
        this.webSocketClientService.ClientWantsToSubscribeToTimeSeriesData(new ClientWantsToSubscribeToTimeSeriesData())
    }


}
