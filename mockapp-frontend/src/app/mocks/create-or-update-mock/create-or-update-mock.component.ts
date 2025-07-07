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
  ],
})
export class CreateOrUpdateMockComponent implements OnInit {
  form!: FormGroup<TypedFormControls<CreateOrUpdateMockFormModel>>;
  saving: boolean = false;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<CreateOrUpdateMockComponent>,
    private mockApiService: MockApiService,
    private snackbarService: SnackbarService,
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
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;

    const dto: CreateMockInput = {
      ...this.form.getRawValue(),
      projectId: this.data.projectId ?? '',
    };

    const action$ = this.isEdit
      ? this.mockApiService.update(this.id, dto)
      : this.mockApiService.create(dto);

    action$.pipe(finalize(() => (this.saving = false))).subscribe({
      next: (result) => this.dialogRef.close(result),
      error: () =>
        this.snackbarService.show({
          message: 'Wystąpił błąd przy zapisie',
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
        [Validators.required, noWhitespaceValidator()],
      ],
      method: [this.mock.method ?? 'GET', Validators.required],
      statusCode: [
        this.mock.statusCode ?? 200,
        [Validators.required, Validators.min(100), Validators.max(599)],
      ],
      responseBody: [
        this.mock.responseBody ?? '{}',
        [Validators.required, noWhitespaceValidator()],
      ],
      headersJson: [this.mock.headersJson ?? ''],
      enabled: [this.mock.enabled ?? true, Validators.required],
    }) as FormGroup<TypedFormControls<CreateOrUpdateMockFormModel>>;
  }
}
