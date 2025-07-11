import { Component, Inject, OnInit } from '@angular/core';
import { CreateMockInput, Mock } from '../../../services/models/mock.model';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TypedFormControls } from '../../../helpers/form';
import { noWhitespaceValidator } from '../../../validators/no-whitespace.validator';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MockApiService } from '../../../services/apis/mock-api.service';
import { finalize } from 'rxjs';
import { SnackbarService } from '../../shared/snackbar/snackbar.service';
import { SpinnerContentComponent } from '../../shared/spinner-content/spinner-content.component';
import { TextFieldModule } from '@angular/cdk/text-field';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

type CreateOrUpdateMockFormModel = Omit<CreateMockInput, 'projectId'>;

export interface CreateOrUpdateMockDialogData {
  mock?: CreateMockInput | Mock;
  projectId: string;
}

@Component({
  selector: 'app-create-or-update-mock',
  templateUrl: './create-or-update-mock.component.html',
  styleUrls: ['./create-or-update-mock.component.scss'],
  imports: [
    MatDialogModule,
    MatFormFieldModule,
    MatButtonModule,
    CommonModule,
    ReactiveFormsModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    SpinnerContentComponent,
    TextFieldModule,
    TranslateModule,
  ],
})
export class CreateOrUpdateMockComponent implements OnInit {
  form!: FormGroup<TypedFormControls<CreateOrUpdateMockFormModel>>;
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
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<CreateOrUpdateMockComponent>,
    private mockApiService: MockApiService,
    private snackbarService: SnackbarService,
    private translate: TranslateService,
    @Inject(MAT_DIALOG_DATA) public data: CreateOrUpdateMockDialogData
  ) {}

  get isEdit(): boolean {
    return !!(this.data.mock as Mock)?.id;
  }

  get id(): string {
    return (this.data.mock as Mock)?.id;
  }

  get mock(): Partial<Mock | CreateMockInput> {
    return this.data.mock ?? {};
  }

  save() {
    if (this.saving) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;

    const dto: CreateMockInput = {
      ...this.form.getRawValue(),
      projectId: this.data.projectId ?? '',
    };

    if (dto.urlPath) {
      const path = dto.urlPath?.trim();
      dto.urlPath = path ? (path.startsWith('/') ? path : '/' + path) : '';
    }

    const action$ = this.isEdit
      ? this.mockApiService.update(this.id, dto)
      : this.mockApiService.create(dto);

    action$.pipe(finalize(() => (this.saving = false))).subscribe({
      next: (result) => this.dialogRef.close(result),
      error: (err) =>
        this.snackbarService.show({
          message: err.error ?? this.translate.instant('WYSTPI_BD_PRZY_ZAPISIE'),
          type: 'error',
        }),
    });
  }

  cancel() {
    this.dialogRef.close();
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      urlPath: [
        this.mock.urlPath ?? '',
        [
          Validators.required,
          noWhitespaceValidator(),
          Validators.maxLength(200),
        ],
      ],
      method: [
        this.mock.method ?? 'GET',
        [Validators.required, Validators.maxLength(10)],
      ],
      statusCode: [
        this.mock.statusCode ?? 200,
        [Validators.required, Validators.min(100), Validators.max(599)],
      ],
      responseBody: [
        this.mock.responseBody ?? '{}',
        [Validators.required, noWhitespaceValidator()],
      ],
      headersJson: [this.mock.headersJson ?? '', Validators.maxLength(1000)],
      enabled: [this.mock.enabled ?? true, Validators.required],
    }) as FormGroup<TypedFormControls<CreateOrUpdateMockFormModel>>;
  }
}
