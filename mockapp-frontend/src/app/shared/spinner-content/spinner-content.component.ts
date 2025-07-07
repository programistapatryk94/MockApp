import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-spinner-content',
  templateUrl: './spinner-content.component.html',
  styleUrls: ['./spinner-content.component.scss'],
  imports: [MatProgressSpinnerModule, CommonModule],
  standalone: true
})
export class SpinnerContentComponent {
  @Input() loading = false;

  constructor() {}
}
