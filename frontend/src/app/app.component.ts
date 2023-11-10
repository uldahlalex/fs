import {Component} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterOutlet} from '@angular/router';
import {FormsModule} from "@angular/forms";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, FormsModule],
  template: `
      <div>
          <input placeholder="insert some number" [(ngModel)]="input">
          <button class="" (click)="pushToItems()">insert</button>
      </div>


      @if (items.length > 1) {
          <div style="display: flex">
              @for (i of items; track i) {
                  <div style="flex-direction: row; order: 2;">here is an item: {{ i }}</div>
              }

          </div>
      }

  `,
})
export class AppComponent {


  items: string[] = ["asdsad", "asdsadd"];
  input: string = "";

  pushToItems() {
    this.items.push(this.input);
  }
}
