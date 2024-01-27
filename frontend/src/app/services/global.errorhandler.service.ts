import {ErrorHandler, Injectable} from "@angular/core";
import {MessageService} from "primeng/api";
import {NoJwtError} from "../../errors/custom-errors";

@Injectable({
  providedIn: 'root'
})
export class GlobalErrorHandlerService implements ErrorHandler{

  constructor(private messageService: MessageService) {
  }

  handleError(error: any): void {
    if (error instanceof NoJwtError) {
      this.messageService.add({severity: 'error', detail: error.customMessage});
    }

    else {
      this.messageService.add({severity: 'error',  detail: error.message});
    }
  }


}
