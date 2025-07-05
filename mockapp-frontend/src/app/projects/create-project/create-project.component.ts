import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TypedFormControls } from '../../../helpers/form';
import { Router } from '@angular/router';
import { noWhitespaceValidator } from '../../../validators/no-whitespace.validator';
import { ProjectApiService } from '../../../services/apis/project-api.service';

interface CreateProjectFormModel {
  name: string;
}

@Component({
  selector: 'app-create-project',
  templateUrl: './create-project.component.html',
  imports: [
    MatCardModule,
    MatFormFieldModule,
    MatButtonModule,
    CommonModule,
    ReactiveFormsModule,
    MatInputModule
  ],
})
export class CreateProjectComponent {
  form: FormGroup<TypedFormControls<CreateProjectFormModel>>;
  error: string = '';
  submitted: boolean = false;

  constructor(
    private fb: FormBuilder,
    private projectApiService: ProjectApiService,
    private router: Router
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, noWhitespaceValidator()]],
    });
  }

  submit() {
    this.submitted = true;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.value;

    this.projectApiService.create({
      name: value.name!,
      apiPrefix: ''
    }).subscribe({
      next: () => this.router.navigate(['/projects']),
      error: (err) => (this.error = 'Błąd przy zapisie projektu'),
    });
  }
}
