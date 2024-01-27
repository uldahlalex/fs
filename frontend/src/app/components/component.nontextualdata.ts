import {Component} from "@angular/core";
import {WebSocketClientService} from "../services/service.websocketclient";
import {ClientWantsToSendBase64EncodedData} from "../models/clientWantsToSendBase64EncodedData";

@Component({
  template: `

    <h1>Demo of transmitting data to API that is base64 encoded (such as images).</h1>
    <h2>Upload an image and see it here</h2>
    <img *ngIf="webSocketClientService.img!=undefined" [src]="webSocketClientService.img" alt="Base64 Image">
    <input
      hidden
      type="file"
      #uploader
      (change)="uploadFile($event)"
    />
    <button (click)="uploader.click()">
      Upload
    </button>
  `
})
export class ComponentNontextualdata {
  constructor(public webSocketClientService: WebSocketClientService) {
  }


  uploadFile($event: Event) {
    let file = ($event.target as HTMLInputElement).files![0];
    let reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => {
      let base64EncodedData = reader.result!.toString().split(',')[1];
      this.webSocketClientService.socketConnection.sendDto(new ClientWantsToSendBase64EncodedData({base64EncodedData: base64EncodedData}));
    };
  }
}
