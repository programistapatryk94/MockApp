import { Component, OnInit } from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { SubscriptionApiService } from '../../../services/apis/subscription-api.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { SpinnerContentComponent } from '../../shared/spinner-content/spinner-content.component';
import { finalize, forkJoin } from 'rxjs';
import { SnackbarService } from '../../shared/snackbar/snackbar.service';
import { MatButtonModule } from '@angular/material/button';
import { SubscriptionPlanApiService } from '../../../services/apis/subscription-plan-api.service';
import {
  ICurrentSubscriptionInfo,
  ISubscriptionPlanDto,
  ISubscriptionPlanPriceDto,
} from '../../../services/models/subscription-plan.model';
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
  selectedPriceId: string;
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
  currentSubscription: ICurrentSubscriptionInfo | null = null;

  get currentSubscriptionPrice(): ISubscriptionPlanPriceDto | undefined {
    return this.currentSubscription?.prices[0];
  }

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
    private fb: FormBuilder
  ) {}

  cancelSubscription() {
    if (this.saving) return;

    this.saving = true;

    this.subscriptionApiService
      .cancelSubscription()
      .pipe(finalize(() => (this.saving = false)))
      .subscribe(() => {
        this.snackbarService.show({
          message: this.translate.instant(
            'SUBSKRYPCJA_ANULOWANA_Z_KONCEM_OKRESU'
          ),
          type: 'success',
        });

        this.dialogRef.close(true);
      });
  }

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

    const dto: ICreateCheckoutSessionInput = {
      subscriptionPlanPriceId: value.selectedPriceId!,
    };

    this.subscriptionApiService
      .startSubscription(dto)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe((res) => {
        window.location.href = res.url;
      });
  }

  cancel() {
    this.dialogRef.close();
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      selectedPriceId: ['', Validators.required],
    }) as FormGroup<TypedFormControls<SubscriptionFormModel>>;

    this.loading = true;

    forkJoin({
      currentSubscription:
        this.subscriptionPlanApiService.getCurrentSubscription(),
      plans: this.subscriptionPlanApiService.getAll(),
    })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(({ currentSubscription, plans }) => {
        this.currentSubscription = currentSubscription;

        if (currentSubscription) {
          return;
        }

        this.plans = plans;
        var selectedPlanPriceId = this.plans?.[0]?.prices?.[0]?.id ?? null;
        if (selectedPlanPriceId != null) {
          this.form.controls.selectedPriceId.setValue(selectedPlanPriceId);
        }
      });
  }
}
