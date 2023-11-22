export class BaseTransferObject {
  eventType?: string;
  constructor() {
    this.eventType = this.constructor.name;
  }
}

