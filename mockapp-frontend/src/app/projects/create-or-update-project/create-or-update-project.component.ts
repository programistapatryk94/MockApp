import { Component, Inject, OnInit } from '@angular/core';
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
import { ProjectApiService } from '../../../services/apis/project-api.service';
import { finalize } from 'rxjs';
import { SnackbarService } from '../../shared/snackbar/snackbar.service';

type CreateOrUpdateProjectFormModel = {
  name: string;
  apiPrefix: string;
};

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
export class CreateOrUpdateProjectComponent implements OnInit {
  form!: FormGroup<TypedFormControls<CreateOrUpdateProjectFormModel>>;
  saving: boolean = false;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<CreateOrUpdateProjectComponent>,
    private projectApiService: ProjectApiService,
    private snackbarService: SnackbarService,
    @Inject(MAT_DIALOG_DATA) public data: CreateOrUpdateProjectDialogData
  ) {}

  get isEdit(): boolean {
    return !!(this.data.project as Project)?.id;
  }

  get id(): string {
    return (this.data.project as Project)?.id;
  }

  get project(): Partial<Project | CreateProjectInput> {
    return this.data.project ?? {};
  }

  save() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;

    const dto: CreateProjectInput = {
      ...this.form.getRawValue(),
    };

    const action$ = this.isEdit
      ? this.projectApiService.update(this.id, dto)
      : this.projectApiService.create(dto);

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
      name: [
        this.project.name ?? '',
        [Validators.required, noWhitespaceValidator()],
      ],
      apiPrefix: [this.project.apiPrefix ?? ''],
    }) as FormGroup<TypedFormControls<CreateOrUpdateProjectFormModel>>;
  }
}
