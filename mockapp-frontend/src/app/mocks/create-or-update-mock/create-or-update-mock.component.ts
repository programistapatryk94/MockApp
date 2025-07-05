import { Component, Inject } from '@angular/core';
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

interface CreateOrUpdateMockFormModel {
  urlPath: string;
  method: string;
  statusCode: number;
  responseBody: string;
  headersJson: string | null;
}

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
  ],
})
export class CreateOrUpdateMockComponent {
  form: FormGroup<TypedFormControls<CreateOrUpdateMockFormModel>>;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<CreateOrUpdateMockComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CreateOrUpdateMockDialogData
  ) {
    this.form = this.fb.group({
      urlPath: ['', [Validators.required, noWhitespaceValidator()]],
      method: ['GET', Validators.required],
      statusCode: [200, Validators.required],
      responseBody: ['{}', [Validators.required, noWhitespaceValidator()]],
      headersJson: [''],
    });

    if (this.data?.mock) {
      this.form.patchValue(data.mock!);
    }
  }

  save() {
    if (this.form.invalid) return;
    const value = this.form.value;

    var output: CreateMockInput = {
      method: value.method!,
      projectId: this.data.projectId,
      responseBody: value.responseBody!,
      statusCode: value.statusCode!,
      urlPath: value.urlPath!,
      headersJson: value.headersJson || undefined,
    };

    this.dialogRef.close(output);
  }

  cancel() {
    this.dialogRef.close();
  }
}
