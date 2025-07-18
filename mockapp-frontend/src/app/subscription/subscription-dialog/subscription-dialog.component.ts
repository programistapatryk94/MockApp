import { Component, OnInit } from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { SubscriptionApiService } from '../../../services/apis/subscription-api.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { SpinnerContentComponent } from '../../shared/spinner-content/spinner-content.component';
import { catchError, EMPTY, finalize } from 'rxjs';
import { SnackbarService } from '../../shared/snackbar/snackbar.service';
import { MatButtonModule } from '@angular/material/button';
import { SubscriptionPlanApiService } from '../../../services/apis/subscription-plan-api.service';
import { ISubscriptionPlanDto } from '../../../services/models/subscription-plan.model';
import { LanguageService } from '../../../helpers/language.service';
import { MatRadioModule } from '@angular/material/radio';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TypedFormControls } from '../../../helpers/form';
import { MatIconModule } from '@angular/material/icon';
import { ICreateCheckoutSessionInput } from '../../../services/models/subscription.model';

type SubscriptionFormModel = {
  selectedPlanId: string;
};

@Component({
  selector: 'app-subscription-dialog',
  templateUrl: './subscription-dialog.component.html',
  styleUrls: ['./subscription-dialog.component.scss'],
  imports: [
    MatDialogModule,
    CommonModule,
    SpinnerContentComponent,
    TranslateModule,
    MatButtonModule,
    MatRadioModule,
    ReactiveFormsModule,
    MatIconModule,
  ],
})
export class SubscriptionDialogComponent implements OnInit {
  loading: boolean = false;
  plans: ISubscriptionPlanDto[] = [];
  form!: FormGroup<TypedFormControls<SubscriptionFormModel>>;
  private _saving = false;

  get saving(): boolean {
    return this._saving;
  }

  set saving(value: boolean) {
    this._saving = value;

    if (value) {
      this.form.disable();
    } else {
      this.form.enable();
    }
  }

  constructor(
    private dialogRef: MatDialogRef<SubscriptionDialogComponent>,
    private subscriptionApiService: SubscriptionApiService,
    private translate: TranslateService,
    private snackbarService: SnackbarService,
    private subscriptionPlanApiService: SubscriptionPlanApiService,
    private languageService: LanguageService,
    private fb: FormBuilder
  ) {}

  subscribe() {
    if (this.saving) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;

    const value = this.form.value;

    var selectedPlan = this.plans.find((p) => p.id == value.selectedPlanId)!;

    const dto: ICreateCheckoutSessionInput = {
      subscriptionPlanPriceId: selectedPlan.prices[0].id,
    };

    this.subscriptionApiService
      .startSubscription(dto)
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
        finalize(() => (this.saving = false))
      )
      .subscribe({
        next: (res) => {
          window.location.href = res.url;
        },
        error: (err) => {
          this.snackbarService.show({
            message:
              err.error ??
              this.translate.instant('WYSTAPIL_NIEOCZEKIWANY_BLAD'),
            type: 'error',
          });
        },
      });
  }

  cancel() {
    this.dialogRef.close();
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      selectedPlanId: ['', Validators.required],
    }) as FormGroup<TypedFormControls<SubscriptionFormModel>>;

    this.loading = true;
    var currency = this.getCurrency();

    this.subscriptionPlanApiService
      .getAll()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (items) => {
          for (var i = 0; i < items.length; i++) {
            var item = items[i];
            item.prices = item.prices.filter((p) => p.currency == currency);
          }
          this.plans = items;
          var selectedPlanPriceId = this.plans?.[0]?.prices?.[0]?.id ?? null;
          if (selectedPlanPriceId != null) {
            this.form.controls.selectedPlanId.setValue(selectedPlanPriceId);
          }
        },
        error: (err) => {
          this.snackbarService.show({
            message:
              err.error ??
              this.translate.instant(
                'WYSTPI_BD_PODCZAS_POBIERANIA_PLANOW_SUBSKRYPCJI'
              ),
            type: 'error',
          });
        },
      });
  }

  private getCurrency(): string {
    var currentLanguage = this.languageService.currentLanguage;

    switch (currentLanguage.name) {
      case 'pl':
        return 'PLN';
      case 'en':
        return 'USD';
      default:
        throw new Error('Not supported currency');
    }
  }
}
