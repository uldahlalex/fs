import {BrowserModule} from '@angular/platform-browser';
import {ComponentApp} from './components/component.app';
import {RouterModule} from "@angular/router";
import {ComponentRoom} from "./components/component.room";
import {ComponentLogin} from "./components/component.login";
import {environment} from "../environments/environment";
import {enableProdMode, NgModule} from "@angular/core";
import {ComponentSidebar} from "./components/component.sidebar";
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
      }
    ]),
    DialogModule
  ],
  declarations: [
    ComponentApp,
    ComponentRoom,
    ComponentSidebar,
    ComponentLogin,
    TimeSeriesComponent
  ],
  providers: [MessageService],
  bootstrap: [ComponentApp]
})
export class AppModule {
}

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => console.log(err));
