
export class Message {
  sender?: number;
  timestamp?: Date
  id?: number;
  messageContent?: string;

  constructor(init?: Partial<Message>) {
    Object.assign(this, init);
  }
}
export class Room {
  id?: number;
  title?: string;
  constructor(init?: Partial<Room>) {
    Object.assign(this, init);
  }
}

