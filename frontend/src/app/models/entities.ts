export class Message {
    sender?: number;
    timestamp?: string
    id?: number;
    messageContent?: string;
  email?: string;

    constructor(init?: Partial<Message>) {
        Object.assign(this, init);
    }
}

export class EndUser {
    id?: number;
    email?: string;

    constructor(init?: Partial<EndUser>) {
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

export class TimeSeries {
    timestamp?: string;
    datapoint?: number;
    id?: number;

    constructor(init?: Partial<TimeSeries>) {
        Object.assign(this, init);
    }
}

export class TimeSeriesApexChartData {
  x?: Date;
  y?: number;
}
