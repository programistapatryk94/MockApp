import { Component, OnInit } from '@angular/core';
import { CreateUserInput } from '../../../helpers/auth.model';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TypedFormControls } from '../../../helpers/form';
import { AuthService } from '../../../helpers/auth.service';
import { finalize } from 'rxjs';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { SnackbarService } from '../../shared/snackbar/snackbar.service';
import { RouterModule } from '@angular/router';
import { SpinnerContentComponent } from '../../shared/spinner-content/spinner-content.component';

type RegisterFormModel = CreateUserInput;

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    RouterModule,
    SpinnerContentComponent,
  ],
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit {
  form!: FormGroup<TypedFormControls<RegisterFormModel>>;
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
    private auth: AuthService,
    private snackbarService: SnackbarService
  ) {}

  submit() {
    if (this.saving) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAsTouched();
      return;
    }

    this.saving = true;

    const dto: CreateUserInput = this.form.getRawValue();

    this.auth
      .register(dto)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        error: (err) =>
          this.snackbarService.show({
            message: err.error ?? 'Wystąpił błąd podczas rejestracji',
            type: 'error',
          }),
      });
  }

  ngOnInit() {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    }) as FormGroup<TypedFormControls<RegisterFormModel>>;
  }
}
