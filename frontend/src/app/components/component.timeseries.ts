import {Component, Inject} from "@angular/core";
import {ApexChart, ApexTitleSubtitle, ApexXAxis} from "ng-apexcharts";
import {WebSocketClientService} from "../services/service.websocketclient";
import {API_SERVICE_TOKEN} from "../app.module";
import {ClientWantsToSubscribeToTimeSeriesData} from "../models/ClientWantsToSubscribeToTimeSeriesData";


@Component({
  template: `

    <div style="text-align:center">
      <apx-chart [series]="webSocketClientService.series!" [chart]="chart!" [title]="title!"
                 [xaxis]="xaxis"></apx-chart>
    </div>
    <button (click)="clearTemporarily()">Clear temporarily</button>

  `,
})
export class TimeSeriesComponent {

  chart: ApexChart | undefined = {
    height: 350,
    type: 'line',
    zoom: {
      enabled: false
    },
  };
  title: ApexTitleSubtitle | undefined = {
    text: 'Time series data',
  }
  xaxis: ApexXAxis = {
    type: 'datetime',
  };

  constructor(@Inject(API_SERVICE_TOKEN) public webSocketClientService: WebSocketClientService) {
    this.webSocketClientService.ClientWantsToSubscribeToTimeSeriesData(
      new ClientWantsToSubscribeToTimeSeriesData())
  }

  clearTemporarily() {
    // @ts-ignore
    this.webSocketClientService.series = [{
      name: "Timeseries",
      data: []
    }];
  }


}
