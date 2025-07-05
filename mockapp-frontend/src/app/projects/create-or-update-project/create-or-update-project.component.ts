import { Component, Inject } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { TypedFormControls } from '../../../helpers/form';
import { noWhitespaceValidator } from '../../../validators/no-whitespace.validator';
import {
  CreateProjectInput,
  Project,
} from '../../../services/models/project.model';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';

interface CreateOrUpdateProjectFormModel {
  name: string;
  apiPrefix: string;
}

export interface CreateOrUpdateProjectDialogData {
  project?: CreateProjectInput | Project;
}

@Component({
  selector: 'app-create-or-update-project',
  templateUrl: './create-or-update-project.component.html',
  styleUrls: ['./create-or-update-project.component.scss'],
  imports: [
    MatDialogModule,
    MatFormFieldModule,
    MatButtonModule,
    CommonModule,
    ReactiveFormsModule,
    MatInputModule,
  ],
})
export class CreateOrUpdateProjectComponent {
  form: FormGroup<TypedFormControls<CreateOrUpdateProjectFormModel>>;
  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<CreateOrUpdateProjectComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CreateOrUpdateProjectDialogData
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, noWhitespaceValidator()]],
      apiPrefix: [''],
    });

    if (this.data?.project) {
      this.form.patchValue(data.project!);
    }
  }

  save() {
    if (this.form.invalid) return;
    const value = this.form.value;

    const output: CreateProjectInput = {
      apiPrefix: value.apiPrefix!,
      name: value.name!,
    };

    this.dialogRef.close(output);
  }

  cancel() {
    this.dialogRef.close();
  }
}
