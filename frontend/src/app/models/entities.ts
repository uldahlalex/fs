export class Message {
    sender?: number;
    timestamp?: string
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

export class TimeSeriesData {
    timestamp?: string;
    messageContent?: string;
    id?: number;

    constructor(init?: Partial<TimeSeriesData>) {
        Object.assign(this, init);
    }
}
