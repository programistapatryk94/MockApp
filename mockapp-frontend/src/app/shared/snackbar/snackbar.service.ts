import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import {
  AppSnackbarComponent,
  SnackbarOptions,
} from './app-snackbar/app-snackbar.component';

@Injectable({ providedIn: 'root' })
export class SnackbarService {
  constructor(private snackBar: MatSnackBar) {}

  show(options: SnackbarOptions): void {
    const { message, type = 'success', duration = 3000 } = options;

    this.snackBar.openFromComponent(AppSnackbarComponent, {
      data: { message, type },
      panelClass: [`${type}-snackbar`],
      duration,
    });
  }
}
