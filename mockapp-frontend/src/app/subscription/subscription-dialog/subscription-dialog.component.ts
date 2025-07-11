import { Component } from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { SubscriptionApiService } from '../../../services/apis/subscription-api.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { SpinnerContentComponent } from '../../shared/spinner-content/spinner-content.component';
import { catchError, EMPTY, finalize } from 'rxjs';
import { SnackbarService } from '../../shared/snackbar/snackbar.service';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-subscription-dialog',
  templateUrl: './subscription-dialog.component.html',
  styleUrls: ['./subscription-dialog.component.scss'],
  imports: [
    MatDialogModule,
    CommonModule,
    SpinnerContentComponent,
    TranslateModule,
    MatButtonModule
  ],
})
export class SubscriptionDialogComponent {
  loading: boolean = false;

  constructor(
    private dialogRef: MatDialogRef<SubscriptionDialogComponent>,
    private subscriptionApiService: SubscriptionApiService,
    private translate: TranslateService,
    private snackbarService: SnackbarService
  ) {}

  subscribe() {
    this.loading = true;
    this.subscriptionApiService
      .startSubscription()
      .pipe(
        catchError((err) => {
          this.snackbarService.show({
            message:
              err.error ??
              this.translate.instant('WYSTAPIL_NIEOCZEKIWANY_BLAD'),
            type: 'error',
          });
          return EMPTY;
        }),
        finalize(() => (this.loading = false))
      )
      .subscribe((res) => {
        window.location.href = res.url;
      });
  }

  cancel() {
    this.dialogRef.close();
  }
}
