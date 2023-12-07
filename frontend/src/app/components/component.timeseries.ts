import {Component, Inject} from "@angular/core";
import {API_SERVICE_TOKEN} from "../app.module";
import {WebSocketClientService} from "../services/service.websocketclient";
import {ClientWantsToSubscribeToTimeSeriesData} from "../models/ClientWantsToSubscribeToTimeSeriesData";


@Component({
    template: `
        <div>{{ webSocketClientService.timeseriesData | json }}</div>

        <div>
            <apx-chart [series]="webSocketClientService.series" [chart]="webSocketClientService.chart!"
                       [title]="webSocketClientService.title!"></apx-chart>

        </div>
    `,
})
export class ComponentTimeseries {
    constructor(@Inject(API_SERVICE_TOKEN) public webSocketClientService: WebSocketClientService
    ) {
        this.webSocketClientService.ClientWantsToSubscribeToTimeSeriesData(new ClientWantsToSubscribeToTimeSeriesData())
        this.makeMockChart()
    }


    makeMockChart() {
        this.webSocketClientService.chart = {
            height: 350,
            type: "line",
            zoom: {
                enabled: false
            }
        };
        this.webSocketClientService.title = {
            text: "Data over time",
            align: "left"
        };
        this.webSocketClientService.dataLabels = {
            enabled: true
        };
        this.webSocketClientService.stroke = {
            curve: "smooth"
        };
        this.webSocketClientService.title = {
            text: "Line Chart",
            align: "left"
        };
        this.webSocketClientService.grid = {
            row: {
                colors: ["#f3f3f3", "transparent"], // takes an array which will be repeated on columns
                opacity: 0.5
            }
        };
        this.webSocketClientService.xaxis = {
            type: "datetime"
        }
    }

}
