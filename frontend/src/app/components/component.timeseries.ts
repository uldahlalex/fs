import {Component} from "@angular/core";
import {ApexAxisChartSeries, ApexChart, ApexNonAxisChartSeries, ApexTitleSubtitle} from "ng-apexcharts";


@Component({
  template: `

      <div style="text-align:center">
          <apx-chart [series]="series!" [chart]="chart!" [title]="title!"></apx-chart>
      </div>
      <button (click)="updateSeriesData()">Update data</button>
  `,
})
export class TimeSeriesComponent {
  series: ApexAxisChartSeries | ApexNonAxisChartSeries = [{
    name: "Desktops",
    data: [10, 41, 35, 51, 49, 62, 69, 91, 148]
  }];

  chart: ApexChart | undefined = {
    height: 350,
    type: 'line',
    zoom: {
      enabled: false
    }
  };

  title: ApexTitleSubtitle | undefined;

  updateSeriesData() {
    this.series = [{
      name: "Desktops",
      data: [10, 41, 35, 51, 49, 62, 69, 91, 35, 51, 49, 62, 69, 91, 148]
    }];
  }

}
