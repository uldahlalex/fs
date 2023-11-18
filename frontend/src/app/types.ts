
export type Message = {
  sender: number;
  timestamp: Date
  id: number;
  messageContent: string;
}
export type Room = {
  id: number;
  title: string
}

export type TransferObject<T> = {
  eventType: string; //Always Upstream/Downstream + Event name
  data: T;
}

export type DownstreamSendPastMessagesForRoom = {
  roomId: number;
  messages: Message[];
}
