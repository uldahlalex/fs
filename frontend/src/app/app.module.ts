import {BrowserModule} from '@angular/platform-browser';
import {ComponentApp} from './components/component.app';
import {RouterModule} from "@angular/router";
import {ComponentRoom} from "./components/component.room";
import {ComponentLogin} from "./components/component.login";
import {environment} from "../environments/environment";
import {enableProdMode, ErrorHandler, NgModule} from "@angular/core";
import {ComponentMenu} from "./components/component.menu";
import {ToastModule} from "primeng/toast";
import {MessageModule} from "primeng/message";
import {BrowserAnimationsModule} from "@angular/platform-browser/animations";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {CommonModule} from "@angular/common";
import {platformBrowserDynamic} from "@angular/platform-browser-dynamic";
import {MessageService} from "primeng/api";
import {DialogModule} from "primeng/dialog";
import {TimeSeriesComponent} from "./components/component.timeseries";
import {NgApexchartsModule} from "ng-apexcharts";
import {ChipModule} from "primeng/chip";
import {GlobalErrorHandlerService} from "./services/global.errorhandler.service";
import {ComponentNontextualdata} from "./components/component.nontextualdata";


@NgModule({
  imports: [
    BrowserModule,
    CommonModule,
    ToastModule,
    MessageModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    NgApexchartsModule,
    FormsModule,
    RouterModule.forRoot([
      {
        path: 'room/:id', component: ComponentRoom
      },
      {
        path: 'login', component: ComponentLogin
      },
      {
        path: 'timeseries', component: TimeSeriesComponent
      },
      {
        path: 'nontextualdata', component: ComponentNontextualdata
      }
    ]),
    DialogModule,
    ChipModule,

  ],
  declarations: [
    ComponentApp,
    ComponentRoom,
    ComponentNontextualdata,
    ComponentMenu,
    ComponentLogin,
    TimeSeriesComponent
  ],
  providers: [MessageService, {
    provide: ErrorHandler, useClass: GlobalErrorHandlerService
  }],
  bootstrap: [ComponentApp]
})
export class AppModule {
}

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => console.log(err));
