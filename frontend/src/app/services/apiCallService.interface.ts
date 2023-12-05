import {ClientWantsToRegister} from "../models/clientWantsToRegister";
import {Injectable} from "@angular/core";

export interface ApiCallServiceInterface {
  ClientWantsToRegister: (clientWantsToRegister: ClientWantsToRegister) => void;
}
