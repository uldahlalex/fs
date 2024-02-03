import {ErrorHandler, Injectable} from "@angular/core";
import {MessageService} from "primeng/api";

@Injectable({
  providedIn: 'root'
})
export class GlobalErrorHandlerService implements ErrorHandler {

  constructor(private messageService: MessageService) {
  }

  handleError(error: any): void {
    console.log(error);
    this.messageService.add({severity: 'error', detail: error.message});
  }

}
