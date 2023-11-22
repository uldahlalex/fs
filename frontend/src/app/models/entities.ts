
export class Message {
  sender: number;
  timestamp: Date
  id: number;
  messageContent: string;

  constructor(sender: number, timestamp: Date, id: number, messageContent: string) {
    this.sender = sender;
    this.timestamp = timestamp;
    this.id = id;
    this.messageContent = messageContent;
  }
  deserialize(input: any): Message {
    return Object.assign(this, input);
  }
}
export type Room = {
  id: number;
  title: string
}

