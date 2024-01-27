export class NoJwtError extends Error {
//override message: string = "No JWT provided";
  customMessage: string = "No JWT provided";
  constructor(string = "No JWT provided") {
    super(string);
    this.customMessage = string;
  }
}
