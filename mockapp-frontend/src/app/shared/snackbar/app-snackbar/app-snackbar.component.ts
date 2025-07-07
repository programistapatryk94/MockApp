import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import {
  MAT_SNACK_BAR_DATA,
  MatSnackBarRef,
} from '@angular/material/snack-bar';
import { TranslateModule } from '@ngx-translate/core';

export type SnackbarType = 'success' | 'error' | 'warning';

export interface SnackbarOptions {
  message: string;
  type?: SnackbarType;
  duration?: number;
}

@Component({
  selector: 'app-snackbar',
  template: `
    <div class="snackbar-content">
      <mat-icon>{{ icon }}</mat-icon>
      <span class="text">{{ data.message }}</span>
      <button mat-button class="snackbar-close" (click)="close()">
        {{ 'ZAMKNIJ' | translate }}
      </button>
    </div>
  `,
  imports: [MatIconModule, MatButtonModule, TranslateModule],
  styleUrls: ['./app-snackbar.component.scss'],
})
export class AppSnackbarComponent {
  icon = 'info';

  constructor(
    @Inject(MAT_SNACK_BAR_DATA)
    public data: { message: string; type: SnackbarType },
    private snackBarRef: MatSnackBarRef<AppSnackbarComponent>
  ) {
    this.icon = this.getIcon(data.type);
  }

  getIcon(type: SnackbarType): string {
    switch (type) {
      case 'success':
        return 'check_circle';
      case 'error':
        return 'error';
      case 'warning':
        return 'warning';
      default:
        return 'info';
    }
  }

  close() {
    this.snackBarRef.dismiss();
  }
}
