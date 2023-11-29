import {Injectable} from "@angular/core";
import {Message, Room} from "../models/entities";

@Injectable({
  providedIn: 'root'
})
export class State {

  input: string = "";
  socketConnection: WebSocket = new WebSocket(`ws://localhost:8181`);
  roomsWithMessages: Map<number, Message[]> = new Map<number, Message[]>();
  rooms: Room[] = [{id: 1, title: "Work stuff"}, {id: 2, title: "Casual conversations"}, {id: 3, title: "Sports"}];

  constructor() {
    this.rooms.forEach(room => this.roomsWithMessages.set(room.id!, []));
  }
}

