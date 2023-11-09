import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import {FormsModule} from "@angular/forms";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, FormsModule],
  templateUrl: 'app.component.html',
})
export class AppComponent {


  items: number[] = [];
  input: any;

  pushToItems() {
    this.items.push(this.input);
  }
}
