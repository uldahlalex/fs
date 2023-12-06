import {Component, Inject, inject, signal} from "@angular/core";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule} from "@angular/forms";
import {NgIf} from "@angular/common";
import {WebSocketClientService} from "../services/service.websocketclient";
import {ClientWantsToRegister} from "../models/clientWantsToRegister";
import {ClientWantsToAuthenticate} from "../models/clientWantsToAuthenticate";
import {API_SERVICE_TOKEN} from "../../main";

@Component({
template: `
      <div *ngIf="showLogin"
           style="height: calc(20% + 100px); width: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center;">
          <input [formControl]="loginEmail" placeholder="email">
          <input type="password" [formControl]="loginPassword" placeholder="password">
          <div style="display: flex; flex-direction: row">
              <button (click)="toggleRegisterLogin()">Not signed
                  up?
              </button>
              <button (click)="login()">Log in!</button>
          </div>
      </div>

      <div *ngIf="!showLogin"
           style="height: calc(20% + 100px); width: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center;">
          <input [formControl]="registerEmail" placeholder="email">
          <input type="password" [formControl]="registerPassword" placeholder="password">
          <input type="password" [formControl]="registerPasswordRepeat" placeholder="password">
          <div style="display: flex; flex-direction: row">
              <button (click)="toggleRegisterLogin()">Already
                  signed up?
              </button>
              <button (click)="register()">Register!</button>
          </div>
      </div>

  `
})
export class ComponentLogin {
  registerEmail = new FormControl('');
  registerPassword = new FormControl('');
  registerPasswordRepeat = new FormControl('');
  constructor(
    @Inject(API_SERVICE_TOKEN)private apiCallService: WebSocketClientService
  ) {}

  loginEmail = new FormControl('');
  loginPassword = new FormControl('');

  loginForm = new FormGroup({
    email: this.loginEmail,
    password: this.loginPassword
  })

  registerForm = new FormGroup({
    email: this.registerEmail,
    password: this.registerPassword,
  })

  showLogin: boolean = true;
  toggleRegisterLogin() {
    this.showLogin = !this.showLogin;
  }
  login() {
    this.apiCallService.ClientWantsToAuthenticate(new ClientWantsToAuthenticate(this.loginForm.value as ClientWantsToAuthenticate));
  }
  register() {
      this.apiCallService.ClientWantsToRegister(new ClientWantsToRegister(this.registerForm.value as ClientWantsToRegister));
  }
}
