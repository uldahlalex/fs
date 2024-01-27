import {Injectable} from "@angular/core";
import {NoJwtError} from "../../errors/custom-errors";

@Injectable({
  providedIn: 'root'
})
export class UtilityServices {
  get jwt() {
    const jwt = localStorage.getItem('jwt');
    if (jwt == null || jwt == '')
      throw new NoJwtError();
    return jwt;
  }

}
